namespace MVCApplication.Models
{
    public class ProductListDto
    {
        public string ProductID { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = null!;
        public string? MainImage { get; set; }
    }

    public class ProductDetailDto
    {
        public string ProductID { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<string> Images { get; set; } = new();
    }

    public class ProductCreateDto
    {
        public int CategoryID { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductUpdateDto
    {
        public int CategoryID { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = null!;
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public string ImageUrl { get; set; } = null!;
        public bool IsMain { get; set; }
    }
}
