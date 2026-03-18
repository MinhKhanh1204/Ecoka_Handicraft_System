using AccountAPI.Helpers;
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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// Admin Customer Management
using AccountAPI.Admin.Repositories;
using AccountAPI.Admin.Repositories.Implements;
using AccountAPI.Admin.Services;
using AccountAPI.Admin.Services.Implements;
using AccountAPI.Admin.Mappers;
using AccountAPI.Admin.Mappers.Implements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Forgot password: 3 requests per 5 minutes
    options.AddPolicy("ForgotPasswordPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Reset password: 10 attempts per 15 minutes (prevent brute force token)
    options.AddPolicy("ResetPasswordPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<PasswordResetSettings>(builder.Configuration.GetSection("PasswordReset"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Admin Customer Management
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerMapper, CustomerMapper>();

builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

//Mapping
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Account API",
		Version = "v1"
	});

	// 🔐 Add JWT Auth
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Nhập token dạng: Bearer {your_token}"
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

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
app.UseRateLimiter();

app.UseStaticFiles();

app.MapControllers();

app.Run();
