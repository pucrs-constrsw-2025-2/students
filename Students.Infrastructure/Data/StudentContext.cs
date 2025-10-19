using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Students.Domain.Entities;
using System.Collections.Generic;
using System.Text.Json;

namespace Students.Infrastructure.Data
{
    public class StudentContext : DbContext
    {
        public StudentContext(DbContextOptions<StudentContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Enrollment).IsRequired();
                entity.HasIndex(e => e.Enrollment).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CourseCurriculum).IsRequired();

                entity.Property(e => e.PhoneNumbers)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<PhoneNumber>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("jsonb");

                var guidListConverter = new ValueConverter<List<Guid>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null) ?? new List<Guid>()
                );

                entity.Property(e => e.Classes)
                    .HasConversion(guidListConverter)
                    .HasColumnType("jsonb");
            });
        }
    }
}
