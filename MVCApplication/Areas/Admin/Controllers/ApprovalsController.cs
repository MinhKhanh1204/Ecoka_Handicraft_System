using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Models.DTOs;
using MVCApplication.Services;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<CategoryHub> _categoryHubContext;

        public ApprovalsController(
            IProductAdminService productService,
            ICategoryService categoryService,
            IVoucherService voucherService,
            IVoucherAdminService voucherAdminService,
            IHubContext<PendingApprovalHub> hubContext,
            IHubContext<CategoryHub> categoryHubContext)
        {
            _productService = productService;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _voucherAdminService = voucherAdminService;
            _hubContext = hubContext;
            _categoryHubContext = categoryHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new ApprovalDto
            {
                PendingProducts = await _productService.GetPagedAsync(
                    keyword: null,
                    status: "Pending",
                    pageNumber: 1,
                    pageSize: 50),

                PendingCategories = (await _categoryService.GetAllAsync())
                    ?.Where(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    .ToList() ?? new List<ReadCategoryDto>(),

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
            // Use the dedicated ApproveAsync so we don't send a partial CategoryUpdateDto
            var ok = await _categoryService.ApproveAsync(id);

            if (ok)
            {
                await _categoryHubContext.Clients.All.SendAsync("CategoryApprovalStatusChanged", new { categoryId = id, status = "Active" });
            }

            return Json(new
            {
                success = ok,
                message = ok ? "Category approved successfully" : "Failed to approve category",
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCategory(int id)
        {
            // Use the dedicated RejectAsync so we don't send a partial CategoryUpdateDto
            var ok = await _categoryService.RejectAsync(id);

            if (ok)
            {
                await _categoryHubContext.Clients.All.SendAsync("CategoryApprovalStatusChanged", new { categoryId = id, status = "Rejected" });
            }

            return Json(new
            {
                success = ok,
                message = ok ? "Category rejected successfully" : "Failed to reject category",
                id
            });
        }

        // ================= VOUCHER =================
        // Chỉ dùng khi service của bạn đã có update trạng thái voucher

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
