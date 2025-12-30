# Notes.Web.Tests.Integration

Integration tests for the NotesApp using **TestContainers.MongoDb** and **NSubstitute** for mocking external dependencies.

## Overview

This test project contains integration tests that verify the behavior of the application with a real MongoDB instance running in a Docker container via TestContainers. These tests ensure that:

- Repository operations work correctly with MongoDB
- MediatR handlers integrate properly with the database
- AI services can be mocked for testing business logic
- Data persistence and retrieval work as expected

## Technologies Used

- **xUnit** - Test framework
- **TestContainers.MongoDb** - Provides isolated MongoDB instances for testing
- **NSubstitute** - Mocking framework for external dependencies (e.g., AI services)
- **FluentAssertions** - Fluent assertion library for readable test assertions
- **MongoDB.Driver** - Native MongoDB driver for database operations

## Test Structure

### Fixtures

#### `MongoDbFixture`
Base fixture that manages the lifecycle of a MongoDB TestContainer. It:
- Starts a MongoDB container before tests run
- Provides a connection string and client for tests
- Offers helper methods to seed and clear test data
- Disposes the container after tests complete

### Test Categories

#### Repository Integration Tests
Located in `Repositories/` directory:
- `NoteRepositoryIntegrationTests.cs` - Tests for `NoteRepository` CRUD operations

Tests verify:
- Creating, reading, updating, and deleting notes
- Querying with filters and pagination
- Counting documents
- Archiving notes

#### Feature Handler Integration Tests
Located in `Features/Notes/` directory:
- `CreateNoteHandlerIntegrationTests.cs` - Tests for creating notes with AI integration (5 tests)
- `UpdateNoteHandlerIntegrationTests.cs` - Tests for updating notes with AI regeneration (6 tests)
- `DeleteNoteHandlerIntegrationTests.cs` - Tests for deleting notes with authorization checks (5 tests)
- `GetNoteDetailsHandlerIntegrationTests.cs` - Tests for retrieving note details (6 tests)
- `ListNotesHandlerIntegrationTests.cs` - Tests for listing and paginating notes (8 tests)
- `SearchNotesHandlerIntegrationTests.cs` - Tests for searching notes by title/content (9 tests)

Tests verify:
- MediatR command/query handling
- AI service integration (mocked)
- Data persistence and retrieval
- Authorization checks (user-based access control)
- Pagination functionality
- Search functionality (case-insensitive, content/title matching)
- Special character and Unicode handling
- Parallel AI operations

## Running the Tests

### Prerequisites

- Docker Desktop must be running (required for TestContainers)
- .NET 10 SDK

### Run All Tests

```bash
dotnet test
```

### Run with Coverage

```bash
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~NoteRepositoryIntegrationTests"
```

## Writing New Integration Tests

### 1. Create a Test Class

```csharp
public class MyIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly MongoDbFixture _fixture;
    
    public MyIntegrationTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 2. Setup Dependencies

```csharp
// Create repository with test context
var contextFactory = Substitute.For<IMongoDbContextFactory>();
contextFactory.CreateDbContext().Returns(_fixture.CreateContext());
var repository = new NoteRepository(contextFactory);

// Mock external services
var aiService = Substitute.For<IAiService>();
aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns("Test summary");
```

### 3. Arrange Test Data

```csharp
// Clear existing data
await _fixture.ClearNotesAsync();

// Seed test data
var testNote = new Note { /* properties */ };
await _fixture.SeedNotesAsync(testNote);
```

### 4. Write Test

```csharp
[Fact]
public async Task MyTest_Scenario_ExpectedResult()
{
    // Arrange
    await _fixture.ClearNotesAsync();
    // ... setup

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    // ... assertions
}
```

## Best Practices

1. **Clean State**: Always clear data before each test using `_fixture.ClearNotesAsync()`
2. **Isolation**: Each test should be independent and not rely on other tests
3. **Mocking**: Mock external dependencies (AI services, HTTP clients) using NSubstitute
4. **Real Database**: Use the real MongoDB container for data operations
5. **Assertions**: Use FluentAssertions for readable and expressive assertions
6. **Naming**: Follow the pattern `MethodName_Scenario_ExpectedResult`

## Common Patterns

### Testing Repository Operations

```csharp
[Fact]
public async Task AddNote_ValidNote_PersistsToDatabase()
{
    // Arrange
    await _fixture.ClearNotesAsync();
    var note = new Note { /* properties */ };

    // Act
    var result = await _repository.AddNote(note);

    // Assert
    result.IsSuccess.Should().BeTrue();
    
    var context = _fixture.CreateContext();
    var saved = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();
    saved.Should().NotBeNull();
}
```

### Testing MediatR Handlers

```csharp
[Fact]
public async Task Handle_ValidCommand_CreatesNote()
{
    // Arrange
    await _fixture.ClearNotesAsync();
    
    _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
        .Returns("Summary");
    
    var command = new CreateNoteCommand { /* properties */ };

    // Act
    var response = await _handler.Handle(command, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
    response.Id.Should().NotBe(ObjectId.Empty);
}
```

### Verifying Mock Interactions

```csharp
// Verify method was called
await _aiService.Received(1).GenerateSummaryAsync(
    Arg.Any<string>(), 
    Arg.Any<CancellationToken>());

// Verify method was not called
await _aiService.DidNotReceive().GenerateSummaryAsync(
    Arg.Any<string>(), 
    Arg.Any<CancellationToken>());
```

## Troubleshooting

### Docker Issues

If tests fail with Docker-related errors:
1. Ensure Docker Desktop is running
2. Check that you have sufficient resources allocated to Docker
3. Try restarting Docker Desktop

### Port Conflicts

TestContainers automatically assigns random ports, so port conflicts should be rare. If they occur, the tests will fail during initialization.

### Slow Tests

Integration tests are slower than unit tests because they:
- Start Docker containers
- Perform real database operations
- Run async operations

This is expected and normal for integration tests.

## Continuous Integration

When running in CI/CD pipelines:
- Ensure Docker is available in the CI environment
- TestContainers will automatically pull the MongoDB image
- Tests may take longer on first run while images are downloaded
- Consider caching Docker images to speed up subsequent runs

## Resources

- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [xUnit Documentation](https://xunit.net/)
