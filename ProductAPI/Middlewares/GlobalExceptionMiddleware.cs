using ProductAPI.CustomFormatter;
using ProductAPI.Exceptions;
using System.Text.Json;

namespace ProductAPI.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse<object> response = exception switch
            {
                BadRequestException => ApiResponse<object>.Fail(
                    exception.Message, 
                    StatusCodes.Status400BadRequest),

                UnauthorizedException => ApiResponse<object>.Fail(
                    exception.Message,
                    StatusCodes.Status401Unauthorized),

                ForbiddenException => ApiResponse<object>.Fail(
                    exception.Message,
                    StatusCodes.Status403Forbidden),

                NotFoundException => ApiResponse<object>.Fail(
                    exception.Message, 
                    StatusCodes.Status404NotFound),


                _ => ApiResponse<object>.Fail(
                    "Internal server error",
                    StatusCodes.Status500InternalServerError)
            };

            context.Response.StatusCode = response.StatusCode;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
