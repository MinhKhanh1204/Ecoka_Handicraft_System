using AccountAPI.Helpers;
using AccountAPI.Mappers.Implements;
using AccountAPI.Mappers;
using AccountAPI.Repositories.Implements;
using AccountAPI.Repositories;
using AccountAPI.Services.Implements;
using AccountAPI.Services;
using Microsoft.EntityFrameworkCore;
using AccountAPI.Models;
using AccountAPI.CustomFormatter;
using AccountAPI.Middlewares;
using System.Text.Json;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountMapper, AccountMapper>();
builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CLOUDARY
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;

    var account = new CloudinaryDotNet.Account(
        config.CloudName,
        config.ApiKey,
        config.ApiSecret
    );

    return new Cloudinary(account);
});

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

app.UseStaticFiles();

app.MapControllers();

app.Run();
