using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using FeedbackAPI.Hubs;
using FeedbackAPI.Models;
using FeedbackAPI.Profiles;
using FeedbackAPI.Repositories;
using FeedbackAPI.Repositories.Implements;
using FeedbackAPI.Services;
using FeedbackAPI.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// ========================
// OData EDM model
// ========================
var edmBuilder = new ODataConventionModelBuilder();
edmBuilder.EntitySet<Feedback>("feedbacks");

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
// Controllers + OData
// ========================
builder.Services.AddControllers()
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
// SignalR
// ========================
builder.Services.AddSignalR();

// ========================
// AutoMapper + EF Core
// ========================
builder.Services.AddAutoMapper(typeof(FeedbackProfile));
builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================
// Application services
// ========================
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

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
app.UseCors("AllowConfigured");
app.UseAuthorization();

app.MapControllers();

// ========================
// SignalR Hub endpoint
// ========================
app.MapHub<FeedbackHub>("/hubs/feedback");

app.Run();
