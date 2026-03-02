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

        public OrderService(
            IOrderRepository orderRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
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

                var priceAfterDiscount =
                    unitPrice - (unitPrice * discount / 100m);

                total += priceAfterDiscount * item.Quantity.Value;
            }

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
    }
}