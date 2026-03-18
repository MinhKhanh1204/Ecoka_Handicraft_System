using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.CustomFormatter;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoucherController : Controller
    {
        private readonly IVoucherAdminService _voucherAdminService;
        private const int PageSize = 10;

        public VoucherController(IVoucherAdminService voucherAdminService)
        {
            _voucherAdminService = voucherAdminService;
        }

        /// <summary>
        /// UC_46 View vouchers | UC_47 Search voucher
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
        /// UC_51 View voucher detail
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
        /// UC_48 Add voucher
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var success = await _voucherAdminService.CreateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to create voucher. Code may already exist.");
                return View(dto);
            }
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Voucher created successfully.";
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
        /// UC_49 Edit voucher
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

            var success = await _voucherAdminService.UpdateAsync(id, dto);
            if (!success)
            {
                ModelState.AddModelError("", "Voucher not found.");
                ViewBag.VoucherId = id;
                return View(dto);
            }
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Voucher updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// UC_50 Delete voucher
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _voucherAdminService.DeleteAsync(id);
            if (!success)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Voucher not found.";
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
