using AccountAPI.Admin.DTOs;
using AccountAPI.Admin.Services;
using AccountAPI.CustomFormatter;
using Microsoft.AspNetCore.Mvc;

namespace AccountAPI.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _service.GetAllAsync();
            return Ok(ApiResponse<List<CustomerDto>>.SuccessResponse(customers));
        }

        // SEARCH
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var customers = await _service.SearchAsync(keyword, status);
            return Ok(ApiResponse<List<CustomerDto>>.SuccessResponse(customers));
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound(ApiResponse<CustomerDto>.Fail("Customer not found", 404));

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer));
        }

        // UPDATE STATUS
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] CustomerStatusDto dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto.Status);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Customer not found", 404));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Status updated successfully"));
        }
    }
}
