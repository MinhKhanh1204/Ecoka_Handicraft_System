using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;
using OrderAPI.Repositories;
using OrderAPI.Services;

namespace OrderAPI.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrderService(
            IOrderRepository orderRepo,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ================= CUSTOMER =================

        public async Task<IEnumerable<OrderReadDto>> GetOrdersByCustomerAsync(string customerId)
        {
            var orders = await _orderRepo.GetOrdersByCustomerAsync(customerId);
            return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
        }

        public async Task<IEnumerable<OrderReadDto>> SearchOrdersAsync(
            string customerId,
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus)
        {
            var orders = await _orderRepo.SearchOrdersAsync(
                customerId, orderId, from, to, paymentStatus, tabStatus);

            return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
        }

        public async Task<OrderReadDto?> GetOrderDetailAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderDetailAsync(orderId);
            return order == null ? null : _mapper.Map<OrderReadDto>(order);
        }

        public Task<bool> CancelOrderAsync(string orderId, string cancelReason)
            => _orderRepo.CancelOrderAsync(orderId, cancelReason);

        public Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId)
            => _orderRepo.HasCustomerPurchasedProductAsync(customerId, productId);

        // ================= STAFF =================

        public async Task<IEnumerable<OrderReadDto>> GetAllOrdersForStaffAsync()
        {
            var orders = await _orderRepo.GetAllOrdersForStaffAsync();
            return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
        }

        public async Task<IEnumerable<OrderReadDto>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            var orders = await _orderRepo.SearchOrdersForStaffAsync(
                orderId, customerName, from, to, shippingStatus, paymentStatus);

            return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
        }

        public Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId)
            => _orderRepo.UpdateOrderStatusAsync(orderId, newStatus, staffId);

        public async Task<OrderReadDto?> GetOrderDetailForStaffAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderDetailForStaffAsync(orderId);
            return order == null ? null : _mapper.Map<OrderReadDto>(order);
        }

        // ================= CREATE ORDER =================

        public async Task<OrderReadDto> CreateAsync(OrderCreateDto dto)
        {
            if (dto.OrderItems == null || !dto.OrderItems.Any())
                throw new ArgumentException("Order must contain at least one item.");

            var entity = _mapper.Map<Order>(dto);
            decimal total = 0m;

            foreach (var item in entity.OrderItems)
            {
                if (item.Quantity == null || item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0.");

                if (item.UnitPrice == null || item.UnitPrice <= 0)
                    throw new ArgumentException("UnitPrice must be greater than 0.");

                var unitPrice = item.UnitPrice.Value;
                var discount = item.Discount ?? 0m;
                var priceAfterDiscount = unitPrice - (unitPrice * discount / 100m);
                total += priceAfterDiscount * item.Quantity.Value;
            }

            // Apply Voucher
            if (dto.VoucherID.HasValue && dto.VoucherID > 0)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    string voucherUrl = _configuration["VoucherApiUrl"] + dto.VoucherID;
                    var response = await client.GetAsync(voucherUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<VoucherApiResponse>();
                        if (result != null && result.Success && result.Data != null)
                        {
                            var v = result.Data;
                            decimal voucherDiscount = total * ((v.DiscountPercentage ?? 0m) / 100m);
                            
                                if (v.MaxReducing.HasValue && voucherDiscount > v.MaxReducing.Value)
                                {
                                    voucherDiscount = v.MaxReducing.Value;
                                }
                                total -= voucherDiscount;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Handle voucher error (should use ILogger)
                    }
            }

            if (total < 0) total = 0;
            entity.TotalAmount = total;

            var created = await _orderRepo.CreateAsync(entity);
            return _mapper.Map<OrderReadDto>(created);
        }

        private class VoucherApiResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("success")]
            public bool Success { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public VoucherData? Data { get; set; }
        }

        private class VoucherData
        {
            [System.Text.Json.Serialization.JsonPropertyName("discountPercentage")]
            public decimal? DiscountPercentage { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("maxReducing")]
            public decimal? MaxReducing { get; set; }
        }

        // ================= PAYMENT =================

        public Task UpdatePaymentStatusAsync(
            string orderId,
            string paymentMethod,
            string paymentStatus,
            string? note)
            => _orderRepo.UpdatePaymentStatusAsync(
                orderId,
                paymentMethod,
                paymentStatus,
                note ?? string.Empty);

        // ================= GENERAL =================

        public async Task<OrderReadDto?> GetByIdAsync(string orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            return order == null ? null : _mapper.Map<OrderReadDto>(order);
        }
    }
}