namespace MVCApplication.Models.DTOs
{
	public class ProductDto
	{
		public string ProductID { get; set; }
		public string ProductName { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
		public string CategoryName { get; set; } = null!;
		public string Description { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal FinalPrice { get; set; }
		public string? MainImage { get; set; }
	}
}

