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
                return NotFound(ApiResponse<StaffDetailDto>.Fail("Không tìm thấy nhân viên", 404));

            return Ok(ApiResponse<StaffDetailDto>.SuccessResponse(staff));
        }

        // POST /api/admin/staffs
        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<bool>.Fail("Nội dung yêu cầu trống", 400));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Dữ liệu không hợp lệ", 400));

            try
            {
                await _service.CreateStaffAsync(dto);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Tạo nhân viên thành công"));
            }
            catch (InvalidOperationException ex) when (ex.Message == "EMAIL_EXISTS")
            {
                return BadRequest(ApiResponse<bool>.Fail("Email đã tồn tại", 400));
            }
            catch (InvalidOperationException ex) when (ex.Message == "PHONE_EXISTS")
            {
                return BadRequest(ApiResponse<bool>.Fail("Số điện thoại đã tồn tại", 400));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CITIZENID_EXISTS")
            {
                return BadRequest(ApiResponse<bool>.Fail("CCCD đã tồn tại", 400));
            }
            catch (InvalidOperationException ex) when (ex.Message == "ROLE_NOT_FOUND")
            {
                return BadRequest(ApiResponse<bool>.Fail("Không tìm thấy vai trò", 400));
            }
            catch (InvalidOperationException ex) when (ex.Message == "INVALID_AGE")
            {
                return BadRequest(ApiResponse<bool>.Fail("Nhân viên phải từ 18 tuổi trở lên", 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Lỗi hệ thống: " + ex.Message, 500));
            }
        }

        // PUT /api/admin/nhan-vien/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(string id, [FromBody] UpdateStaffDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

                return BadRequest(ApiResponse<bool>.Fail(string.Join("; ", errors), 400));
            }

            // So sánh id trên URL và dto
            if (id != dto.StaffId)
                return BadRequest(ApiResponse<bool>.Fail("ID nhân viên không khớp", 400));

            try
            {
                var result = await _service.UpdateStaffAsync(dto);

                if (!result)
                {
                    return NotFound(ApiResponse<bool>.Fail("Không tìm thấy nhân viên", 404));
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật nhân viên thành công"));
            }
            catch (InvalidOperationException ex)
            {
                string message = ex.Message switch
                {
                    "INVALID_AGE" => "Nhân viên phải từ 18 tuổi trở lên",
                    "CITIZENID_EXISTS" => "CCCD đã tồn tại cho nhân viên khác",
                    _ => "Đã xảy ra lỗi không xác định"
                };

                return BadRequest(ApiResponse<bool>.Fail(message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Lỗi hệ thống: " + ex.Message, 500));
            }
        }

        // DELETE /api/admin/nhan-vien/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var result = await _service.DeleteStaffAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Không tìm thấy nhân viên", 404));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Xóa nhân viên thành công"));
        }
    }
}