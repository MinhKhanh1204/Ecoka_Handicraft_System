using AccountAPI.Admin.DTOs;
using AccountAPI.Admin.Mappers;
using AccountAPI.Admin.Repositories;

namespace AccountAPI.Admin.Services.Implements
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ICustomerMapper _mapper;

        public CustomerService(ICustomerRepository repository, ICustomerMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers.Select(c => _mapper.ToDto(c)).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(string customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);
            return customer == null ? null : _mapper.ToDto(customer);
        }

        public async Task<List<CustomerDto>> SearchAsync(string? keyword, string? status)
        {
            var customers = await _repository.SearchAsync(keyword, status);
            return customers.Select(c => _mapper.ToDto(c)).ToList();
        }

        public async Task<bool> UpdateStatusAsync(string customerId, string status)
        {
            return await _repository.UpdateStatusAsync(customerId, status);
        }
    }
}
