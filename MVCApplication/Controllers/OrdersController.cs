using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    [Authorize]
    [Route("customer/orders")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            IProductService productService,
            ICartService cartService,
            IConfiguration configuration,
            IPaymentService paymentService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _productService = productService;
            _cartService = cartService;
            _configuration = configuration;
            _paymentService = paymentService;
            _logger = logger;
        }

        // GET: /customer/orders/checkout?cartItemIds=1,2,3
        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout(string cartItemIds)
        {
            if (string.IsNullOrWhiteSpace(cartItemIds))
                return RedirectToAction("View", "Cart");

            var ids = cartItemIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!ids.Any())
                return RedirectToAction("View", "Cart");

            var cart = await _cartService.GetCartAsync();
            if (cart?.CartItems == null)
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading product detail for ProductId: {ProductId}", item.ProductId);
                }
            }

            var model = new CheckoutViewModel
            {
                SelectedItems = selectedItems,
                Subtotal = subtotal
            };

            return View(model);
        }

        // POST: /customer/orders/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CheckoutViewModel model, string cartItemIds)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return Unauthorized();

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(cartItemIds))
                return BadRequest("Invalid order data");

            var ids = cartItemIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!ids.Any())
                return BadRequest("No valid cart item ids");

            var cart = await _cartService.GetCartAsync();
            var selectedItems = cart?.CartItems?.Where(i => ids.Contains(i.CartItemId)).ToList();

            if (selectedItems == null || !selectedItems.Any())
                return BadRequest("No items selected for checkout");

            foreach (var item in selectedItems)
            {
                try
                {
                    var product = await _productService.GetProductDetailAsync(item.ProductId);
                    if (product != null)
                        item.Price = product.FinalPrice > 0 ? product.FinalPrice : product.Price;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enriching product price for ProductId: {ProductId}", item.ProductId);
                }
            }

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

            var newOrder = await _orderService.CreateAsync(orderDto);

            if (newOrder == null)
                return BadRequest("Failed to create order");

            foreach (var id in ids)
            {
                await _cartService.DeleteCartItemAsync(id);
            }

            if (model.PaymentMethod == "VNPAY")
            {
                string returnUrl = Url.Action(nameof(PaymentCallback), "Orders", null, Request.Scheme)!;
                string? url = await _paymentService.GetVnPayUrlAsync(
                    newOrder.OrderID.ToString(),
                    newOrder.TotalAmount ?? 0,
                    "Thanh toan don hang " + newOrder.OrderID,
                    returnUrl);

                if (!string.IsNullOrEmpty(url))
                    return Redirect(url);
            }
            else if (model.PaymentMethod == "MOMO")
            {
                string returnUrl = Url.Action(nameof(PaymentCallback), "Orders", null, Request.Scheme)!;
                string? url = await _paymentService.GetMomoUrlAsync(
                    newOrder.OrderID.ToString(),
                    newOrder.TotalAmount ?? 0,
                    "Thanh toan don hang " + newOrder.OrderID,
                    returnUrl);

                if (!string.IsNullOrEmpty(url))
                    return Redirect(url);
            }

            return RedirectToAction(nameof(Details), new { id = newOrder.OrderID });
        }

        [HttpGet("payment-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback()
        {
            string? vnp_ResponseCode = Request.Query["vnp_ResponseCode"];
            string? momo_ResultCode = Request.Query["resultCode"];
            string? orderIdRaw = Request.Query["orderId"];
            string? vnp_TxnRef = Request.Query["vnp_TxnRef"];

            if (vnp_ResponseCode == "00" || momo_ResultCode == "0")
            {
                string? orderId = !string.IsNullOrEmpty(orderIdRaw)
                    ? orderIdRaw.Split('_')[0]
                    : vnp_TxnRef;

                string paymentMethod = !string.IsNullOrEmpty(orderIdRaw) ? "MoMo" : "VNPay";

                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        await _orderService.UpdatePaymentStatusAsync(orderId, paymentMethod, "Paid", "Thanh toan thanh cong");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update payment status for OrderId: {OrderId}", orderId);
                    }
                }

                TempData["PaymentMsg"] = "Thành công! Đơn hàng của bạn đã được ghi nhận và đang chờ xử lý.";
            }
            else
            {
                TempData["PaymentMsg"] = "Giao dịch không thành công hoặc đã bị người dùng hủy.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /customer/orders
        [HttpGet("")]
        public async Task<IActionResult> Index(int currentPage = 1, int historyPage = 1)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);

            int pageSize = 6;

            var currentOrders = orders
                .Where(o => o.ShippingStatus != "Delivered" && o.ShippingStatus != "Cancelled")
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var historyOrders = orders
                .Where(o => o.ShippingStatus == "Delivered" || o.ShippingStatus == "Cancelled")
                .Skip((historyPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = currentPage;
            ViewBag.HistoryPage = historyPage;
            ViewBag.CurrentTotal = orders.Count(o => o.ShippingStatus != "Delivered" && o.ShippingStatus != "Cancelled");
            ViewBag.HistoryTotal = orders.Count(o => o.ShippingStatus == "Delivered" || o.ShippingStatus == "Cancelled");
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentOrders = currentOrders;
            ViewBag.HistoryOrders = historyOrders;

            return View();
        }

        // GET: /customer/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var order = await _orderService.GetOrderDetailAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", id);
                    return NotFound();
                }

                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ProductID))
                        {
                            try
                            {
                                var product = await _productService.GetProductDetailAsync(item.ProductID);
                                if (product != null)
                                {
                                    item.ProductName = product.ProductName;
                                    item.ProductImage = product.MainImage;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Product API error while enriching order {OrderId}, product {ProductId}", id, item.ProductID);
                            }
                        }
                    }
                }

                return PartialView("_OrderDetailPartial", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order detail error for order {OrderId}", id);
                return StatusCode(500, "An error occurred while retrieving order details.");
            }
        }

        // GET: /customer/orders/search
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            int currentPage = 1,
            int historyPage = 1)
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
                null);

            int pageSize = 6;

            var currentOrders = orders
                .Where(o => o.ShippingStatus != "Delivered" && o.ShippingStatus != "Cancelled")
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var historyOrders = orders
                .Where(o => o.ShippingStatus == "Delivered" || o.ShippingStatus == "Cancelled")
                .Skip((historyPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentOrders = currentOrders;
            ViewBag.HistoryOrders = historyOrders;
            ViewBag.CurrentPage = currentPage;
            ViewBag.HistoryPage = historyPage;
            ViewBag.CurrentTotal = orders.Count(o => o.ShippingStatus != "Delivered" && o.ShippingStatus != "Cancelled");
            ViewBag.HistoryTotal = orders.Count(o => o.ShippingStatus == "Delivered" || o.ShippingStatus == "Cancelled");
            ViewBag.PageSize = pageSize;

            return View("Index");
        }

        // POST: /customer/orders/{id}/cancel
        [HttpPost("{id}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            await _orderService.CancelOrderAsync(id, "Cancelled by customer");
            return RedirectToAction(nameof(Index));
        }
    }
}