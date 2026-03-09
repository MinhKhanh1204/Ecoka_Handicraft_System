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
    }
}
