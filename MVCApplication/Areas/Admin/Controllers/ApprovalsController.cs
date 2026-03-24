using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Hubs;
using MVCApplication.Models.DTOs;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ApprovalsController : Controller
    {
        private readonly IProductAdminService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IVoucherService _voucherService;
        private readonly IVoucherAdminService _voucherAdminService;
        private readonly IHubContext<PendingApprovalHub> _pendingHub;
        private readonly IHubContext<ProductHub> _productHub;
        private readonly IHubContext<CategoryHub> _categoryHub;
        private readonly IHubContext<VoucherHub> _voucherHub;

        public ApprovalsController(
            IProductAdminService productService,
            ICategoryService categoryService,
            IVoucherService voucherService,
            IVoucherAdminService voucherAdminService,
            IHubContext<PendingApprovalHub> pendingHub,
            IHubContext<ProductHub> productHub,
            IHubContext<CategoryHub> categoryHub,
            IHubContext<VoucherHub> voucherHub)
        {
            _productService = productService;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _voucherAdminService = voucherAdminService;
            _pendingHub = pendingHub;
            _productHub = productHub;
            _categoryHub = categoryHub;
            _voucherHub = voucherHub;
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

        private static bool IsAjaxRequest(HttpRequest request) =>
            string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        private IActionResult ApprovalPostResult(bool success, string message, object? jsonPayload = null)
        {
            if (IsAjaxRequest(Request))
            {
                return Json(jsonPayload ?? new { success, message });
            }

            TempData["ToastType"] = success ? "success" : "error";
            TempData["ToastMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ApprovalPostResult(false, "Mã sản phẩm không hợp lệ.",
                    new { success = false, message = "Invalid product id", id });
            }

            var ok = await _productService.ApproveAsync(id);

            if (ok)
            {
                await _pendingHub.Clients.All.SendAsync("ProductApproved", new
                {
                    productId = id
                });

                await _productHub.Clients.All.SendAsync("ProductApprovalStatusChanged", new
                {
                    productId = id,
                    status = "Active"
                });
            }

            var msg = ok ? "Đã duyệt sản phẩm." : "Duyệt sản phẩm thất bại.";
            return ApprovalPostResult(ok, msg, new
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
            {
                return ApprovalPostResult(false, "Mã sản phẩm không hợp lệ.",
                    new { success = false, message = "Invalid product id", id });
            }

            var ok = await _productService.RejectAsync(id);

            if (ok)
            {
                await _pendingHub.Clients.All.SendAsync("ProductRejected", new
                {
                    productId = id
                });

                await _productHub.Clients.All.SendAsync("ProductApprovalStatusChanged", new
                {
                    productId = id,
                    status = "Rejected"
                });
            }

            var msg = ok ? "Đã từ chối sản phẩm." : "Từ chối sản phẩm thất bại.";
            return ApprovalPostResult(ok, msg, new
            {
                success = ok,
                message = ok ? "Product rejected successfully" : "Failed to reject product",
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCategory(int id)
        {
            var ok = await _categoryService.ApproveAsync(id);

            if (ok)
            {
                await _pendingHub.Clients.All.SendAsync("CategoryApproved", new
                {
                    categoryId = id
                }); await _categoryHub.Clients.All.SendAsync("CategoryApprovalStatusChanged", new
                {
                    categoryId = id,
                    status = "Active"
                });
            }

            var msg = ok ? "Đã duyệt danh mục." : "Duyệt danh mục thất bại.";
            return ApprovalPostResult(ok, msg, new
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
            var ok = await _categoryService.RejectAsync(id);

            if (ok)
            {
                await _pendingHub.Clients.All.SendAsync("CategoryRejected", new
                {
                    categoryId = id
                });

                await _categoryHub.Clients.All.SendAsync("CategoryApprovalStatusChanged", new
                {
                    categoryId = id,
                    status = "Rejected"
                });
            }

            var msg = ok ? "Đã từ chối danh mục." : "Từ chối danh mục thất bại.";
            return ApprovalPostResult(ok, msg, new
            {
                success = ok,
                message = ok ? "Category rejected successfully" : "Failed to reject category",
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVoucher(int id)
        {
            var result = await _voucherAdminService.ApproveAsync(id);

            if (result.Success)
            {
                await _pendingHub.Clients.All.SendAsync("VoucherApproved", new
                {
                    voucherId = id
                });

                await _voucherHub.Clients.All.SendAsync("VoucherApprovalStatusChanged", new
                {
                    voucherId = id,
                    status = "Active"
                });
            }

            var msg = result.Success
                ? "Đã duyệt voucher."
                : (result.ErrorMessage ?? "Duyệt voucher thất bại.");
            return ApprovalPostResult(result.Success, msg, new
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
            var result = await _voucherAdminService.DeleteAsync(id);

            if (result.Success)
            {
                await _pendingHub.Clients.All.SendAsync("VoucherRejected", new
                {
                    voucherId = id
                }); await _voucherHub.Clients.All.SendAsync("VoucherApprovalStatusChanged", new
                {
                    voucherId = id,
                    status = "Rejected"
                });
            }

            var msg = result.Success
                ? "Đã từ chối voucher."
                : (result.ErrorMessage ?? "Từ chối voucher thất bại.");
            return ApprovalPostResult(result.Success, msg, new
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