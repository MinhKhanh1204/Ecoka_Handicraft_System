using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatbotAPI;
using ChatbotAPI.Middlewares;
using ChatbotAPI.Models;
using ChatbotAPI.Services;
using ChatbotAPI.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// CORS - Allow all for testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Required: bind ApiKey / Model from appsettings.json — without this, IOptions<GeminiSettings> stays empty and Gemini calls fail.
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(); // required for IMemoryCache in ContextBuilderService
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IContextBuilderService, ContextBuilderService>();
builder.Services.AddScoped<IPromptBuilderService, PromptBuilderService>();

// All internal APIs use HTTPS with JWT auth — relax SSL validation only in Development.
var isDev = builder.Environment.IsDevelopment();
builder.Services.AddHttpClient("SecureClient").ConfigurePrimaryHttpMessageHandler(() =>
{
    var h = new HttpClientHandler();
    if (isDev) h.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return h;
});
builder.Services.AddHttpClient();

builder.Services.AddControllers();

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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Chatbot API",
        Version = "v1",
        Description = "AI-powered chatbot service using Gemini for Ecoka Handicraft"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter token: Bearer {your_token}"
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

var app = builder.Build();

app.UseCors("AllowAll");
app.UseGlobalExceptionMiddleware();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbot API v1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();