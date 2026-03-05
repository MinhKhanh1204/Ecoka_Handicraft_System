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

        // ================= GET BY PRODUCT + FILTER =================
        [HttpGet]
        public async Task<IActionResult> GetByProduct(string productId, int? minRating)
        {
            var feedbacks = await _feedbackService.FilterAsync(new FeedbackFilterDto
            {
                ProductID = productId,
                Status = "Active",
                MinRating = minRating
            });

            return Json(feedbacks);
        }

        // ================= CREATE =================
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

        // ================= UPDATE =================
        [HttpPut]
        [Route("Feedback/Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FeedbackUpdateDto dto)
        {
            try
            {
                var result = await _feedbackService.UpdateAsync(id, dto);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ================= DELETE =================
        [HttpDelete]
        [Route("Feedback/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _feedbackService.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}