using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;
using OrderAPI.Repositories;

namespace OrderAPI.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
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
                var quantity = item.Quantity.Value;
                var discount = item.Discount ?? 0m;

                if (discount < 0 || discount > 100)
                    throw new ArgumentException("Discount must be between 0 and 100.");

                var priceAfterDiscount = unitPrice - (unitPrice * discount / 100m);
                total += priceAfterDiscount * quantity;
            }

            // Apply Voucher
            if (dto.VoucherID.HasValue && dto.VoucherID.Value > 0)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();

                    string baseUrl = _configuration["VoucherApiUrl"]
                        ?? throw new InvalidOperationException("VoucherApiUrl is missing.");

                    string voucherUrl = $"{baseUrl.TrimEnd('/')}/{dto.VoucherID.Value}";

                    var response = await client.GetAsync(voucherUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<VoucherApiResponse>();

                        if (result != null && result.Success && result.Data != null)
                        {
                            var v = result.Data;
                            decimal discountPercentage = v.DiscountPercentage ?? 0m;

                            if (discountPercentage > 0)
                            {
                                decimal voucherDiscount = total * discountPercentage / 100m;

                                if (v.MaxReducing.HasValue && voucherDiscount > v.MaxReducing.Value)
                                {
                                    voucherDiscount = v.MaxReducing.Value;
                                }

                                total -= voucherDiscount;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while applying voucher {VoucherId}", dto.VoucherID);
                }
            }

            if (total < 0) total = 0;
            entity.TotalAmount = total;

            var created = await _orderRepo.CreateAsync(entity);
            return _mapper.Map<OrderReadDto>(created);
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

        public async Task<OrderReadDto?> GetOrderDetailAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderDetailAsync(orderId);
            return order == null ? null : _mapper.Map<OrderReadDto>(order);
        }

        public Task<bool> CancelOrderAsync(string orderId, string cancelReason)
            => _orderRepo.CancelOrderAsync(orderId, cancelReason);

        public Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId)
            => _orderRepo.HasCustomerPurchasedProductAsync(customerId, productId);

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
    }
}