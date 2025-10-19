using Microsoft.EntityFrameworkCore;
using Students.Application.Interfaces;
using Students.Application.Services;
using Students.Domain.Interfaces;
using Students.Infrastructure.Data;
using Students.Infrastructure.Repositories;

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Automatically apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudentContext>();
    dbContext.Database.Migrate();
}

app.Run();
