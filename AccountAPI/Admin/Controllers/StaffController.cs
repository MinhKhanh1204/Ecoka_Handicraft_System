using AccountAPI.Admin.Services;
using AccountAPI.CustomFormatter;
using Microsoft.AspNetCore.Mvc;
using static AccountAPI.Admin.DTOs.StaffDto;

namespace AccountAPI.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/staffs")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _service;

        public StaffController(IStaffService service)
        {
            _service = service;
        }

        // GET /api/admin/staffs?keyword=...&role=...&status=...&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetStaffs([FromQuery] StaffSearchDto search)
        {
            var result = await _service.GetStaffsAsync(search);
            return Ok(ApiResponse<PagedResult<ReadStaffDto>>.SuccessResponse(result));
        }

        // GET /api/admin/staffs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffDetail(string id)
        {
            var staff = await _service.GetStaffDetailAsync(id);

            if (staff == null)
                return NotFound(ApiResponse<StaffDetailDto>.Fail("Staff not found", 404));

            return Ok(ApiResponse<StaffDetailDto>.SuccessResponse(staff));
        }

        // POST /api/admin/staffs
        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Invalid data", 400));

            var result = await _service.CreateStaffAsync(dto);

            if (!result)
                return BadRequest(ApiResponse<bool>.Fail("Email already exists or role not found", 400));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff created successfully"));
        }

        // PUT /api/admin/staffs
        [HttpPut]
        public async Task<IActionResult> UpdateStaff([FromBody] UpdateStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Invalid data", 400));

            var result = await _service.UpdateStaffAsync(dto);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Staff not found", 404));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff updated successfully"));
        }

        // DELETE /api/admin/staffs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var result = await _service.DeleteStaffAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Staff not found", 404));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff deleted successfully"));
        }
    }
}
