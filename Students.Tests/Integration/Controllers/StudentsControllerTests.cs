using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Students.Infrastructure.Data;
using Students.Tests.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Students.Application.DTOs;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Students.Tests.Integration.Controllers
{
    public class StudentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StudentsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registrations
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<StudentContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    var contextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(StudentContext));
                    if (contextDescriptor != null)
                    {
                        services.Remove(contextDescriptor);
                    }

                    // Add in-memory database for testing (no EnsureCreated to avoid conflict)
                    services.AddDbContext<StudentContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        private string GenerateMockJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ThisIsASecretKeyForTestingPurposesOnly1234567890");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                    new Claim(ClaimTypes.Email, "test@pucrs.br"),
                    new Claim(ClaimTypes.Role, "administrator")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task GetStudentById_ShouldReturnUnauthorized_WhenNoTokenProvided()
        {
            // Act
            var response = await _client.GetAsync($"/api/v1/students/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task CreateStudent_ShouldReturnUnauthorized_WhenNoTokenProvided()
        {
            // Arrange
            var createDto = TestDataGenerator.GenerateCreateStudentDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(Skip = "Requires real JWT configuration - use for E2E tests with actual auth service")]
        public async Task CreateStudent_ShouldReturnCreated_WhenValidDataProvided()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenInvalidDataProvided()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var createDto = new CreateStudentDto(); // Empty - missing required fields

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task GetStudentById_ShouldReturnOk_WhenStudentExists()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task GetStudentById_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/api/v1/students/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task GetAllStudents_ShouldReturnOk_WithListOfStudents()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task GetAllStudents_ShouldFilterByName_WhenNameQueryProvided()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task UpdateStudent_ShouldReturnNoContent_WhenValidDataProvided()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task DeleteStudent_ShouldReturnNoContent_WhenStudentExists()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
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

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Email = "invalid-email"; // Invalid email format

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Requires real JWT configuration")]
        public async Task CreateStudent_ShouldReturnBadRequest_WhenClassesListIsEmpty()
        {
            // Arrange
            var token = GenerateMockJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var createDto = TestDataGenerator.GenerateCreateStudentDto();
            createDto.Classes = new System.Collections.Generic.List<Guid>(); // Empty classes

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
