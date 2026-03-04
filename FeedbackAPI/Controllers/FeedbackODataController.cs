using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    /// <summary>
    /// OData-enabled endpoint for querying feedbacks.
    /// Supports $filter, $orderby, $select, $top, $skip, $count.
    ///
    /// Examples:
    ///   GET /odata/feedbacks?$filter=Rating ge 4
    ///   GET /odata/feedbacks?$orderby=CreatedAt desc&$top=10
    ///   GET /odata/feedbacks?$select=FeedbackID,CustomerID,Rating
    ///   GET /odata/feedbacks?$filter=ProductID eq 'P001'&$count=true
    /// </summary>
    [Route("odata")]
    public class FeedbackODataController : ODataController
    {
        private readonly DBContext _context;

        public FeedbackODataController(DBContext context)
        {
            _context = context;
        }

        // ================= FEEDBACKS =================

        [HttpGet("feedbacks")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions =
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Count)]
        public IQueryable<Feedback> GetFeedbacks()
        {
            return _context.Feedbacks.AsNoTracking();
        }

        [HttpGet("feedbacks({feedbackId})")]
        [EnableQuery]
        public IActionResult GetFeedback([FromRoute] int feedbackId)
        {
            var feedback = _context.Feedbacks
                .AsNoTracking()
                .Where(f => f.FeedbackID == feedbackId);

            return Ok(feedback.FirstOrDefault());
        }

    }
}
