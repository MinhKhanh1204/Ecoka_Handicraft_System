namespace ProductAPI.DTOs
{
    public class ProductDetailResponseDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }

        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }

        public int StockQuantity { get; set; }
        public string Status { get; set; }

        public CategoryDto Category { get; set; }

        public string MainImage { get; set; }
        public List<string> Images { get; set; }
    }
}
