using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MVCApplication.Models;
using MVCApplication.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MVCApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _accountService.ForgotPasswordAsync(model);
            TempData["success"] = "Nếu có tài khoản với email này, liên kết đặt lại mật khẩu đã được gửi.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction(nameof(Login));
            }
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _accountService.ResetPasswordAsync(model);
            if (!success)
            {
                TempData["error"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu gửi lại.";
                return View(model);
            }
            TempData["success"] = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.LoginAsync(model);

            if (!result.Success)
            {
                TempData["error"] = result.Message;
                return View(model);
            }

            // Store JWT in HttpOnly
            Response.Cookies.Append("AccessToken",
                result.Data.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

            return RedirectToAction(nameof(RedirectByRole));
        }

		[Authorize]
		public IActionResult RedirectByRole()
		{
			if (User.IsInRole("Admin"))
				return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

			if (User.IsInRole("Employee"))
				return RedirectToAction("Index", "Dashboard", new { area = "Employee" });

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.RegisterAsync(model);

            if (!result.Success)
            {
                TempData["error"] = result.Message;
                return View(model);
            }

            TempData["success"] = result.Message;

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.ChangePasswordAsync(model);

            if (!result.Success)
            {
                TempData["error"] = result.Message;
                return View(model);
            }

            TempData["success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("RedirectByRole");
        }

        // =========================
        // VIEW PROFILE
        // =========================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await _accountService.GetProfileAsync();
            return View(profile);
        }

        // =========================
        // UPDATE PROFILE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile? Avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.UpdateProfileAsync(model, Avatar);

            if (!result.Success)
            {
                TempData["error"] = result.Message;
                return View(model);
            }

            TempData["success"] = result.Message;
            return RedirectToAction("Profile");
        }

        // ===== GOOGLE LOGIN =====
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            var loginResult = await _accountService.LoginSocialAsync(email);
            var token = loginResult.Data.AccessToken;

            // lưu JWT để gọi API
            Response.Cookies.Append("AccessToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            return RedirectToAction("Index", "Home");
        }

        // ===== FACEBOOK LOGIN =====
        public IActionResult LoginFacebook()
        {
            var redirectUrl = Url.Action("FacebookResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            var LoginResult = await _accountService.LoginSocialAsync(email);

            // Store JWT in HttpOnly
            Response.Cookies.Append("AccessToken",
                LoginResult.Data.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

            return RedirectToAction("Index", "Home");
        }
    }
}
