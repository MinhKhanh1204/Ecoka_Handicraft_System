using Microsoft.AspNetCore.Mvc;
using VoucherAPI.Admin.DTOs;
using VoucherAPI.Admin.Services;
using VoucherAPI.CustomFormatter;

namespace VoucherAPI.Admin.Controllers
{
    /// <summary>
    /// Voucher Management API (UC_46 - UC_51)
    /// Staff, Admin only
    /// </summary>
    [ApiController]
    [Route("api/admin/vouchers")]
    //[Authorize(Roles = "Admin,Staff")]
    public class VoucherAdminController : ControllerBase
    {
        private readonly IVoucherAdminService _service;

        public VoucherAdminController(IVoucherAdminService service)
        {
            _service = service;
        }

        /// <summary>
        /// UC_46 View vouchers - List with pagination, sort, filter
        /// UC_47 Search voucher - keyword (name, code, discount rate), status, expiry
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? keyword,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPagedAsync(keyword, status, sortBy, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VoucherListDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// UC_51 View voucher - Detailed info
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var voucher = await _service.GetByIdAsync(id);
            if (voucher == null)
                return NotFound(ApiResponse<VoucherDetailDto>.Fail("Voucher not found", 404));
            return Ok(ApiResponse<VoucherDetailDto>.SuccessResponse(voucher));
        }

        /// <summary>
        /// UC_48 Add voucher
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid model state", 400));

            try
            {
                var id = await _service.CreateAsync(dto);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(
                    new { VoucherId = id },
                    "Voucher created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// UC_49 Edit voucher
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVoucherDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid model state", 400));

            try
            {
                var success = await _service.UpdateAsync(id, dto);
                if (!success)
                    return NotFound(ApiResponse<string>.Fail("Voucher not found", 404));
                return Ok(ApiResponse<string>.SuccessResponse("Voucher updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// UC_50 Delete voucher
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<string>.Fail("Voucher not found", 404));
            return Ok(ApiResponse<string>.SuccessResponse("Voucher deleted successfully"));
        }
    }
}
