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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Feedback API",
        Version = "v1"
    });

    // 🔐 Thêm định nghĩa bảo mật JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token của bạn: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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
// NOTE: all inter-service calls now go through the API Gateway (port 5000)
builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5000");
});
builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5000");
});

// ========================
// Authentication / JWT
// ========================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Prefer Authorization header, but if missing try cookie named "AccessToken"
                if (string.IsNullOrEmpty(context.Token))
                {
                    var token = context.Request.Cookies["AccessToken"];
                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;
                }
                return Task.CompletedTask;
            }
        };

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
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowConfigured");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
