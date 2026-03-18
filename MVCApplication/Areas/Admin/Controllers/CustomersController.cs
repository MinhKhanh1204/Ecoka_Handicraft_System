using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private const int PageSize = 10;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: /Admin/Customers
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status, string? sortBy, int page = 1)
        {
            // Lấy danh sách từ API (đã có filter keyword/status)
            var customers = await _customerService.SearchAsync(keyword, status);
            
            // Lưu lại các tham số filter để hiển thị trên view
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;

            // Áp dụng sắp xếp theo ID (chỉ id_asc và id_desc)
            var sortedCustomers = sortBy switch
            {
                "id_desc" => customers.OrderByDescending(c => c.CustomerID),
                _ => customers.OrderBy(c => c.CustomerID) // default & id_asc: ID thấp -> cao
            };

            // Tính phân trang
            var totalCustomers = sortedCustomers.Count();
            var totalPages = (int)Math.Ceiling(totalCustomers / (double)PageSize);
            
            // Đảm bảo page hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedCustomers = sortedCustomers
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCustomers = totalCustomers;

            return View(pagedCustomers);
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
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to update status";
            }
            else
            {
                // Active = xanh lá, Inactive = đỏ
                TempData["ToastType"] = status == "Active" ? "success" : "error";
                TempData["ToastMessage"] = status == "Active" 
                    ? $"Customer activated successfully" 
                    : $"Customer deactivated successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
