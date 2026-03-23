using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffsController : Controller
    {
        private readonly IStaffAdminService _staffService;

        public StaffsController(IStaffAdminService staffService)
        {
            _staffService = staffService;
        }

        // GET: /Admin/Staffs
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, bool? status, int page = 1)
        {
            // Only fetch personnel with "Employee" role, hide "Admin"
            var result = await _staffService.GetStaffsAsync(keyword, "Employee", status, page, 10);

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalItems / 10.0);
            ViewBag.TotalStaffs = result.TotalItems;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Index", result.Items);
            }

            return View(result.Items);
        }

        // GET: /Admin/Staffs/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Employee ID is required");

            var staff = await _staffService.GetStaffDetailAsync(id);
            if (staff == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("Details", staff);

            return View(staff);
        }

        // GET: /Admin/Staffs/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("Create", new CreateStaffViewModel());

            return View(new CreateStaffViewModel());
        }

        // POST: /Admin/Staffs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            // ✅ Validate tuổi (server-side)
            if (model.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - model.DateOfBirth.Value.Year;

                if (model.DateOfBirth.Value > today.AddYears(-age))
                    age--;

                if (age < 18)
                {
                    ModelState.AddModelError("DateOfBirth", "Employee must be at least 18 years old");
                }
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Create", model);
                return View(model);
            }

            // ✅ Upload avatar (optional)
            if (model.AvatarFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.AvatarFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/staff", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                model.Avatar = "/images/staff/" + fileName;
            }

            var (success, errorMessage) = await _staffService.CreateStaffAsync(model);

            if (!success)
            {
                // 🔥 Đưa lỗi API xuống form luôn
                ModelState.AddModelError("", errorMessage ?? "Failed to create employee");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Create", model);

                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = errorMessage ?? "Failed to create employee";

                return View(model);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Employee created successfully" });

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Employee created successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Staffs/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Employee ID is required");

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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("Edit", model);

            return View(model);
        }

        // POST: /Admin/Staffs/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            // ✅ Validate tuổi (server-side) giống Create
            if (model.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - model.DateOfBirth.Value.Year;

                if (model.DateOfBirth.Value > today.AddYears(-age))
                    age--;

                if (age < 18)
                {
                    ModelState.AddModelError("DateOfBirth", "Employee must be at least 18 years old");
                }
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Edit", model);
                
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Please correct the errors in the form";
                return View(model);
            }

            try
            {
                // Upload avatar (optional) and send its URL to API
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.AvatarFile.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/staff", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(stream);
                    }

                    model.Avatar = "/images/staff/" + fileName;
                }

                var (success, errorMessage) = await _staffService.UpdateStaffAsync(model);
                if (!success)
                {
                    ModelState.AddModelError("", errorMessage ?? "Failed to update employee");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return PartialView("Edit", model);

                    TempData["ToastType"] = "error";
                    TempData["ToastMessage"] = errorMessage ?? "Failed to update employee";
                    return View(model);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Employee updated successfully" });

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Employee updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                string message = ex.Message switch
                {
                    "INVALID_AGE" => "Employee must be at least 18 years old",
                    "CITIZENID_EXISTS" => "Citizen ID already exists",
                    _ => "Failed to update employee. Please try again."
                };
                
                ModelState.AddModelError("", message);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Edit", model);

                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = message;
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to update employee. Please try again.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Edit", model);

                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to update employee. Please try again.";
                return View(model);
            }
        }

        // POST: /Admin/Staffs/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Employee ID is required");

            var result = await _staffService.DeleteStaffAsync(id);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result, message = result ? "Employee deleted successfully" : "Failed to delete employee" });
            }

            if (!result)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to delete employee";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Employee deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
