using FluentAssertions;
using Moq;
using Students.Application.Services;
using Students.Domain.Entities;
using Students.Domain.Interfaces;
using Students.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Students.Tests.Unit.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repositoryMock;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _repositoryMock = new Mock<IStudentRepository>();
            _service = new StudentService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateStudentAsync_ShouldReturnStudentDto_WhenValidDataProvided()
        {
            // Arrange
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateStudentAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.Email.Should().Be(createDto.Email);
            result.Enrollment.Should().Be(createDto.Enrollment);
            result.CourseCurriculum.Should().Be(createDto.CourseCurriculum);
            result.Id.Should().NotBe(Guid.Empty);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
        }

        [Fact]
        public async Task GetStudentByIdAsync_ShouldReturnStudentDto_WhenStudentExists()
        {
            // Arrange
            var student = TestDataGenerator.GenerateStudent();
            _repositoryMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);

            // Act
            var result = await _service.GetStudentByIdAsync(student.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(student.Id);
            result.Name.Should().Be(student.Name);
            result.Email.Should().Be(student.Email);
            _repositoryMock.Verify(r => r.GetByIdAsync(student.Id), Times.Once);
        }

        [Fact]
        public async Task GetStudentByIdAsync_ShouldReturnNull_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student)null!);

            // Act
            var result = await _service.GetStudentByIdAsync(studentId);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents_WhenNoFiltersProvided()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(5);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                .ReturnsAsync(students);

            // Act
            var result = await _service.GetAllStudentsAsync(null, null, null);

            // Assert
            result.Should().HaveCount(5);
            _repositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldFilterByName_WhenNameProvided()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(3);
            var targetName = "João";
            students[0].Name = "João Silva";
            students[1].Name = "Maria Santos";
            students[2].Name = "João Pedro";

            var filteredStudents = students.Where(s => s.Name.Contains(targetName)).ToList();
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                .ReturnsAsync(filteredStudents);

            // Act
            var result = await _service.GetAllStudentsAsync(targetName, null, null);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Name.Contains(targetName));
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldFilterByEmail_WhenEmailProvided()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(2);
            var targetEmail = "test@pucrs.br";
            students[0].Email = targetEmail;

            var filteredStudents = students.Where(s => s.Email == targetEmail).ToList();
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                .ReturnsAsync(filteredStudents);

            // Act
            var result = await _service.GetAllStudentsAsync(null, null, targetEmail);

            // Assert
            result.Should().HaveCount(1);
            result.First().Email.Should().Be(targetEmail);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldFilterByEnrollment_WhenEnrollmentProvided()
        {
            // Arrange
            var students = TestDataGenerator.GenerateStudents(2);
            var targetEnrollment = "2024001";
            students[0].Enrollment = targetEnrollment;

            var filteredStudents = students.Where(s => s.Enrollment == targetEnrollment).ToList();
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                .ReturnsAsync(filteredStudents);

            // Act
            var result = await _service.GetAllStudentsAsync(null, targetEnrollment, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Enrollment.Should().Be(targetEnrollment);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldUpdateStudent_WhenStudentExists()
        {
            // Arrange
            var existingStudent = TestDataGenerator.GenerateStudent();
            var updateDto = TestDataGenerator.GenerateUpdateStudentDto();

            _repositoryMock.Setup(r => r.GetByIdAsync(existingStudent.Id)).ReturnsAsync(existingStudent);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

            // Act
            await _service.UpdateStudentAsync(existingStudent.Id, updateDto);

            // Assert
            _repositoryMock.Verify(r => r.GetByIdAsync(existingStudent.Id), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Student>(s =>
                s.Name == updateDto.Name &&
                s.Email == updateDto.Email &&
                s.Enrollment == updateDto.Enrollment
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldNotCallUpdate_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var updateDto = TestDataGenerator.GenerateUpdateStudentDto();

            _repositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student)null!);

            // Act
            await _service.UpdateStudentAsync(studentId, updateDto);

            // Assert
            _repositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldUpdateOnlyProvidedFields_WhenPartialUpdateProvided()
        {
            // Arrange
            var existingStudent = TestDataGenerator.GenerateStudent();
            var updateDto = new Application.DTOs.UpdateStudentDto
            {
                Name = "Updated Name",
                // Other fields are null - should keep existing values
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(existingStudent.Id)).ReturnsAsync(existingStudent);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

            var originalEmail = existingStudent.Email;
            var originalEnrollment = existingStudent.Enrollment;

            // Act
            await _service.UpdateStudentAsync(existingStudent.Id, updateDto);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Student>(s =>
                s.Name == "Updated Name" &&
                s.Email == originalEmail &&
                s.Enrollment == originalEnrollment
            )), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldCallRepositoryDelete()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(studentId)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteStudentAsync(studentId);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(studentId), Times.Once);
        }

        [Fact]
        public async Task CreateStudentAsync_ShouldMapPhoneNumbersCorrectly()
        {
            // Arrange
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.PhoneNumbers = new List<Application.DTOs.PhoneNumberDto>
            {
                new Application.DTOs.PhoneNumberDto { Ddd = 51, Number = 999999999, Description = "Celular" },
                new Application.DTOs.PhoneNumberDto { Ddd = 11, Number = 988888888, Description = "Casa" }
            };

            Student capturedStudent = null!;
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Student>()))
                .Callback<Student>(s => capturedStudent = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateStudentAsync(createDto);

            // Assert
            capturedStudent.Should().NotBeNull();
            capturedStudent.PhoneNumbers.Should().HaveCount(2);
            capturedStudent.PhoneNumbers[0].Ddd.Should().Be(51);
            capturedStudent.PhoneNumbers[0].Number.Should().Be(999999999);
            capturedStudent.PhoneNumbers[0].Description.Should().Be("Celular");
        }

        [Fact]
        public async Task CreateStudentAsync_ShouldMapClassesCorrectly()
        {
            // Arrange
            var classId1 = Guid.NewGuid();
            var classId2 = Guid.NewGuid();
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Classes = new List<Guid> { classId1, classId2 };

            Student capturedStudent = null!;
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Student>()))
                .Callback<Student>(s => capturedStudent = s)
                .Returns(Task.CompletedTask);

            // Act
            await _service.CreateStudentAsync(createDto);

            // Assert
            capturedStudent.Should().NotBeNull();
            capturedStudent.Classes.Should().HaveCount(2);
            capturedStudent.Classes.Should().Contain(classId1);
            capturedStudent.Classes.Should().Contain(classId2);
        }
    }
}
