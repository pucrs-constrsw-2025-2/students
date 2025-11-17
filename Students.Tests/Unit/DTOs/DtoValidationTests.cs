using FluentAssertions;
using Students.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Students.Tests.Unit.DTOs
{
    public class CreateStudentDtoValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void CreateStudentDto_ShouldBeValid_WhenAllRequiredFieldsProvided()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "joao.silva@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                PhoneNumbers = new List<PhoneNumberDto>(),
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenNameIsMissing()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = null!,
                Enrollment = "2024001",
                Email = "test@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenEnrollmentIsMissing()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = null!,
                Email = "test@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Enrollment"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenEmailIsMissing()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = null!,
                CourseCurriculum = "Engenharia de Software",
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenEmailIsInvalid()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "invalid-email",
                CourseCurriculum = "Engenharia de Software",
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenCourseCurriculumIsMissing()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "test@pucrs.br",
                CourseCurriculum = null!,
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("CourseCurriculum"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeInvalid_WhenClassesIsEmpty()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "test@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                Classes = new List<Guid>() // Empty list
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Classes"));
        }

        [Fact]
        public void CreateStudentDto_ShouldBeValid_WithEmptyPhoneNumbers()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "test@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                PhoneNumbers = new List<PhoneNumberDto>(),
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void CreateStudentDto_ShouldBeValid_WithMultipleClasses()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "João Silva",
                Enrollment = "2024001",
                Email = "test@pucrs.br",
                CourseCurriculum = "Engenharia de Software",
                PhoneNumbers = new List<PhoneNumberDto>(),
                Classes = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }
    }

    public class UpdateStudentDtoValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void UpdateStudentDto_ShouldBeValid_WithAllFieldsProvided()
        {
            // Arrange
            var dto = new UpdateStudentDto
            {
                Name = "João Silva Updated",
                Enrollment = "2024001",
                Email = "joao.updated@pucrs.br",
                CourseCurriculum = "Ciência da Computação",
                PhoneNumbers = new List<PhoneNumberDto>(),
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UpdateStudentDto_ShouldBeValid_WithNullFields()
        {
            // Arrange
            var dto = new UpdateStudentDto
            {
                Name = "João Silva",
                // Other fields are null - this is allowed for updates
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UpdateStudentDto_ShouldBeInvalid_WhenEmailIsInvalid()
        {
            // Arrange
            var dto = new UpdateStudentDto
            {
                Name = "João Silva",
                Email = "invalid-email",
                Classes = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UpdateStudentDto_ShouldBeInvalid_WhenClassesIsEmpty()
        {
            // Arrange
            var dto = new UpdateStudentDto
            {
                Name = "João Silva",
                Email = "test@pucrs.br",
                Classes = new List<Guid>() // Empty list
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Classes"));
        }
    }

    public class PhoneNumberDtoTests
    {
        [Fact]
        public void PhoneNumberDto_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var phoneDto = new PhoneNumberDto
            {
                Ddd = 51,
                Number = 999999999,
                Description = "Celular"
            };

            // Assert
            phoneDto.Ddd.Should().Be(51);
            phoneDto.Number.Should().Be(999999999);
            phoneDto.Description.Should().Be("Celular");
        }

        [Theory]
        [InlineData(11, 999999999, "Celular")]
        [InlineData(51, 988888888, "Casa")]
        [InlineData(21, 977777777, "Trabalho")]
        public void PhoneNumberDto_ShouldAcceptVariousValues(int ddd, int number, string description)
        {
            // Arrange & Act
            var phoneDto = new PhoneNumberDto
            {
                Ddd = ddd,
                Number = number,
                Description = description
            };

            // Assert
            phoneDto.Ddd.Should().Be(ddd);
            phoneDto.Number.Should().Be(number);
            phoneDto.Description.Should().Be(description);
        }
    }
}
