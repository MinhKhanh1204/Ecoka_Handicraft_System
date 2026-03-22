using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FeedbackAPI.Models;
using FeedbackAPI.Profiles;
using FeedbackAPI.Repositories;
using FeedbackAPI.Repositories.Implements;
using FeedbackAPI.Services;
using FeedbackAPI.Services.Implements;
using FeedbackAPI.Helpers;
using FeedbackAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ========================
// CORS (for AJAX clients)
// ========================
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfigured", policy =>
    {
        if (builder.Environment.IsDevelopment() || allowedOrigins.Length == 0)
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// ========================
// Controllers
// ========================
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================
// AutoMapper + EF Core
// ========================
builder.Services.AddAutoMapper(typeof(FeedbackProfile));
builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================
// Cloudinary Settings
// ========================
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// ========================
// Application services
// ========================
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddHttpContextAccessor();
// NOTE: base addresses must match the actual running API ports (see AccountAPI and OrderAPI launchSettings)
builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
{
    c.BaseAddress = new Uri("https://localhost:7018");
});
builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
{
    c.BaseAddress = new Uri("https://localhost:7289");
});

// ========================
// Authentication / JWT
// ========================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// ========================
// HTTP pipeline
// ========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowConfigured");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
