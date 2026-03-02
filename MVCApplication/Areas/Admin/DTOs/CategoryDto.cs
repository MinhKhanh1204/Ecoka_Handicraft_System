using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Areas.Admin.DTOs
{
    public class ReadCategoryDto
    {
        public int CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }

    public class CategoryCreateDto
    {
        [Required]
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }

    public class CategoryUpdateDto
    {
        [Required]
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}
