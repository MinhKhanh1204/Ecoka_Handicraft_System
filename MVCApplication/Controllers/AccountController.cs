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

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.LoginAsync(model);

            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                TempData["error"] = "Email hoặc Password không đúng";
                return View(model);
            }

            // Store JWT in HttpOnly
            Response.Cookies.Append("AccessToken",
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
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

            if (!result)
            {
                TempData["error"] = "Username/Email đã tồn tại hoặc đăng ký thất bại";
                return View(model);
            }

            TempData["success"] = "Đăng ký thành công. Vui lòng đăng nhập.";

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
    }
}
