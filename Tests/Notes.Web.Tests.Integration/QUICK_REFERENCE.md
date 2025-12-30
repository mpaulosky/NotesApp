# Quick Reference Guide - Integration Tests

## Quick Start

### Run All Integration Tests
```bash
dotnet test Tests\Notes.Web.Tests.Integration
```

### Run with Coverage
```bash
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test Tests\Notes.Web.Tests.Integration
```

### Run Specific Test Class
```bash
# Repository tests
dotnet test --filter "FullyQualifiedName~NoteRepositoryIntegrationTests"

# CreateNote handler tests
dotnet test --filter "FullyQualifiedName~CreateNoteHandlerIntegrationTests"

# UpdateNote handler tests
dotnet test --filter "FullyQualifiedName~UpdateNoteHandlerIntegrationTests"

# MongoDbContext tests
dotnet test --filter "FullyQualifiedName~MongoDbContextIntegrationTests"
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~GetNoteByIdAsync_WhenNoteExists_ReturnsNote"
```

## Test Inventory

### Repository Tests (10 tests)
- `GetNoteByIdAsync_WhenNoteExists_ReturnsNote`
- `GetNoteByIdAsync_WhenNoteDoesNotExist_ReturnsNull`
- `GetNotes_ReturnsAllNotes`
- `GetNotes_WithPredicate_ReturnsFilteredNotes`
- `GetNotes_WithPagination_ReturnsPagedResults`
- `GetNoteCount_ReturnsCorrectCount`
- `AddNote_AddsNoteToDatabase`
- `UpdateNote_UpdatesExistingNote`
- `DeleteNote_RemovesNoteFromDatabase`
- `ArchiveNote_SetsIsArchivedToTrue`

### CreateNote Handler Tests (5 tests)
- `Handle_CreatesNoteWithAiGeneratedContent`
- `Handle_PersistsNoteToDatabase`
- `Handle_CallsAiServicesInParallel`
- `Handle_WithEmptyContent_CreatesNoteWithEmptyAiFields`
- `Handle_MultipleNotes_AllPersistedCorrectly`

### UpdateNote Handler Tests (6 tests)
- `Handle_WithValidNote_UpdatesNoteSuccessfully`
- `Handle_PersistsUpdatesToDatabase`
- `Handle_WithNonExistentNote_ReturnsFailure`
- `Handle_WithWrongOwner_ReturnsFailure`
- `Handle_RegeneratesAiContentOnEveryUpdate`
- `Handle_PreservesCreatedAtTimestamp`

### MongoDbContext Tests (9 tests)
- `CreateDbContext_ReturnsValidContext`
- `Notes_Collection_CanInsertAndRetrieve`
- `Database_Property_AllowsDirectAccess`
- `Notes_Collection_SupportsComplexQueries`
- `Notes_Collection_SupportsTextSearch`
- `Notes_Collection_SupportsUpdate`
- `Notes_Collection_SupportsDelete`
- `Notes_Collection_SupportsBulkOperations`
- `Notes_Collection_SupportsAggregation`

## Common Test Patterns

### Basic Test Structure
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    await _fixture.ClearNotesAsync();
    // ... setup test data

    // Act
    var result = await _repository.SomeMethod(...);

    // Assert
    result.Should().NotBeNull();
    // ... more assertions
}
```

### Testing with Mocked AI Service
```csharp
_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns("Test summary");

// Verify it was called
await _aiService.Received(1).GenerateSummaryAsync(
    Arg.Any<string>(), 
    Arg.Any<CancellationToken>());
```

### Seeding Test Data
```csharp
var testNote = new Note
{
    Id = ObjectId.GenerateNewId(),
    Title = "Test",
    Content = "Content",
    OwnerSubject = "user-123",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await _fixture.SeedNotesAsync(testNote);
```

### Verifying Database State
```csharp
var context = _fixture.CreateContext();
var savedNote = await context.Notes.Find(n => n.Id == noteId).FirstOrDefaultAsync();

savedNote.Should().NotBeNull();
savedNote!.Title.Should().Be("Expected Title");
```

## Important Prerequisites

### Required
- ? Docker Desktop must be running
- ? .NET 10 SDK installed

### Optional (for coverage)
- Install dotnet-coverage tool:
  ```bash
  dotnet tool install -g dotnet-coverage
  ```

## Troubleshooting

### "Docker is not running"
- Start Docker Desktop
- Wait for it to fully initialize

### "Port already in use"
- TestContainers uses random ports
- This should rarely happen
- Restart Docker if it does

### Tests are slow
- First run downloads MongoDB image (one-time)
- Container startup takes 2-5 seconds
- This is normal for integration tests
- Consider running unit tests separately for quick feedback

### Test hangs indefinitely
- Check Docker Desktop is responsive
- Check available Docker resources (memory, CPU)
- Restart Docker Desktop

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run Integration Tests
  run: |
    dotnet test Tests/Notes.Web.Tests.Integration --no-build --verbosity normal
```

### Azure Pipelines Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests'
  inputs:
    command: 'test'
    projects: 'Tests/Notes.Web.Tests.Integration/*.csproj'
```

## Coverage Reports

### Generate HTML Report (with ReportGenerator)
```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test Tests\Notes.Web.Tests.Integration

# Generate HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html

# Open in browser
start coveragereport\index.html
```

## Best Practices Checklist

- ? Always clear data before test: `await _fixture.ClearNotesAsync()`
- ? Each test should be independent
- ? Mock external services (AI, HTTP clients)
- ? Use real MongoDB for data operations
- ? Use FluentAssertions for readable tests
- ? Follow naming: `MethodName_Scenario_ExpectedResult`
- ? Test both success and failure paths
- ? Verify database state after operations
- ? Use `Arg.Any<T>()` for flexible mock matching

## File Locations

```
Tests/Notes.Web.Tests.Integration/
??? Fixtures/
?   ??? MongoDbFixture.cs              # Test infrastructure
??? Repositories/
?   ??? NoteRepositoryIntegrationTests.cs
??? Features/
?   ??? Notes/
?       ??? CreateNoteHandlerIntegrationTests.cs
?       ??? UpdateNoteHandlerIntegrationTests.cs
??? Data/
?   ??? MongoDbContextIntegrationTests.cs
??? GlobalUsings.cs                     # Global using directives
??? README.md                           # Full documentation
??? IMPLEMENTATION_SUMMARY.md           # What was implemented
??? QUICK_REFERENCE.md                  # This file
```

## Need Help?

- See `README.md` for detailed documentation
- See `IMPLEMENTATION_SUMMARY.md` for implementation details
- Check existing tests for examples
- Ensure Docker is running for all integration tests
