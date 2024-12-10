using Microsoft.EntityFrameworkCore;
using SnowflakeID;
using SnowflakeIDGenerator.Example.Web.WithEF.dbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSnowflakeIdGeneratorService();

var connectionString = builder.Configuration.GetConnectionString("Weather") ?? "Data Source=Weather.db";

builder.Services.AddDbContext<WeatherDbContext>(options => options.UseSqlite(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
