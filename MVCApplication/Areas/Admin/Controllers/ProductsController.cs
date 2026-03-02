using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductAdminService _service;
        private readonly ICategoryAdminService _categoryService;

        public ProductsController(IProductAdminService service, ICategoryAdminService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetAllAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var products = await _service.SearchAsync(keyword);
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return Json(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Json(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.CreateAsync(dto);
            if (!success)
                return BadRequest(new { message = "Failed to create product." });

            return Ok(new { message = "Created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.UpdateAsync(id, dto);
            if (!success)
                return BadRequest(new { message = "Failed to update product." });

            return Ok(new { message = "Updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            var success = await _service.ApproveAsync(id);
            if (!success)
                return BadRequest(new { message = "Failed to approve product." });

            return Ok(new { message = "Approved successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            var success = await _service.RejectAsync(id);
            if (!success)
                return BadRequest(new { message = "Failed to reject product." });

            return Ok(new { message = "Rejected successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "Failed to delete product." });

            return Ok(new { message = "Product inactivated successfully." });
        }
    }
}
