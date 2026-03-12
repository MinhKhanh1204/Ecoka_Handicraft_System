using AutoMapper;
using OrderAPI.Admin.DTOs;
using OrderAPI.Admin.Repositories;
using OrderAPI.DTOs;

namespace OrderAPI.Admin.Services.Implements
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IAdminOrderRepository _orderRepo;
        private readonly IMapper _mapper;


        public AdminOrderService(IAdminOrderRepository repository, IMapper mapper)
        {
            _orderRepo = repository;
            _mapper = mapper;

        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            return await _orderRepo.GetRevenueByYearAsync(year);
        }

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

        // ================= GENERAL =================

        public async Task<OrderReadDto?> GetByIdAsync(string orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            return order == null ? null : _mapper.Map<OrderReadDto>(order);
        }
    }
}
