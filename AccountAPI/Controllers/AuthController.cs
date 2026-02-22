using AccountAPI.CustomFormatter;
using AccountAPI.DTOs;
using AccountAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(ApiResponse<object>.SuccessResponse(null, "Register successfully"));
        }
    }

}
