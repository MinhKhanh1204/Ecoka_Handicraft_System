using CartAPI.DTOs;
using CartAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;
        public CartRepository(CartDbContext context) => _context = context;

        public async Task<Cart?> GetCartByCustomerAsync(string CustomerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == CustomerId);
        }

        public async Task<Cart?> GetByIdAsync(int CartId)
        {
            return await _context.Carts.Include(c => c.CartItems)
                                      .FirstOrDefaultAsync(c => c.CartId == CartId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems.FindAsync(cartItemId);
        }

        public async Task<Cart> AddCartAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItem> AddCartItemAsync(int CartId, CartItem item)
        {
            var cart = await _context.Carts.Include(c => c.CartItems)
                             .FirstOrDefaultAsync(c => c.CartId == CartId);
            if (cart == null) throw new Exception("Cart not found.");

            // Check if item exists, if so, just update quantity
            var existing = cart.CartItems.FirstOrDefault(x => x.ProductID == item.ProductID);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                await _context.SaveChangesAsync();
                return existing;
            }

            item.CartId = CartId;
            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return false;
            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return false;
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
