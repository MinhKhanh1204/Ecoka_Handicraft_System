using MVCApplication.Extensions;
using MVCApplication.Services;
using MVCApplication.Services.Implements;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Areas.Admin.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAccountService, AccountService>();
//builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Read gateway base URL from configuration (appsettings.json)
var gatewayBase = builder.Configuration["ApiGateway:ApiBaseUrl"] ?? "https://localhost:5000/";
// AccountService → PUBLIC (No attach JWT)
builder.Services.AddGatewayPublicClient<IAccountService, AccountService>(gatewayBase);
builder.Services.AddGatewayAuthClient<IOrderService, OrderService>(gatewayBase);
//builder.Services.AddGatewayAuthClient<IProductService, ProductService>(gatewayBase);
//builder.Services.AddGatewayAuthClient<IProductService, MVCApplication.Services.Implements.ProductService>(gatewayBase);
builder.Services.AddGatewayAuthClient<MVCApplication.Areas.Admin.Services.IProductAdminService, MVCApplication.Areas.Admin.Services.Implements.ProductService>(gatewayBase);
builder.Services.AddGatewayAuthClient<ICategoryService, CategoryService>(gatewayBase);

//setting authen
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Get JWT from Cookie
                context.Token = context.Request.Cookies["AccessToken"];
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.Redirect("/Account/AccessDenied");
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            NameClaimType = "username",
            RoleClaimType = ClaimTypes.Role
		};
    });

builder.Services.AddAuthorization();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
