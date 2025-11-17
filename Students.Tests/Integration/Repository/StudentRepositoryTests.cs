using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Students.Domain.Entities;
using Students.Infrastructure.Data;
using Students.Infrastructure.Repositories;
using Students.Tests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Students.Tests.Integration.Repository
{
    public class StudentRepositoryTests : IDisposable
    {
        private readonly StudentContext _context;
        private readonly StudentRepository _repository;

        public StudentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StudentContext(options);
            _repository = new StudentRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddStudentToDatabase()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent(withId: false);
            student.Id = Guid.NewGuid();

            // Act
            await _repository.AddAsync(student);

            // Assert
            var savedStudent = await _context.Students.FindAsync(student.Id);
            savedStudent.Should().NotBeNull();
            savedStudent!.Name.Should().Be(student.Name);
            savedStudent.Email.Should().Be(student.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnStudent_WhenExists()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent();
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(student.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(student.Id);
            result.Name.Should().Be(student.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStudents()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(3);
            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnFilteredStudents()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(5);
            students[0].Name = "João Silva";
            students[1].Name = "Maria Santos";
            students[2].Name = "João Pedro";
            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(s => s.Name.Contains("João"));

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Name.Contains("João"));
        }

        [Fact]
        public async Task FindAsync_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(2);
            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(s => s.Name == "NonExistent");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateStudentInDatabase()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent();
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Detach to simulate a fresh update
            _context.Entry(student).State = EntityState.Detached;

            var updatedStudent = new Student
            {
                Id = student.Id,
                Name = "Updated Name",
                Email = student.Email,
                Enrollment = student.Enrollment,
                CourseCurriculum = student.CourseCurriculum,
                PhoneNumbers = student.PhoneNumbers,
                Classes = student.Classes
            };

            // Act
            await _repository.UpdateAsync(updatedStudent);

            // Assert
            var result = await _context.Students.FindAsync(student.Id);
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStudentFromDatabase()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent();
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(student.Id);

            // Assert
            var result = await _context.Students.FindAsync(student.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();

            // Act
            await _repository.DeleteAsync(studentId);

            // Assert - No exception should be thrown
            var allStudents = await _repository.GetAllAsync();
            allStudents.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_ShouldPersistPhoneNumbers()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent();
            student.PhoneNumbers = new System.Collections.Generic.List<PhoneNumber>
            {
                new PhoneNumber { Ddd = 51, Number = 999999999, Description = "Celular" },
                new PhoneNumber { Ddd = 11, Number = 988888888, Description = "Casa" }
            };

            // Act
            await _repository.AddAsync(student);

            // Assert
            var saved = await _context.Students.FindAsync(student.Id);
            saved.Should().NotBeNull();
            saved!.PhoneNumbers.Should().HaveCount(2);
            saved.PhoneNumbers[0].Ddd.Should().Be(51);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistClasses()
        {
            // Arrange
            var classId1 = Guid.NewGuid();
            var classId2 = Guid.NewGuid();
            var student = TestDataGenerator.GenerateStudent();
            student.Classes = new System.Collections.Generic.List<Guid> { classId1, classId2 };

            // Act
            await _repository.AddAsync(student);

            // Assert
            var saved = await _context.Students.FindAsync(student.Id);
            saved.Should().NotBeNull();
            saved!.Classes.Should().HaveCount(2);
            saved.Classes.Should().Contain(classId1);
            saved.Classes.Should().Contain(classId2);
        }

        [Fact]
        public async Task FindAsync_ShouldFilterByEmail()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(3);
            var targetEmail = "target@test.com";
            students[1].Email = targetEmail;
            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(s => s.Email == targetEmail);

            // Assert
            result.Should().HaveCount(1);
            result.First().Email.Should().Be(targetEmail);
        }

        [Fact]
        public async Task FindAsync_ShouldFilterByEnrollment()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(3);
            var targetEnrollment = "TEST2024";
            students[0].Enrollment = targetEnrollment;
            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(s => s.Enrollment == targetEnrollment);

            // Assert
            result.Should().HaveCount(1);
            result.First().Enrollment.Should().Be(targetEnrollment);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
