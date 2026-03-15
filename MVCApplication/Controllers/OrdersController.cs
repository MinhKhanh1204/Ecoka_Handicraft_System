using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;

        public OrdersController(IOrderService service, IProductService productService, ICartService cartService, IConfiguration configuration)
        {
            _orderService = service;
            _productService = productService;
            _cartService = cartService;
            _configuration = configuration;
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
            decimal subtotal = 0;
            foreach (var item in selectedItems)
            {
                try
                {
                    var product = await _productService.GetProductDetailAsync(item.ProductId);
                    if (product != null)
                    {
                        item.Price = product.FinalPrice > 0 ? product.FinalPrice : product.Price;
                        subtotal += item.Price * item.Quantity;
                    }
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

                // Nếu là VNPay thì redirect sang cổng thanh toán
                if (model.PaymentMethod == "VNPAY")
                {
                    var vnpay = new Utils.VnPayLibrary();
                    var vnp_TmnCode = _configuration["VNPay:TmnCode"];
                    var vnp_HashSecret = _configuration["VNPay:HashSecret"];
                    var vnp_Url = _configuration["VNPay:BaseUrl"];
                    var vnp_ReturnUrl = _configuration["VNPay:ReturnUrl"];

                    vnpay.AddRequestData("vnp_Version", _configuration["VNPay:Version"]);
                    vnpay.AddRequestData("vnp_Command", _configuration["VNPay:Command"]);
                    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                    vnpay.AddRequestData("vnp_Amount", ((long)(newOrder.TotalAmount * 100 ?? 0)).ToString()); 
                    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + newOrder.OrderID);
                    vnpay.AddRequestData("vnp_OrderType", "other");
                    vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
                    vnpay.AddRequestData("vnp_TxnRef", newOrder.OrderID.ToString()); 

                    string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                    return Redirect(paymentUrl);
                }

                return RedirectToAction("Details", new { id = newOrder.OrderID });
            }

            return BadRequest("Failed to create order");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpay = new Utils.VnPayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            string orderId = vnpay.GetResponseData("vnp_TxnRef");
            long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];
            string terminalID = Request.Query["vnp_TmnCode"];
            long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
            string bankCode = Request.Query["vnp_BankCode"];

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["VNPay:HashSecret"]);
            if (checkSignature)
            {
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    // Thanh toán thành công
                    await _orderService.UpdatePaymentStatusAsync(orderId, "Paid");
                    TempData["PaymentMsg"] = "Thanh toán thành công qua VNPay!";
                }
                else
                {
                    // Thanh toán lỗi
                    TempData["PaymentMsg"] = $"Thanh toán thất bại. Mã lỗi: {vnp_ResponseCode}";
                }
            }
            else
            {
                TempData["PaymentMsg"] = "Chữ ký không hợp lệ!";
            }

            return RedirectToAction("Index");
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
