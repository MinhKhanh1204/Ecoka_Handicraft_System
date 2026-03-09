using AccountAPI.CustomFormatter;
using AccountAPI.DTOs;
using AccountAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AccountAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            var result = _service.Login(request);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result));
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer(
        [FromForm] RegisterCustomerRequestDto request)
        {
            await _service.RegisterCustomerAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "Register successfully"));
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("ForgotPasswordPolicy")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _service.ForgotPasswordAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "If an account exists with this email, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("ResetPasswordPolicy")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            await _service.ResetPasswordAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "Password has been reset successfully."));
        }
    }
}
