namespace ProductAPI.DTOs
{
	public class ProductFilterRequestDto
	{
		public string? TxtSearch { get; set; }
		public int? CategoryId { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 9;
	}
}
