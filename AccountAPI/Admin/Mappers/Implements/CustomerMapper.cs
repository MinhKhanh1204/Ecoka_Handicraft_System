using AccountAPI.Admin.DTOs;
using AccountAPI.Models;

namespace AccountAPI.Admin.Mappers.Implements
{
    public class CustomerMapper : ICustomerMapper
    {
        public CustomerDto ToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerID = customer.CustomerID,
                Username = customer.Account?.Username,
                Email = customer.Account?.Email,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                Phone = customer.Phone,
                Address = customer.Address,
                Avatar = customer.Account?.Avatar,
                Status = customer.Status,
                CreatedAt = customer.Account?.CreatedAt
            };
        }
    }
}
