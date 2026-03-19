using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;
using MVCApplication.Models.DTOs;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ApprovalsController : Controller
    {
        private readonly IProductAdminService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IVoucherService _voucherService;

        public ApprovalsController(
            IProductAdminService productService,
            ICategoryService categoryService,
            IVoucherService voucherService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _voucherService = voucherService;
        }

        // View model used to render pending approvals list
        public sealed record ApprovalsViewModel(
            PagedResult<ReadProductDto> PendingProducts,
            IReadOnlyList<ReadCategoryDto> PendingCategories,
            IList<VoucherDto> PendingVouchers);

        // GET: /Admin/Approvals
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get pending products (show a reasonable page size for admin review)
            var pendingProducts = await _productService.GetPagedAsync(
                keyword: null,
                status: "Pending",
                userRole: null,
                pageNumber: 1,
                pageSize: 50);

            // Get categories and filter those not yet Active (staff-created might be Pending/Rejected/etc.)
            var allCategories = await _categoryService.GetAllAsync();
            var pendingCategories = allCategories?
                .Where(c => !string.Equals(c.Status, "Active", System.StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<ReadCategoryDto>();

            // Get vouchers and treat inactive vouchers as awaiting approval
            var allVouchers = await _voucherService.GetAllVouchersAsync();
            var pendingVouchers = allVouchers?
                .Where(v => !(v.IsActive ?? false))
                .ToList() ?? new List<VoucherDto>();

            var vm = new ApprovalsViewModel(
                PendingProducts: pendingProducts,
                PendingCategories: pendingCategories,
                PendingVouchers: pendingVouchers);

            return View(vm);
        }

        // ================= Product approvals =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var ok = await _productService.ApproveAsync(id);
            TempData["ToastType"] = ok ? "success" : "error";
            TempData["ToastMessage"] = ok ? "Product approved" : "Failed to approve product";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var ok = await _productService.RejectAsync(id);
            TempData["ToastType"] = ok ? "error" : "error";
            TempData["ToastMessage"] = ok ? "Product rejected" : "Failed to reject product";
            return RedirectToAction(nameof(Index));
        }

        // ================= Category approvals =================
        // Note: ICategoryService exposes UpdateAsync; we update the category status to "Active" or "Rejected".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCategory(int id)
        {
            var dto = new CategoryUpdateDto { Status = "Active" };
            var ok = await _categoryService.UpdateAsync(id, dto);
            TempData["ToastType"] = ok ? "success" : "error";
            TempData["ToastMessage"] = ok ? "Category approved" : "Failed to approve category";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCategory(int id)
        {
            var dto = new CategoryUpdateDto { Status = "Rejected" };
            var ok = await _categoryService.UpdateAsync(id, dto);
            TempData["ToastType"] = ok ? "error" : "error";
            TempData["ToastMessage"] = ok ? "Category rejected" : "Failed to reject category";
            return RedirectToAction(nameof(Index));
        }

        // ================= Voucher approvals =================
        // Voucher admin update endpoints are not present in the current IVoucherService.
        // These actions act as placeholders until an admin-update API / service exists.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveVoucher(int id)
        {
            TempData["ToastType"] = "error";
            TempData["ToastMessage"] = "Voucher approval is not implemented: backend admin API required.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectVoucher(int id)
        {
            TempData["ToastType"] = "error";
            TempData["ToastMessage"] = "Voucher rejection is not implemented: backend admin API required.";
            return RedirectToAction(nameof(Index));
        }
    }
}