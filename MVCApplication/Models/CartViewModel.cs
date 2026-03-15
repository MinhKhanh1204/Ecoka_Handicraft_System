namespace MVCApplication.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public string CustomerId { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        
        // Enrich from ProductAPI
        public string ProductName { get; set; } = "Unknown Product";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "";
        public int StockQuantity { get; set; }
    }
}