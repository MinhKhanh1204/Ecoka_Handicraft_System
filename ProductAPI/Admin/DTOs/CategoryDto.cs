namespace ProductAPI.Admin.DTOs
{
    public class ReadCategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; }
    }

    public class CategoryCreateDto
    {
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class CategoryUpdateDto
    {
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
    }
}
