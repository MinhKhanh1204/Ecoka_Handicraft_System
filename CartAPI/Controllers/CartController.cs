using CartAPI.DTOs;
using CartAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        // POST api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto dto)
        {
            string? CustomerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(CustomerId)) return Unauthorized();

            var result = await _service.AddToCartAsync(CustomerId, dto);
            return Ok(result);
        }

        // GET api/cart/items
        [HttpGet("items")]
        public async Task<IActionResult> GetCart()
        {
            string? CustomerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(CustomerId)) return Unauthorized();

            var cart = await _service.GetCartOfCustomerAsync(CustomerId);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        // PUT api/cart/items/{cartItemId}
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] CartItemUpdateDto dto)
        {
            string? CustomerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(CustomerId)) return Unauthorized();

            var ok = await _service.UpdateCartItemAsync(cartItemId, dto.Quantity);
            return ok ? Ok() : NotFound();
        }

        // DELETE api/cart/items/{cartItemId}
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem(int cartItemId)
        {
            string? CustomerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(CustomerId)) return Unauthorized();

            var ok = await _service.DeleteCartItemAsync(cartItemId);
            return ok ? Ok() : NotFound();
        }
    }
}
