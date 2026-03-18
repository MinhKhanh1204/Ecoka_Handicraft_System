using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface ICartService
    {
        Task<CartViewModel?> GetCartAsync();
        Task<bool> AddToCartAsync(string productId, int quantity);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> DeleteCartItemAsync(int cartItemId);
    }
}
