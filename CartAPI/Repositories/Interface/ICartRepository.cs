namespace CartAPI.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByCustomerAsync(string CustomerId);
        Task<Cart?> GetByIdAsync(int CartId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task<Cart> AddCartAsync(Cart cart);
        Task<CartItem> AddCartItemAsync(int CartId, CartItem item);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> DeleteCartItemAsync(int cartItemId);
    }
}
