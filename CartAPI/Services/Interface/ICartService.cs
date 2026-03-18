using CartAPI.DTOs;

namespace CartAPI.Services.Interface
{
    public interface ICartService
    {
        Task<CartReadDto?> GetCartOfCustomerAsync(string CustomerId);
        Task<CartReadDto> AddToCartAsync(string CustomerId, CartItemCreateDto itemDto);
        Task<bool> UpdateCartItemAsync(int cartItemId, int newQuantity);
        Task<bool> DeleteCartItemAsync(int cartItemId);
    }
}
