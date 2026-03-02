using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Services;

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

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ================= SEARCH =================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _service.SearchAsync(keyword);
            return Ok(result);
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ================= CREATE =================
        [HttpPost]
        //[Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.CreateAsync(dto);
            return StatusCode(201, "Created successfully");
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        //[Authorize(Roles = "Staff")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok("Updated successfully");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ================= APPROVE =================
        [HttpPut("approve/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(string id)
        {
            try
            {
                await _service.ApproveAsync(id);
                return Ok("Approved successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================= REJECT =================
        [HttpPut("reject/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(string id)
        {
            try
            {
                await _service.RejectAsync(id);
                return Ok("Rejected successfully"); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================= DELETE (INACTIVE) =================
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteAsync(id); 
                return Ok("Product inactivated successfully");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}