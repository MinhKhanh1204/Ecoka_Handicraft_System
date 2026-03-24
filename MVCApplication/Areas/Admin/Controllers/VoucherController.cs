using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Hubs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class VoucherController : Controller
    {
        private readonly IVoucherAdminService _voucherAdminService;
        private const int PageSize = 10;
        private readonly IHubContext<PendingApprovalHub> _pendingHub;
        private readonly IHubContext<VoucherHub> _voucherHub;

        public VoucherController(
            IVoucherAdminService voucherAdminService,
            IHubContext<PendingApprovalHub> pendingHub,
            IHubContext<VoucherHub> voucherHub)
        {
            _voucherAdminService = voucherAdminService;
            _pendingHub = pendingHub;
            _voucherHub = voucherHub;
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
                ModelState.AddModelError("", result.ErrorMessage ?? "Tao voucher that bai. Vui long thu lai.");
                return View(dto);
            }

            // Tim voucher vua tao de gui du lieu realtime
            var paged = await _voucherAdminService.GetPagedAsync(dto.VoucherName, null, "id_desc", 1, 10);
            var createdVoucher = paged.Items.FirstOrDefault(v =>
                string.Equals(v.VoucherName, dto.VoucherName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(v.Code, dto.Code, StringComparison.OrdinalIgnoreCase));

            if (createdVoucher != null && createdVoucher.IsActive != true)
            {
                await _pendingHub.Clients.All.SendAsync("PendingVoucherCreated", new
                {
                    voucherId = createdVoucher.VoucherId,
                    voucherName = createdVoucher.VoucherName,
                    code = createdVoucher.Code,
                    discountPercentage = createdVoucher.DiscountPercentage,
                    maxReducing = createdVoucher.MaxReducing,
                    quantity = createdVoucher.Quantity,
                    expiryDate = createdVoucher.ExpiryDate,
                    isActive = createdVoucher.IsActive
                });
            }

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = createdByAdmin
                ? "Tao voucher thanh cong."
                : "Tao voucher thanh cong. Voucher dang cho phep duyet.";

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
                ModelState.AddModelError("", result.ErrorMessage ?? "Cap nhat voucher that bai. Vui long thu lai.");
                ViewBag.VoucherId = id;
                return View(dto);
            }

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Cap nhat voucher thanh cong.";
            await _voucherHub.Clients.All.SendAsync("VoucherUpdated", new
            {
                voucherId = id
            });
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
                TempData["ToastMessage"] = result.ErrorMessage ?? "Xoa voucher that bai.";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Xoa voucher thanh cong.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
