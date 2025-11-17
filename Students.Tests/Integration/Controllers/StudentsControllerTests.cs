using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Students.Infrastructure.Data;
using Students.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Students.Application.DTOs;

namespace Students.Tests.Integration.Controllers
{
    public class StudentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StudentsControllerTests(WebApplicationFactory<Program> factory)
        {
            // Use a static database name so all requests in a test use the same database
            var testDbName = "TestDb_" + Guid.NewGuid();
            
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureTestServices(services =>
                {
                    // Register in-memory database (Program.cs skips DbContext registration in Testing environment)
                    services.AddDbContext<StudentContext>(options =>
                    {
                        options.UseInMemoryDatabase(testDbName);
                    });

                    // Replace JWT authentication with fake authentication
                    services.AddAuthentication(defaultScheme: "FakeScheme")
                        .AddScheme<AuthenticationSchemeOptions, FakeJwtAuthenticationHandler>(
                            "FakeScheme", options => { });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateStudent_ShouldReturnCreated_WhenValidDataProvided()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<StudentDto>();
            result.Should().NotBeNull();
            result!.Name.Should().Be(createDto.Name);
            result.Email.Should().Be(createDto.Email);
            result.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenInvalidDataProvided()
        {
            // Arrange
            
            var createDto = new CreateStudentDto(); // Empty - missing required fields

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetStudentById_ShouldReturnOk_WhenStudentExists()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            var createResponse = await _client.PostAsJsonAsync("/api/v1/students", createDto);
            var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            // Act
            var response = await _client.GetAsync($"/api/v1/students/{createdStudent!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<StudentDto>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdStudent.Id);
        }

        [Fact]
        public async Task GetStudentById_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync($"/api/v1/students/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllStudents_ShouldReturnOk_WithListOfStudents()
        {
            // Arrange
            // Create some students first
            for (int i = 0; i < 3; i++)
            {
                var createDto = TestDataGenerator.GenerateCreateStudentDto();
                await _client.PostAsJsonAsync("/api/v1/students", createDto);
            }

            // Act
            var response = await _client.GetAsync("/api/v1/students");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<StudentDto[]>();
            result.Should().NotBeNull();
            result!.Length.Should().BeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public async Task GetAllStudents_ShouldFilterByName_WhenNameQueryProvided()
        {
            // Arrange

            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Name = "João Test Filter";
            await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Act
            var response = await _client.GetAsync("/api/v1/students?name=João");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<StudentDto[]>();
            result.Should().NotBeNull();
            result.Should().Contain(s => s.Name.Contains("João"));
        }

        [Fact]
        public async Task UpdateStudent_ShouldReturnNoContent_WhenValidDataProvided()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            var createResponse = await _client.PostAsJsonAsync("/api/v1/students", createDto);
            var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            var updateDto = TestDataGenerator.GenerateUpdateStudentDto();

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/students/{createdStudent!.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify update
            var getResponse = await _client.GetAsync($"/api/v1/students/{createdStudent.Id}");
            var updatedStudent = await getResponse.Content.ReadFromJsonAsync<StudentDto>();
            updatedStudent!.Name.Should().Be(updateDto.Name);
        }

        [Fact]
        public async Task DeleteStudent_ShouldReturnNoContent_WhenStudentExists()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            var createResponse = await _client.PostAsJsonAsync("/api/v1/students", createDto);
            var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/v1/students/{createdStudent!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/v1/students/{createdStudent.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Email = "invalid-email"; // Invalid email format

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenClassesListIsEmpty()
        {
            // Arrange
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Classes = new System.Collections.Generic.List<Guid>(); // Empty classes

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
