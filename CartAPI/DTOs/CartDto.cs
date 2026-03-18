namespace CartAPI.DTOs
{
    public class CartReadDto
    {
        public int CartId { get; set; }          // PascalCase + "Id" (chuẩn C#)
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemReadDto> CartItems { get; set; } = new(); // tránh null
    }

    public class CartItemReadDto
    {
        public int CartItemId { get; set; }
        public string ProductId { get; set; }  // string (GUID/string ProductId)
        public int Quantity { get; set; }
    }

    public class CartItemCreateDto
    {
        public string ProductId { get; set; }  // string (GUID/string ProductId)
        public int Quantity { get; set; }
    }

    public class CartItemUpdateDto
    {
        public int Quantity { get; set; }
    }
}