using CartAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Controllers
{
    [Route("odata")]
    [Authorize]
    public class CartODataController : ODataController
    {
        private readonly CartDbContext _context;

        public CartODataController(CartDbContext context)
        {
            _context = context;
        }

        // Chỉ cho user hiện tại xem cart của mình
        [HttpGet("carts")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions =
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Count |
            AllowedQueryOptions.Expand)]
        public IQueryable<Cart> GetCarts()
        {
            // Lấy CustomerId từ JWT claims (dạng string, ví dụ "CUS001")
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Enumerable.Empty<Cart>().AsQueryable();

            return _context.Carts
                .Include(c => c.CartItems)
                .Where(c => c.CustomerId == customerId)
                .AsNoTracking();
        }

        // Lấy cart theo CartId (nếu thực sự cần)
        [HttpGet("carts({CartId})")]
        [EnableQuery]
        public IActionResult GetCart([FromRoute] int CartId)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized();

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .Where(c => c.CartId == CartId && c.CustomerId == customerId)
                .AsNoTracking()
                .FirstOrDefault();

            if (cart == null)
                return NotFound();

            return Ok(cart);
        }
    }
}