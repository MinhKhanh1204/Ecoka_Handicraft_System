using AccountAPI.Admin.DTOs;
using AccountAPI.Models;

namespace AccountAPI.Admin.Mappers
{
    public interface ICustomerMapper
    {
        CustomerDto ToDto(Customer customer);
    }
}
