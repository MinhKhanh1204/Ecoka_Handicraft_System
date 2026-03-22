using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Services;
using ProductAPI.CustomFormatter;

namespace ProductAPI.Admin.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
    //[Authorize(Roles = "Admin,Staff")]
    public class ProductAdminController : ControllerBase
    {
        private readonly IProductAdminService _service;

        public ProductAdminController(IProductAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? keyword,
            [FromQuery] string? status,
            [FromQuery] string? userRole,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPagedAsync(keyword, status, userRole, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ProductListDto>>.SuccessResponse(result));
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(product));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<ProductDetailDto>.Fail(ex.Message, 404));
            }
        }

        // ================= CREATE =================
        [HttpPost]
        //[Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid model state", 400));

            try
            {
                await _service.CreateAsync(dto);
                return StatusCode(201, ApiResponse<string>.SuccessResponse("Created successfully", "Created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        //[Authorize(Roles = "Staff")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid model state", 400));

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(ApiResponse<string>.SuccessResponse("Updated successfully", "Updated successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message, 404));
            }
        }

        // ================= DELETE (INACTIVE) =================
        //[Authorize(Roles = "Admin,Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse("Product inactivated successfully", "Product inactivated successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message, 404));
            }
        }
    }
}