using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Students.Application.Interfaces;
using Students.Application.Services;
using Students.Domain.Interfaces;
using Students.Infrastructure.Data;
using Students.Infrastructure.Repositories;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

// Configure Health Checks (equivalente ao Spring Boot Actuator)
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" }
    );

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "students", serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());

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

app.UseAuthorization();

// Expose Prometheus metrics via OpenTelemetry
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();

// Health check endpoint padronizado (formato compatível com Actuator)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status == HealthStatus.Healthy ? "UP" : "DOWN",
            components = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status == HealthStatus.Healthy ? "UP" : "DOWN",
                    details = e.Value.Description
                }
            )
        });
        await context.Response.WriteAsync(result);
    }
});

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
