using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByProduct(string productId)  // ← đổi sang string
        {
            var feedbacks = await _feedbackService.FilterAsync(new FeedbackFilterDto
            {
                ProductID = productId,    // ← giờ khớp string = string
                Status = "Active"
            });
            return Json(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
        {
            var username = User.FindFirst("username")?.Value;
            if (string.IsNullOrEmpty(username))
                return Json(new { success = false, message = "Please login first" });

            try
            {
                var result = await _feedbackService.CreateAsync(dto);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}