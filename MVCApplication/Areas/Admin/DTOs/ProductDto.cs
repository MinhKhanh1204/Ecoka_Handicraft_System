using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Areas.Admin.DTOs
{
    public class ReadProductDto
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

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Vui lòng chọn Category")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }
        public string? Material { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount phải từ 0 đến 100")]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; }

        public List<CreateProductImageDto> Images { get; set; } = new();
    }

    public class CreateProductImageDto
    {
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsMain { get; set; }
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Vui lòng chọn Category")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }
        public string? Material { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount phải từ 0 đến 100")]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        public List<UpdateProductImageDto> Images { get; set; } = new();
    }

    public class UpdateProductImageDto
    {
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsMain { get; set; }
    }
}
