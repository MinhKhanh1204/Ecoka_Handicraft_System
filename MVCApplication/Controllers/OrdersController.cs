using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;

        public OrdersController(IOrderService service, IProductService productService, ICartService cartService, IConfiguration configuration, IPaymentService paymentService)
        {
            _orderService = service;
            _productService = productService;
            _cartService = cartService;
            _configuration = configuration;
            _paymentService = paymentService;
        }

        // --- CHECKOUT FLOW ---
        [HttpGet]
        public async Task<IActionResult> Checkout(string cartItemIds)
        {
            if (string.IsNullOrWhiteSpace(cartItemIds))
                return RedirectToAction("View", "Cart");

            var ids = cartItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse)
                                 .ToList();

            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.CartItems == null)
                return RedirectToAction("View", "Cart");

            var selectedItems = cart.CartItems.Where(i => ids.Contains(i.CartItemId)).ToList();
            if (!selectedItems.Any())
                return RedirectToAction("View", "Cart");

            decimal subtotal = 0;
            foreach (var item in selectedItems)
            {
                try
                {
                    var product = await _productService.GetProductDetailAsync(item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.ProductName;
                        item.Price = product.FinalPrice > 0 ? product.FinalPrice : product.Price;
                        item.ImageUrl = product.MainImage;
                        subtotal += item.Price * item.Quantity;
                    }
                }
                catch { }
            }

            var model = new CheckoutViewModel
            {
                SelectedItems = selectedItems,
                Subtotal = subtotal
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CheckoutViewModel model, string cartItemIds)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return Unauthorized();

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(cartItemIds))
                return BadRequest("Invalid order data");

            // Extract selected Cart items
            var ids = cartItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            var cart = await _cartService.GetCartAsync();
            var selectedItems = cart?.CartItems?.Where(i => ids.Contains(i.CartItemId)).ToList();

            if (selectedItems == null || !selectedItems.Any())
                return BadRequest("No items selected for checkout");

            // Enrich Prices
            foreach (var item in selectedItems)
            {
                try
                {
                    var product = await _productService.GetProductDetailAsync(item.ProductId);
                    if (product != null)
                        item.Price = product.FinalPrice > 0 ? product.FinalPrice : product.Price;
                }
                catch { }
            }

            // Create OrderDto
            var orderDto = new MVCApplication.Models.DTOs.OrderCreateDto
            {
                CustomerID = customerId,
                PaymentMethod = model.PaymentMethod,
                ShippingAddress = model.ShippingAddress,
                Note = model.Note,
                VoucherID = model.VoucherId,
                OrderItems = selectedItems.Select(i => new MVCApplication.Models.DTOs.OrderItemCreateDto
                {
                    ProductID = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList()
            };

            // Send to OrderAPI
            var newOrder = await _orderService.CreateAsync(orderDto);
            
            if (newOrder != null)
            {
                // Xóa cart items đã mua
                foreach (var id in ids)
                {
                    await _cartService.DeleteCartItemAsync(id);
                }

                // Chuyển hướng thanh toán qua PaymentService
                if (model.PaymentMethod == "VNPAY")
                {
                    string returnUrl = Url.Action("PaymentCallback", "Orders", null, Request.Scheme)!;
                    string? url = await _paymentService.GetVnPayUrlAsync(newOrder.OrderID.ToString(), newOrder.TotalAmount ?? 0, "Thanh toan don hang " + newOrder.OrderID, returnUrl);
                    if (!string.IsNullOrEmpty(url)) return Redirect(url);
                }
                else if (model.PaymentMethod == "MOMO")
                {
                    string returnUrl = Url.Action("PaymentCallback", "Orders", null, Request.Scheme)!;
                    string? url = await _paymentService.GetMomoUrlAsync(newOrder.OrderID.ToString(), newOrder.TotalAmount ?? 0, "Thanh toan don hang " + newOrder.OrderID, returnUrl);
                    if (!string.IsNullOrEmpty(url)) return Redirect(url);
                }

                return RedirectToAction("Details", new { id = newOrder.OrderID });
            }

            return BadRequest("Failed to create order");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback()
        {
            // PaymentCallback là trang return cho người dùng xem
            string? vnp_ResponseCode = Request.Query["vnp_ResponseCode"];
            string? memo_ResultCode = Request.Query["resultCode"];
            string? orderIdRaw = Request.Query["orderId"]; // MoMo
            string? vnp_TxnRef = Request.Query["vnp_TxnRef"]; // VNPay

            if (vnp_ResponseCode == "00" || memo_ResultCode == "0")
            {
                string? orderId = !string.IsNullOrEmpty(orderIdRaw) ? orderIdRaw.Split('_')[0] : vnp_TxnRef;
                string paymentMethod = !string.IsNullOrEmpty(orderIdRaw) ? "MoMo" : "VNPay";
                
                if (!string.IsNullOrEmpty(orderId))
                {
                    // Fallback update status nếu IPN chưa tới (đặc biệt hữu ích khi chạy localhost)
                    try 
                    {
                        await _orderService.UpdatePaymentStatusAsync(orderId, paymentMethod, "Paid", "Thanh toan thanh cong");
                    }
                    catch { }
                }

                TempData["PaymentMsg"] = "Thành công! Đơn hàng của bạn đã được ghi nhận và đang chờ xử lý.";
            }
            else
            {
                TempData["PaymentMsg"] = "Giao dịch không thành công hoặc đã bị người dùng hủy.";
            }

            return RedirectToAction("Index", "Orders");
        }
        // --- END CHECKOUT FLOW ---

        // Danh sách order của customer
        public async Task<IActionResult> Index(string? search)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService
                .GetOrdersByCustomerAsync(customerId);

            return View(orders);
        }

        // Chi tiết order
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var order = await _orderService.GetOrderDetailAsync(id);
                if (order == null)
                    return NotFound();

                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ProductID))
                        {
                            try
                            {
                                var product = await _productService
                                    .GetProductDetailAsync(item.ProductID);

                                if (product != null)
                                {
                                    item.ProductName = product.ProductName;
                                    item.ProductImage = product.MainImage;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Product API error: " + ex.Message);
                            }
                        }
                    }
                }

                return PartialView("_OrderDetailPartial", order);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Order detail error: " + ex.Message);
                return StatusCode(500, "Failed to load order details");
            }
        }

        // Search
        public async Task<IActionResult> Search(
    string? orderId,
    DateTime? from,
    DateTime? to,
    string? paymentStatus,
    string? tabStatus)
        {
            var customerId = User.FindFirst("accountID")?.Value;

            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService.SearchOrdersAsync(
                customerId,
                orderId,
                from,
                to,
                paymentStatus,
                tabStatus);

            return View("Index", orders);
        }

        // Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            await _orderService.CancelOrderAsync(id, "Cancelled by customer");

            return RedirectToAction(nameof(Index));
        }
    }
}
