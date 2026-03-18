using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        // Xem giỏ hàng qua OData (qua gateway), KHÔNG cần CustomerId — JWT tự định danh user
        [HttpGet]
        public async Task<IActionResult> View()
        {
            var model = await _cartService.GetCartAsync();

            if (model != null && model.CartItems != null && model.CartItems.Any())
            {
                foreach (var item in model.CartItems)
                {
                    try
                    {
                        var product = await _productService.GetProductDetailAsync(item.ProductId);
                        if (product != null)
                        {
                            item.ProductName = product.ProductName;
                            item.Price = product.FinalPrice > 0 ? product.FinalPrice : product.Price;
                            item.ImageUrl = product.MainImage;
                            item.StockQuantity = product.StockQuantity;
                        }
                    }
                    catch
                    {
                        // Ignore if product fetch fails, will just show default values
                    }
                }
            }

            return View(model);
        }

        // Thêm sản phẩm vào cart (AJAX từ Detail page)
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            var ok = await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
            return ok ? Ok() : BadRequest();
        }

        // Update số lượng 1 item trong cart (AJAX call hoặc form submit)
        [HttpPut]
        public async Task<IActionResult> UpdateItem(int cartItemId, int quantity)
        {
            var ok = await _cartService.UpdateCartItemAsync(cartItemId, quantity);
            return ok ? Ok() : BadRequest();
        }

        // Xóa 1 item khỏi cart (AJAX call hoặc form submit)
        [HttpDelete]
        public async Task<IActionResult> DeleteItem(int cartItemId)
        {
            var ok = await _cartService.DeleteCartItemAsync(cartItemId);
            return ok ? Ok() : BadRequest();
        }
    }

    public class AddCartItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}