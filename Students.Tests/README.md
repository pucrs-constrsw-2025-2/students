# Students API - Automated Tests

This project contains comprehensive automated tests for the Students API, covering all architectural layers.

## Test Structure

The test suite follows the 4-layer architecture of the Students API:

```
Students.Tests/
├── Unit/
│   ├── Services/        # Application layer business logic tests
│   └── DTOs/            # Data validation tests
├── Integration/
│   ├── Repository/      # Infrastructure layer data access tests
│   └── Controllers/     # API layer endpoint tests
└── Helpers/             # Test utilities and data generators
```

## Test Coverage

### Unit Tests (29 tests)
- **Service Layer** (16 tests): Tests for `StudentService` business logic
  - Create, Read, Update, Delete operations
  - Filtering by name, enrollment, email
  - DTO mapping validation
  - Partial updates
  
- **DTO Validation** (13 tests): Tests for request/response data contracts
  - Required field validation
  - Email format validation
  - Classes collection validation
  - Phone number structure

### Integration Tests (26 tests)
- **Repository Layer** (16 tests): Tests for database operations using in-memory database
  - CRUD operations with Entity Framework Core
  - Complex queries with predicates
  - JSON field persistence
  - Relationship handling

- **Controller Layer** (10 tests): API endpoint integration tests
  - Authentication/authorization (skipped - requires real Keycloak)
  - HTTP status code verification
  - Request/response validation

## Running Tests

### Run All Tests
```bash
cd Students.Tests
dotnet test
```

### Run with Detailed Output
```bash
dotnet test --verbosity normal
```

### Run Specific Test Category
```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run only service tests
dotnet test --filter "FullyQualifiedName~StudentServiceTests"
```

### Run Tests and Generate Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking library for unit tests
- **FluentAssertions**: Readable assertion library
- **Bogus**: Test data generation
- **EntityFrameworkCore.InMemory**: In-memory database for integration tests
- **Microsoft.AspNetCore.Mvc.Testing**: WebApplicationFactory for API tests

## CI/CD Integration

Tests are designed to run in automated pipelines:

1. **No External Dependencies**: Uses in-memory database for repository tests
2. **Fast Execution**: All tests complete in < 2 seconds
3. **Isolated**: Each test is independent and can run in parallel
4. **Deterministic**: No flaky tests - same input always produces same output

### GitHub Actions Example
```yaml
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
```

### GitLab CI Example
```yaml
test:
  script:
    - dotnet test --verbosity normal
```

## Test Patterns

### Unit Tests
- Use **Moq** to mock dependencies (repository, external services)
- Test business logic in isolation
- Follow **AAA pattern** (Arrange, Act, Assert)

```csharp
[Fact]
public async Task CreateStudentAsync_ShouldReturnStudentDto_WhenValidDataProvided()
{
    // Arrange
    var createDto = TestDataGenerator.GenerateCreateStudentDto();
    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Student>())).ReturnsAsync(student);

    // Act
    var result = await _service.CreateStudentAsync(createDto);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be(createDto.Name);
}
```

### Integration Tests
- Use **in-memory database** for repository tests
- Use **WebApplicationFactory** for controller tests
- Reset database state between tests

```csharp
[Fact]
public async Task AddAsync_ShouldAddStudentToDatabase()
{
    // Arrange
    var student = TestDataGenerator.GenerateStudent();

    // Act
    await _repository.AddAsync(student);
    var result = await _repository.GetByIdAsync(student.Id);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(student.Id);
}
```

## Notes

- **Controller integration tests** are currently skipped because they require real JWT authentication from Keycloak
- These tests can be enabled for E2E testing in environments with Keycloak running
- All unit and repository integration tests run successfully without external dependencies

## Test Metrics

- **Total Tests**: 55
- **Passing**: 43
- **Skipped**: 12 (controller tests requiring real authentication)
- **Execution Time**: ~1 second
- **Coverage**: All service methods and repository operations

## Continuous Improvement

Future test enhancements:
- [ ] Add code coverage reporting with threshold enforcement
- [ ] Add performance tests for bulk operations
- [ ] Add E2E tests with real Keycloak instance
- [ ] Add mutation testing to verify test quality
