using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services;
using System.Threading.Tasks;

namespace MVCApplication.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllVouchersAsync();
            return View(vouchers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // Endpoint for AJAX Voucher Application
        [HttpGet]
        public async Task<IActionResult> Apply(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Code is required");

            var allVouchers = await _voucherService.GetAllVouchersAsync();
            var matchedVoucher = allVouchers.FirstOrDefault(v => 
                v.Code != null && v.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && 
                v.IsActive == true && 
                (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow)));

            if (matchedVoucher == null)
                return NotFound(new { Message = "Voucher không hợp lệ hoặc đã hết hạn!" });

            return Ok(new 
            { 
                VoucherId = matchedVoucher.VoucherId, 
                Discount = matchedVoucher.DiscountPercentage,
                MaxReducing = matchedVoucher.MaxReducing
            });
        }
    }
}
