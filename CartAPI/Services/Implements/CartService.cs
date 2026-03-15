using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Repositories.Interface;
using CartAPI.Services.Interface;

namespace CartAPI.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repo;
        private readonly IMapper _mapper;

        public CartService(ICartRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CartReadDto?> GetCartOfCustomerAsync(string CustomerId)
        {
            var cart = await _repo.GetCartByCustomerAsync(CustomerId);
            return cart == null ? null : _mapper.Map<CartReadDto>(cart);
        }

        public async Task<CartReadDto> AddToCartAsync(string CustomerId, CartItemCreateDto itemDto)
        {
            var cart = await _repo.GetCartByCustomerAsync(CustomerId);

            // Tạo cart mới nếu chưa có
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = CustomerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                await _repo.AddCartAsync(cart);
            }
            // Thêm hoặc cộng dồn sản phẩm
            var cartItem = new CartItem { ProductID = itemDto.ProductId, Quantity = itemDto.Quantity };
            await _repo.AddCartItemAsync(cart.CartId, cartItem);

            // Load lại cart mới nhất
            var updatedCart = await _repo.GetByIdAsync(cart.CartId);
            return _mapper.Map<CartReadDto>(updatedCart);
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int newQuantity)
        {
            return await _repo.UpdateCartItemAsync(cartItemId, newQuantity);
        }

        public async Task<bool> DeleteCartItemAsync(int cartItemId)
        {
            return await _repo.DeleteCartItemAsync(cartItemId);
        }
    }
}
