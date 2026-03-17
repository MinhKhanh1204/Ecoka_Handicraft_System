using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProductAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
			CreateMap<Product, ProductDto>()
				.ForMember(d => d.OriginalPrice, o => o.MapFrom(s => s.Price))
				.ForMember(d => d.FinalPrice, o => o.MapFrom(s => s.Price - (s.Price * s.Discount / 100)))
				.ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.CategoryName))
				.ForMember(d => d.MainImage, o => o.MapFrom(s =>
					s.ProductImages
					 .Where(i => i.IsMain)
					 .Select(i => i.ImageURL)
					 .FirstOrDefault()
				));

			CreateMap<Product, ProductDetailResponseDto>()
				.ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductID))
				.ForMember(d => d.FinalPrice, o => o.MapFrom(s => s.Price - (s.Price * s.Discount / 100)))
				.ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
				.ForMember(d => d.MainImage, o => o.MapFrom(s =>
					s.ProductImages
					 .Where(i => i.IsMain)
					 .Select(i => i.ImageURL)
					 .FirstOrDefault()
				))
				.ForMember(d => d.Images, o => o.MapFrom(s =>
					s.ProductImages.Select(i => i.ImageURL)
				));

			CreateMap<Category, CategoryDto>();
        }
    }
}
