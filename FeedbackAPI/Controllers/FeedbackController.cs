using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FeedbackAPI.DTOs;
using FeedbackAPI.Services;
using System.Security.Claims;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/feedbacks")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feedbacks = await _service.GetAllAsync();
            return Ok(feedbacks);
        }

        // ================= GET BY ID =================
        [HttpGet("{feedbackId:int}")]
        public async Task<IActionResult> GetById(int feedbackId)
        {
            var feedback = await _service.GetByIdAsync(feedbackId);
            if (feedback == null) return NotFound();
            return Ok(feedback);
        }

        // ================= FILTER =================
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] FeedbackFilterDto filter)
        {
            var feedbacks = await _service.FilterAsync(filter);
            return Ok(feedbacks);
        }

        // ================= CREATE =================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] FeedbackCreateDto dto)
        {
            // If client didn't provide CustomerID, use the authenticated user's id from JWT
            var userId = User.FindFirst("accountID")?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(dto.CustomerID) && !string.IsNullOrWhiteSpace(userId))
            {
                dto.CustomerID = userId;

                // Remove model state error for CustomerID if it was added during model binding
                if (ModelState.ContainsKey(nameof(dto.CustomerID)))
                    ModelState.Remove(nameof(dto.CustomerID));
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { feedbackId = created.FeedbackID },
                created);
        }

        // ================= UPDATE =================
        [HttpPut("{feedbackId:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int feedbackId, [FromForm] FeedbackUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(feedbackId, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // ================= DELETE =================
        [HttpDelete("{feedbackId:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int feedbackId)
        {
            var result = await _service.DeleteAsync(feedbackId);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
