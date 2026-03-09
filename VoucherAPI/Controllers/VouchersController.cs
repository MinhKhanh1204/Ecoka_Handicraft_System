using Microsoft.AspNetCore.Mvc;
using VoucherAPI.Services;
using VoucherAPI.DTOs;
using VoucherAPI.CustomFormatter;

namespace VoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // GET: api/vouchers
        [HttpGet]
        public async Task<IActionResult> GetVouchers()
        {
            var vouchers = await _voucherService.GetAllVouchersAsync();

            return Ok(ApiResponse<IEnumerable<VoucherDto>>.SuccessResponse(
                vouchers,
                "Get voucher list successfully"
            ));
        }

        // GET: api/vouchers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVoucher(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);

            if (voucher == null)
            {
                return NotFound(
                    ApiResponse<VoucherDto>.Fail("Voucher not found", StatusCodes.Status404NotFound)
                );
            }

            return Ok(ApiResponse<VoucherDto>.SuccessResponse(
                voucher,
                "Get voucher successfully"
            ));
        }
    }
}
