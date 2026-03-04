using Microsoft.EntityFrameworkCore;
using ProductAPI.Admin.Repositories;
using ProductAPI.Admin.Repositories.Implements;
using ProductAPI.Admin.Services;
using ProductAPI.Admin.Services.Implements;
using ProductAPI.Mappers;
using ProductAPI.Mappers.Implements;
using ProductAPI.Middlewares;
using ProductAPI.Models;
using ProductAPI.Repositories;
using ProductAPI.Repositories.Implements;
using ProductAPI.Services;
using ProductAPI.Services.Implements;
using ProductAPI.Helpers;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(opt =>
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductMapper, ProductMapper>();

builder.Services.AddScoped<IProductAdminService, ProductAdminService>();
builder.Services.AddScoped<IProductAdminRepository, ProductAdminRepository>();

builder.Services.AddScoped<ProductAPI.Admin.Repositories.ICategoryRepository, ProductAPI.Admin.Repositories.Implements.CategoryRepository>();
builder.Services.AddScoped<ProductAPI.Admin.Services.ICategoryService, ProductAPI.Admin.Services.Implements.CategoryService>();
builder.Services.AddScoped<ProductAPI.Admin.Mappers.ICategoryMapper, ProductAPI.Admin.Mappers.Implements.CategoryMapper>();

builder.Services.AddScoped<ProductAPI.Repositories.ICategoryRepository, ProductAPI.Repositories.Implements.CategoryRepository>();
builder.Services.AddScoped<ProductAPI.Services.ICategoryService, ProductAPI.Services.Implements.CategoryService>();
builder.Services.AddScoped<ICategoryMapper, CategoryMapper>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped(provider =>
{
    var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new CloudinaryDotNet.Account(
        config.CloudName,
        config.ApiKey,
        config.ApiSecret
    );
    return new Cloudinary(account);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// GLOBAL EXCEPTION
app.UseGlobalExceptionMiddleware();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
