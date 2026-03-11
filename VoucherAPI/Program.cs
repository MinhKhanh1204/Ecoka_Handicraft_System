using Microsoft.EntityFrameworkCore;
using VoucherAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(VoucherAPI.Profiles.VoucherProfile));
builder.Services.AddScoped<VoucherAPI.Repositories.IVoucherRepository, VoucherAPI.Repositories.VoucherRepository>();
builder.Services.AddScoped<VoucherAPI.Services.IVoucherService, VoucherAPI.Services.VoucherService>();

// Admin - Voucher Management
builder.Services.AddScoped<VoucherAPI.Admin.Repositories.IVoucherAdminRepository, VoucherAPI.Admin.Repositories.Implements.VoucherAdminRepository>();
builder.Services.AddScoped<VoucherAPI.Admin.Services.IVoucherAdminService, VoucherAPI.Admin.Services.Implements.VoucherAdminService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
