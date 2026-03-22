using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Models.DTOs;
using MVCApplication.Services;
using MVCApplication.Hubs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ApprovalsController : Controller
    {
        private readonly IProductAdminService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IVoucherService _voucherService;
        private readonly IVoucherAdminService _voucherAdminService;
        private readonly IHubContext<PendingApprovalHub> _hubContext;

        public ApprovalsController(
            IProductAdminService productService,
            ICategoryService categoryService,
            IVoucherService voucherService,
            IVoucherAdminService voucherAdminService,
            IHubContext<PendingApprovalHub> hubContext)
        {
            _productService = productService;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _voucherAdminService = voucherAdminService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allCategories = await _categoryService.GetAllAsync();
            var pendingCategories = allCategories?
                .Where(c => string.Equals(c.Status, "Pending", System.StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<ReadCategoryDto>();

            var vm = new ApprovalDto
            {
                PendingProducts = await _productService.GetPagedAsync(
                    keyword: null,
                    status: "Pending",
                    pageNumber: 1,
                    pageSize: 50),
                PendingCategories = pendingCategories,
                PendingVouchers = (await _voucherService.GetAllVouchersAsync())
                    ?.Where(x => !(x.IsActive ?? false))
                    .ToList() ?? new List<VoucherDto>()
            };

            return View(vm);
        }

        // ================= PRODUCT =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { success = false, message = "Invalid product id" });

            var ok = await _productService.ApproveAsync(id);

            return Json(new
            {
                success = ok,
                message = ok ? "Product approved successfully" : "Failed to approve product",
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { success = false, message = "Invalid product id" });

            var ok = await _productService.RejectAsync(id);

            return Json(new
            {
                success = ok,
                message = ok ? "Product rejected successfully" : "Failed to reject product",
                id
            });
        }

        // ================= CATEGORY =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCategory(int id)
        {
            var ok = await _categoryService.ApproveAsync(id);
            TempData["ToastType"] = ok ? "success" : "error";
            TempData["ToastMessage"] = ok ? "Category approved" : "Failed to approve category";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCategory(int id)
        {
            var ok = await _categoryService.RejectAsync(id);
            TempData["ToastType"] = ok ? "error" : "error";
            TempData["ToastMessage"] = ok ? "Category rejected" : "Failed to reject category";
            return RedirectToAction(nameof(Index));
        }

        // ================= VOUCHER =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVoucher(int id)
        {
            var result = await _voucherAdminService.ApproveAsync(id);

            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("VoucherApproved", new { voucherId = id });
            }

            return Json(new
            {
                success = result.Success,
                message = result.Success
                    ? "Voucher approved successfully"
                    : (result.ErrorMessage ?? "Failed to approve voucher"),
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVoucher(int id)
        {
            // Get voucher to check if it exists, then mark inactive via Update
            var voucher = await _voucherAdminService.GetByIdAsync(id);
            if (voucher == null)
            {
                return Json(new { success = false, message = "Voucher not found", id });
            }

            var dto = new UpdateVoucherDto
            {
                VoucherName = voucher.VoucherName ?? "",
                Description = voucher.Description,
                DiscountPercentage = voucher.DiscountPercentage ?? 0,
                MaxReducing = voucher.MaxReducing,
                Quantity = voucher.Quantity ?? 0,
                ExpiryDate = voucher.ExpiryDate ?? DateOnly.FromDateTime(DateTime.Today),
                MinOrderValue = voucher.MinOrderValue,
                MaxUsagePerUser = voucher.MaxUsagePerUser,
                IsActive = false
            };

            var result = await _voucherAdminService.UpdateAsync(id, dto);

            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("VoucherRejected", new { voucherId = id });
            }

            return Json(new
            {
                success = result.Success,
                message = result.Success
                    ? "Voucher rejected successfully"
                    : (result.ErrorMessage ?? "Failed to reject voucher"),
                id
            });
        }
    }
}
