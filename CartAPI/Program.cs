using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CartAPI.DTOs;
using CartAPI.Profiles;
using CartAPI.Repositories.Implements;
using CartAPI.Services.Implements;
using CartAPI.Repositories.Interface;
using CartAPI.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// ========================
// OData EDM model
// ========================
var edmBuilder = new ODataConventionModelBuilder();
edmBuilder.EntitySet<Cart>("carts");
edmBuilder.EntitySet<CartItem>("cartItems");

// ========================
// CORS (for AJAX clients, FE PORT: 7010)
// ========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7010") // Đúng port FE
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ========================
// JWT Authentication
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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.ContainsKey("AccessToken"))
                    ctx.Token = ctx.Request.Cookies["AccessToken"];
                return Task.CompletedTask;
            }
        };
    });

// ========================
// Controllers + OData
// ========================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles)
    .AddOData(opt =>
        opt.AddRouteComponents("odata", edmBuilder.GetEdmModel())
           .Filter()
           .OrderBy()
           .Select()
           .SetMaxTop(100)
           .Count()
           .Expand());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================
// AutoMapper + EF Core
// ========================
builder.Services.AddAutoMapper(typeof(CartProfile));
builder.Services.AddDbContext<CartDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================
// Application services & repository
// ========================
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

// ========================
// HTTP pipeline
// ========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");     // <<< ĐỔI LẠI TÊN POLICY ĐÃ TẠO
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();