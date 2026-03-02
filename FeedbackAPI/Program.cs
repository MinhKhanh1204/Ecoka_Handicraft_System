using Microsoft.EntityFrameworkCore;
using FeedbackAPI.Models;
using FeedbackAPI.Profiles;
using FeedbackAPI.Repositories;
using FeedbackAPI.Repositories.Implements;
using FeedbackAPI.Services;
using FeedbackAPI.Services.Implements;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(FeedbackProfile));
builder.Services.AddDbContext<DBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IFeedbackReplyRepository, FeedbackReplyRepository>();
builder.Services.AddScoped<IFeedbackReplyService, FeedbackReplyService>();

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
