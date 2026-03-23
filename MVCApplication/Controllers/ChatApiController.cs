using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MVCApplication.Controllers;

/// <summary>
/// Proxies chat requests to APIGateway so the browser calls same-origin (no cross-port / HTTPS mismatch).
/// Configure destination via ApiGateway:ApiBaseUrl in appsettings.json.
/// </summary>
[ApiController]
[Route("api/chat")]
[AllowAnonymous]
public class ChatApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ChatApiController> _logger;

    public ChatApiController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ChatApiController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task PostMessage(CancellationToken cancellationToken)
    {
        var gatewayBase = _configuration["ApiGateway:ApiBaseUrl"] ?? "https://localhost:5000/";
        if (!gatewayBase.EndsWith('/'))
            gatewayBase += "/";

        var url = new Uri(new Uri(gatewayBase), "chat/message");

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: false);
        var body = await reader.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("[ChatApiController] Proxying POST {Url}, body length={Len}", url, body.Length);

        var client = _httpClientFactory.CreateClient("GatewayProxy");
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("[ChatApiController] Bearer token attached (len={Len})", token.Length);
        }
        else
        {
            _logger.LogInformation("[ChatApiController] No Bearer token in cookie.");
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
            _logger.LogInformation("[ChatApiController] Gateway responded HTTP {Status}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ChatApiController] Could not reach API Gateway at {Url}. Exception: {ExType}: {ExMsg}", url, ex.GetType().Name, ex.Message);
            Response.StatusCode = StatusCodes.Status502BadGateway;
            Response.ContentType = "application/json; charset=utf-8";
            await Response.WriteAsync(
                $"{{\"success\":false,\"message\":\"Không kết nối được tới API Gateway ({ex.GetType().Name}). Hãy chạy APIGateway (port 5000) và ChatbotAPI (port 7143).\",\"statusCode\":502}}",
                cancellationToken);
            return;
        }

        using (response)
        {
            Response.StatusCode = (int)response.StatusCode;
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
            var charset = response.Content.Headers.ContentType?.CharSet;
            Response.ContentType = charset != null ? $"{mediaType}; charset={charset}" : $"{mediaType}; charset=utf-8";
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("[ChatApiController] Gateway body: {Body}", responseBody.Length > 200 ? responseBody[..200] + "..." : responseBody);
            await Response.WriteAsync(responseBody, cancellationToken);
        }
    }
}
