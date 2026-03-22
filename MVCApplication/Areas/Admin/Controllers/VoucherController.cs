using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Hubs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoucherController : Controller
    {
        private readonly IVoucherAdminService _voucherAdminService;
        private const int PageSize = 10;
        private readonly IHubContext<PendingApprovalHub> _hubContext;

        public VoucherController(IVoucherAdminService voucherAdminService, IHubContext<PendingApprovalHub> hubContext)
        {
            _voucherAdminService = voucherAdminService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// View vouchers | Search voucher
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status, string? sortBy, int pageNumber = 1)
        {
            var result = await _voucherAdminService.GetPagedAsync(keyword, status, sortBy, pageNumber, PageSize);
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            return View(result);
        }

        /// <summary>
        /// View voucher detail
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherAdminService.GetByIdAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateVoucherDto { ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)) });
        }

        /// <summary>
        /// Add voucher
        /// Employee/Staff create -> IsActive = false (pending approval)
        /// Admin create -> IsActive = dto.IsActive
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var createdByAdmin = User.IsInRole("Admin");
            var result = await _voucherAdminService.CreateAsync(dto, createdByAdmin);

            if (!result.Success)
            {
                // Show the specific server error message (e.g. "Duplicate voucher code", "Discount > 50%", etc.)
                ModelState.AddModelError("", result.ErrorMessage ?? "Failed to create voucher. Please try again.");
                return View(dto);
            }
            await _hubContext.Clients.All.SendAsync("PendingVoucherCreated", dto);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = createdByAdmin
                ? "Voucher created successfully."
                : "Voucher created successfully and is pending approval.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Approve voucher - Admin only
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _voucherAdminService.ApproveAsync(id);
            if (!result.Success)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = result.ErrorMessage ?? "Failed to approve voucher.";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Voucher approved successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherAdminService.GetByIdAsync(id);
            if (voucher == null) return NotFound();

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
                IsActive = voucher.IsActive ?? true
            };
            ViewBag.VoucherId = id;
            return View(dto);
        }

        /// <summary>
        /// Edit voucher
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateVoucherDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VoucherId = id;
                return View(dto);
            }

            // Preserve the original IsActive status (cannot be changed via edit)
            var existingVoucher = await _voucherAdminService.GetByIdAsync(id);
            if (existingVoucher == null) return NotFound();
            dto.IsActive = existingVoucher.IsActive ?? false;

            var result = await _voucherAdminService.UpdateAsync(id, dto);
            if (!result.Success)
            {
                // Show the specific server error message
                ModelState.AddModelError("", result.ErrorMessage ?? "Failed to update voucher. Please try again.");
                ViewBag.VoucherId = id;
                return View(dto);
            }

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Voucher updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Delete voucher
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _voucherAdminService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = result.ErrorMessage ?? "Failed to delete voucher.";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Voucher deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
