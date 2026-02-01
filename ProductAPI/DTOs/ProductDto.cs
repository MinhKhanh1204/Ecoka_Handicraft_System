namespace ProductAPI.DTOs
{
	public class ProductDto
	{
		public string ProductID { get; set; }
		public string ProductName { get; set; } = null!;
		public string CategoryName { get; set; } = null!;
		public decimal OriginalPrice { get; set; }
		public decimal FinalPrice { get; set; }
		public string? MainImage { get; set; }
	}
}

