using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ChatbotAPI.CustomFormatter;
using ChatbotAPI.DTOs;
using ChatbotAPI.Exceptions;
using ChatbotAPI.Services;

namespace ChatbotAPI.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IContextBuilderService _contextBuilderService;
    private readonly IPromptBuilderService _promptBuilderService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IGeminiService geminiService,
        IContextBuilderService contextBuilderService,
        IPromptBuilderService promptBuilderService,
        IWebHostEnvironment environment,
        ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _contextBuilderService = contextBuilderService;
        _promptBuilderService = promptBuilderService;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
    {
        // Extract CustomerId from verified JWT — never trust request body for auth-related lookups.
        var jwtAccountId = User.FindFirst("accountID")?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Authoritative CustomerId: JWT claim wins; fallback to request body only for unauthenticated / no-claim paths.
        var customerId = !string.IsNullOrWhiteSpace(jwtAccountId) ? jwtAccountId : request.CustomerId;

        // Sanitize CustomerName before inserting into prompt to reduce prompt-injection risk.
        var customerName = SanitizePromptText(request.CustomerName, maxLength: 60);

        _logger.LogInformation("[ChatController] POST /api/chat/message received. Msg='{Msg}', SessionId={SessionId}, CustomerId={CustomerId}, FromJwt={FromJwt}",
            request.Message, request.SessionId, customerId, !string.IsNullOrWhiteSpace(jwtAccountId));

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("[ChatController] Empty message rejected.");
            return BadRequest(ApiResponse<ChatResponseDto>.Fail("Message cannot be empty", 400));
        }

        try
        {
            _logger.LogInformation("[ChatController] Building context for customer {CustomerId}...", customerId);
            var context = await _contextBuilderService.BuildContextAsync(customerId);
            _logger.LogInformation("[ChatController] Context built. CartItems={CartCount}, Orders={OrderCount}, Products={ProductCount}, Vouchers={VoucherCount}",
                context.CartItems.Count,
                context.RecentOrders.Count,
                context.RecommendedProducts.Count,
                context.AvailableVouchers.Count);

            var systemPrompt = _promptBuilderService.BuildSystemPrompt(context, customerName);
            _logger.LogInformation("[ChatController] System prompt length={Len}", systemPrompt.Length);

            _logger.LogInformation("[ChatController] Calling Gemini...");
            var aiResponse = await _geminiService.GenerateResponseAsync(request.Message, systemPrompt);
            _logger.LogInformation("[ChatController] Gemini returned. Response length={Len}", aiResponse.Length);

            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

            var response = new ChatResponseDto
            {
                Response = aiResponse,
                SessionId = sessionId,
                Timestamp = DateTime.Now,
                SuggestedActions = GetSuggestedActions(request.Message),
                FallbackUsed = false
            };

            _logger.LogInformation("[ChatController] Returning 200 OK. SessionId={SessionId}", sessionId);
            return Ok(ApiResponse<ChatResponseDto>.SuccessResponse(response, "Chat response generated successfully"));
        }
        catch (GeminiAccessDeniedException ex)
        {
            _logger.LogWarning(ex, "[ChatController] Gemini access denied: {Msg}", ex.Message);
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<ChatResponseDto>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
        catch (GeminiRateLimitException ex)
        {
            _logger.LogWarning(ex, "[ChatController] Gemini rate limit: {Msg}", ex.Message);
            return StatusCode(StatusCodes.Status429TooManyRequests,
                ApiResponse<ChatResponseDto>.Fail(ex.Message, StatusCodes.Status429TooManyRequests));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ChatController] Gemini config error: {Msg}", ex.Message);
            return StatusCode(500, ApiResponse<ChatResponseDto>.Fail($"Gemini config error: {ex.Message}", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ChatController] Unexpected error processing message");
            return StatusCode(500, ApiResponse<ChatResponseDto>.Fail($"Failed to process message: {ex.Message}", 500));
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ChatbotAPI", timestamp = DateTime.Now });
    }

    /// <summary>
    /// Gọi thử Gemini (prompt tối thiểu). Chỉ mở khi Development — xem category / nextStepVi / rawErrorSnippet.
    /// </summary>
    [HttpGet("diagnostics/gemini")]
    [AllowAnonymous]
    public async Task<IActionResult> GeminiDiagnostics(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        _logger.LogInformation("[ChatController] GET /api/chat/diagnostics/gemini (dev only)");
        var r = await _geminiService.TestConnectionAsync(cancellationToken);
        var msg = r.Ok ? "Gemini kết nối OK" : "Gemini lỗi — xem data.category và nextStepVi";
        return Ok(ApiResponse<GeminiConnectionTestDto>.SuccessResponse(r, msg));
    }

    private static List<string>? GetSuggestedActions(string userMessage)
    {
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("mua"))
        {
            return new List<string> { "Xem sản phẩm", "Tìm kiếm sản phẩm", "Sản phẩm khuyến mãi" };
        }
        else if (lowerMessage.Contains("đơn hàng") || lowerMessage.Contains("đơn"))
        {
            return new List<string> { "Kiểm tra đơn hàng", "Xem chi tiết đơn hàng", "Theo dõi đơn hàng" };
        }
        else if (lowerMessage.Contains("giỏ hàng") || lowerMessage.Contains("cart"))
        {
            return new List<string> { "Xem giỏ hàng", "Thanh toán", "Tiếp tục mua sắm" };
        }
        else if (lowerMessage.Contains("voucher") || lowerMessage.Contains("khuyến mãi") || lowerMessage.Contains("giảm giá"))
        {
            return new List<string> { "Xem voucher", "Mã giảm giá", "Ưu đãi hiện tại" };
        }

        return null;
    }

    /// <summary>
    /// Sanitizes user-controlled strings before inserting them into the system prompt.
    /// Removes Markdown/HTML injection markers and enforces a max length.
    /// </summary>
    private static string SanitizePromptText(string? input, int maxLength = 60)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var s = input.Trim();

        s = Regex.Replace(s, @"<\/?[^>]+>", "");           // strip HTML tags
        s = Regex.Replace(s, @"\*+", "");                  // strip Markdown markers
        s = Regex.Replace(s, @"_+", "");                   // strip underscore emphasis
        s = Regex.Replace(s, @"```[\s\S]*?```", "");       // strip code fences
        s = Regex.Replace(s, @"\[([^\]]+)\]\([^)]+\)", "$1"); // [text](url) → text

        s = Regex.Replace(s, @"\s+", " ").Trim();

        if (s.Length > maxLength) s = s[..maxLength] + "…";

        return s;
    }
}
