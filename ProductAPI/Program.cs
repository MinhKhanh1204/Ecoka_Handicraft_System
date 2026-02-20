using Microsoft.EntityFrameworkCore;
using ProductAPI.Repositories.Implements;
using ProductAPI.Repositories;
using ProductAPI.Services.Implements;
using ProductAPI.Services;
using ProductAPI.Models;
using ProductAPI.Mappers.Implements;
using ProductAPI.Mappers;
using ProductAPI.CustomFormatter;
using ProductAPI.Middlewares;
using ProductAPI.Admin.Repositories.Implements;
using ProductAPI.Admin.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(opt =>
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductMapper, ProductMapper>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryMapper, CategoryMapper>();

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
