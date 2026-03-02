using Microsoft.AspNetCore.Mvc;
using FeedbackAPI.DTOs;
using FeedbackAPI.Services;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/feedbacks/{feedbackId:int}/replies")]
    public class FeedbackReplyController : ControllerBase
    {
        private readonly IFeedbackReplyService _service;

        public FeedbackReplyController(IFeedbackReplyService service)
        {
            _service = service;
        }

        // ================= GET ALL REPLIES OF A FEEDBACK =================
        [HttpGet]
        public async Task<IActionResult> GetByFeedbackId(int feedbackId)
        {
            var replies = await _service.GetByFeedbackIdAsync(feedbackId);
            return Ok(replies);
        }

        // ================= GET REPLY BY ID =================
        [HttpGet("{replyId:int}")]
        public async Task<IActionResult> GetById(int feedbackId, int replyId)
        {
            var reply = await _service.GetByIdAsync(replyId);
            if (reply == null || reply.FeedbackID != feedbackId) return NotFound();
            return Ok(reply);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(int feedbackId, [FromBody] FeedbackReplyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(feedbackId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { feedbackId, replyId = created.ReplyID },
                created);
        }

        // ================= UPDATE =================
        [HttpPut("{replyId:int}")]
        public async Task<IActionResult> Update(int feedbackId, int replyId, [FromBody] FeedbackReplyUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify reply belongs to this feedback before updating
            var existing = await _service.GetByIdAsync(replyId);
            if (existing == null || existing.FeedbackID != feedbackId) return NotFound();

            var updated = await _service.UpdateAsync(replyId, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // ================= DELETE =================
        [HttpDelete("{replyId:int}")]
        public async Task<IActionResult> Delete(int feedbackId, int replyId)
        {
            // Verify reply belongs to this feedback before deleting
            var existing = await _service.GetByIdAsync(replyId);
            if (existing == null || existing.FeedbackID != feedbackId) return NotFound();

            var result = await _service.DeleteAsync(replyId);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
