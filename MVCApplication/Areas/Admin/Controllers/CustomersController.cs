using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: /Admin/Customers
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status)
        {
            var customers = await _customerService.SearchAsync(keyword, status);
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            return View(customers);
        }

        // GET: /Admin/Customers/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Customer ID is required");

            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();
            return View(customer);
        }

        // POST: /Admin/Customers/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Customer ID is required");

            if (string.IsNullOrWhiteSpace(status) || (status != "Active" && status != "Inactive"))
                return BadRequest("Invalid status value");

            var result = await _customerService.UpdateStatusAsync(id, status);
            if (!result)
            {
                TempData["Error"] = "Failed to update status";
            }
            else
            {
                TempData["Success"] = "Status updated successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
