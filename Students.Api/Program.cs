using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Students.Application.Interfaces;
using Students.Application.Services;
using Students.Domain.Interfaces;
using Students.Infrastructure.Data;
using Students.Infrastructure.Repositories;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// Add support for controllers and JSON Patch
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Configure the database context to use PostgreSQL
builder.Services.AddDbContext<StudentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services and repositories for Dependency Injection
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();


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

// Optional HTTPS redirection: enable only if explicitly configured
var useHttpsRedirection = app.Configuration.GetValue<bool?>("UseHttpsRedirection") ?? false;
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

// Prometheus HTTP request metrics middleware
app.UseHttpMetrics();

app.UseAuthorization();

// Expose Prometheus metrics at /actuator/prometheus
app.MapMetrics("/actuator/prometheus");

app.MapControllers();

// Simple health endpoint for container health checks
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Ensure database is ready: if no migrations exist, create schema from model; otherwise migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudentContext>();
    var hasMigrations = dbContext.Database.GetMigrations().Any();
    if (hasMigrations)
    {
        dbContext.Database.Migrate();
    }
    else
    {
        var creator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
        if (!creator.Exists())
        {
            creator.Create();
        }
        try
        {
            // Create tables for the current model if they don't exist
            creator.CreateTables();
        }
        catch
        {
            // Ignore if tables already exist
        }
    }
}

app.Run();
