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
            TempData["success"] = "If an account exists with this email, a password reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Invalid reset link.";
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
                TempData["error"] = "Invalid or expired reset link. Please request a new one.";
                return View(model);
            }
            TempData["success"] = "Your password has been reset. You can now log in.";
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

			return RedirectToAction("RedirectByRole", "Account");
		}

		[Authorize]
		public IActionResult RedirectByRole()
		{
			if (User.IsInRole("Admin"))
				return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

			if (User.IsInRole("Staff"))
				return RedirectToAction("Index", "Dashboard", new { area = "Staff" });

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
    }
}
