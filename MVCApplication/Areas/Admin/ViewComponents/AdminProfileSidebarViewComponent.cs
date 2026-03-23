using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Areas.Admin.ViewModels;

namespace MVCApplication.Areas.Admin.ViewComponents
{
    public class AdminProfileSidebarViewComponent : ViewComponent
    {
        private readonly IStaffAdminService _staffService;

        public AdminProfileSidebarViewComponent(IStaffAdminService staffService)
        {
            _staffService = staffService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var principal = User as ClaimsPrincipal;
            var accountId = principal?.FindFirst("accountID")?.Value;
            var username = principal?.FindFirst("username")?.Value;
            var roles = principal?.FindAll("role").Select(c => c.Value).ToList() ?? new List<string>();

            StaffDetailViewModel? staff = null;
            if (!string.IsNullOrWhiteSpace(accountId))
            {
                try
                {
                    staff = await _staffService.GetStaffDetailAsync(accountId);
                }
                catch
                {
                    // Giữ fallback từ JWT nếu API lỗi
                }
            }

            var vm = new AdminProfileSidebarViewModel
            {
                Staff = staff,
                FallbackUsername = username ?? "",
                FallbackRoles = roles,
                AccountId = accountId
            };

            return View(vm);
        }
    }
}
