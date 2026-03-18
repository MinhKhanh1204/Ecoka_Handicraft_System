using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffsController : Controller
    {
        private readonly IStaffAdminService _staffService;

        public StaffsController(IStaffAdminService staffService)
        {
            _staffService = staffService;
        }

        // GET: /Admin/Staffs
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? role, bool? status, int page = 1)
        {
            var result = await _staffService.GetStaffsAsync(keyword, role, status, page, 10);

            ViewBag.Keyword = keyword;
            ViewBag.Role = role;
            ViewBag.Status = status;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalItems / 10.0);
            ViewBag.TotalStaffs = result.TotalItems;

            return View(result.Items);
        }

        // GET: /Admin/Staffs/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Staff ID is required");

            var staff = await _staffService.GetStaffDetailAsync(id);
            if (staff == null)
                return NotFound();

            return View(staff);
        }

        // GET: /Admin/Staffs/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateStaffViewModel());
        }

        // POST: /Admin/Staffs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _staffService.CreateStaffAsync(model);
            if (!success)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = errorMessage ?? "Failed to create staff";
                return View(model);
            }

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Staff created successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Staffs/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Staff ID is required");

            var staff = await _staffService.GetStaffDetailAsync(id);
            if (staff == null)
                return NotFound();

            var model = new EditStaffViewModel
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Email = staff.Email,
                Phone = staff.Phone,
                Address = staff.Address,
                Gender = staff.Gender,
                CitizenId = staff.CitizenId,
                DateOfBirth = staff.DateOfBirth,
                Status = staff.Status
            };

            return View(model);
        }

        // POST: /Admin/Staffs/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _staffService.UpdateStaffAsync(model);
            if (!result)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to update staff";
                return View(model);
            }

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Staff updated successfully";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Staffs/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Staff ID is required");

            var result = await _staffService.DeleteStaffAsync(id);
            if (!result)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to delete staff";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Staff deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
