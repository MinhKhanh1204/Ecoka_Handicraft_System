namespace MVCApplication.Models
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
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

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }
    }
}
