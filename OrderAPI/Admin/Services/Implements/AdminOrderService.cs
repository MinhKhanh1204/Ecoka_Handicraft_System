using OrderAPI.Admin.DTOs;
using OrderAPI.Admin.Repositories;

namespace OrderAPI.Admin.Services.Implements
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IAdminOrderRepository _repository;

        public AdminOrderService(IAdminOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            return await _repository.GetRevenueByYearAsync(year);
        }
    }
}
