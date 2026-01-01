# NotesApp - Comprehensive Test Implementation Research Package

**Generated:** January 1, 2026  
**Purpose:** Complete reference for implementing missing architecture, unit, and integration tests

---

## 📊 Executive Summary

This document provides comprehensive research on the NotesApp codebase to facilitate test implementation. It includes technology stack details, coding conventions, architectural patterns, existing test examples, and reusable code patterns.

---

## 🛠 Technology Stack

### Framework Versions

- **.NET Version:** 10.0.100 (SDK)
- **C# Language Version:** 14.0 (latest stable)
- **Target Framework:** net10.0

### Core Testing Frameworks

```xml
<!-- From Directory.Packages.props -->
<PackageVersion Include="xunit" Version="2.9.3" />
<PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageVersion Include="FluentAssertions" Version="7.1.0" />
<PackageVersion Include="NSubstitute" Version="5.3.0" />
<PackageVersion Include="bunit" Version="2.4.2" />
<PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
<PackageVersion Include="Testcontainers" Version="4.9.0" />
<PackageVersion Include="Testcontainers.MongoDb" Version="4.9.0" />
<PackageVersion Include="coverlet.collector" Version="6.0.4" />
<PackageVersion Include="coverlet.msbuild" Version="6.0.4" />
```

### Application Stack

- **Blazor:** Interactive Server Rendering
- **Database:** MongoDB (native driver + EF Core provider)
- **Architecture:** Vertical Slice Architecture with CQRS (MediatR)
- **AI Integration:** OpenAI SDK 2.8.0
- **Authentication:** Auth0
- **Orchestration:** .NET Aspire 13.1.0
- **Observability:** OpenTelemetry 1.14.0

---

## 📁 Project Structure

### Test Projects

```
Tests/
├── Notes.Tests.Architecture/              # Architecture constraint tests
│   ├── BaseArchitectureTest.cs           # Base class with assembly references
│   ├── ComponentArchitectureTests.cs     # Blazor component architecture rules
│   ├── GeneralArchitectureTests.cs       # SOLID principles, etc.
│   ├── NamingConventionTests.cs          # Naming standards
│   ├── LayeringTests.cs                  # Layer dependencies
│   ├── DependencyTests.cs                # DI and dependency patterns
│   └── ServicePatternTests.cs            # Service pattern compliance
│
├── Notes.Web.Tests.Integration/          # Integration tests with TestContainers
│   ├── Fixtures/
│   │   └── MongoDbFixture.cs             # MongoDB TestContainer fixture
│   ├── Features/
│   │   └── Notes/                        # Feature-based organization
│   │       ├── CreateNoteHandlerIntegrationTests.cs
│   │       ├── UpdateNoteHandlerIntegrationTests.cs
│   │       ├── DeleteNoteHandlerIntegrationTests.cs
│   │       ├── GetNoteDetailsHandlerIntegrationTests.cs
│   │       ├── ListNotesHandlerIntegrationTests.cs
│   │       └── SearchNotesHandlerIntegrationTests.cs
│   ├── Data/
│   │   └── MongoDbContextIntegrationTests.cs
│   └── Repositories/
│       └── NoteRepositoryIntegrationTests.cs
│
└── Notes.Web.Tests.Unit/                 # Unit tests
    ├── Infrastructure/
    │   └── TestDataBuilder.cs            # Test data factory methods
    ├── Features/                         # Feature-based tests
    │   ├── CreateNote/
    │   ├── UpdateNote/
    │   ├── DeleteNote/
    │   ├── GetNoteDetails/
    │   ├── ListNotes/
    │   ├── SearchNotes/
    │   └── GetRelatedNotes/
    ├── Components/                       # Blazor component tests (bUnit)
    │   ├── Pages/
    │   ├── Layout/
    │   ├── Shared/
    │   └── User/
    ├── Services/
    │   ├── Ai/
    │   └── Notes/
    └── Data/
```

### Source Code Structure

```
src/
├── Notes.AppHost/                        # .NET Aspire orchestration
│   ├── AppHost.cs
│   ├── DatabaseService.cs
│   └── RedisService.cs
│
├── Notes.ServiceDefaults/                # Shared service configurations
│   └── Extensions.cs                    # OpenTelemetry, health checks, service discovery
│
├── Notes.Web/                           # Main Blazor application
│   ├── Components/
│   │   ├── Account/                     # Auth0 authentication components
│   │   ├── Layout/                      # Layout components
│   │   ├── Pages/                       # Page components (routable)
│   │   └── Shared/                      # Reusable components
│   ├── Data/
│   │   ├── MongoDbContext.cs           # Native MongoDB driver context
│   │   └── Repositories/
│   │       └── NoteRepository.cs        # Repository implementation
│   ├── Features/                        # Vertical Slice Architecture
│   │   └── Notes/
│   │       ├── CreateNote/
│   │       │   └── CreateNote.cs        # Command + Handler + Response
│   │       ├── UpdateNote/
│   │       ├── DeleteNote/
│   │       ├── GetNoteDetails/
│   │       ├── ListNotes/
│   │       ├── SearchNotes/
│   │       └── GetRelatedNotes/
│   ├── Services/
│   │   ├── Ai/                          # AI service abstractions
│   │   │   ├── IAiService.cs
│   │   │   ├── OpenAiService.cs
│   │   │   ├── IChatClientWrapper.cs
│   │   │   └── IEmbeddingClientWrapper.cs
│   │   └── Notes/
│   │       ├── INoteService.cs          # Abstraction over MediatR
│   │       └── NoteService.cs
│   └── Extensions/
│       ├── AiServiceExtensions.cs       # AI DI registration
│       └── MongoDbExtensions.cs         # MongoDB DI registration
│
└── Shared/                              # Shared entities and abstractions
    ├── Entities/
    │   ├── Note.cs                      # MongoDB entity
    │   └── AppUser.cs
    ├── Abstractions/
    │   └── Result.cs                    # Result pattern
    └── Interfaces/
        ├── INoteRepository.cs
        └── IMongoDbContext.cs
```

---

## 🎨 Coding Conventions

### Naming Patterns

#### From .github/copilot-instructions.md

- **Interfaces:** Prefix with `I` (e.g., `IService`)
- **Async Methods:** Suffix with `Async` (e.g., `GetDataAsync`)
- **Private Fields:** Prefix with `_` (e.g., `_myField`)
- **Constants:** `UPPER_CASE` (e.g., `MAX_SIZE`)
- **Blazor Components:** Suffix with `Component` (e.g., `FooterComponent`)
- **Blazor Pages:** Suffix with `Page` (e.g., `AboutPage`)

#### Test-Specific Conventions

- **Test Classes:** `{TypeUnderTest}Tests` (e.g., `CreateNoteHandlerTests`)
- **Integration Tests:** `{TypeUnderTest}IntegrationTests`
- **Test Methods:** `{MethodName}_Should{ExpectedBehavior}` or `{MethodName}_With{Condition}_Should{ExpectedBehavior}`
- **Test Fixtures:** `{Resource}Fixture` (e.g., `MongoDbFixture`)

### File Organization

#### Test File Pattern

```csharp
// Copyright header
using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Features.{FeatureName};

/// <summary>
/// XML documentation describing the test class purpose.
/// </summary>
[ExcludeFromCodeCoverage]
public class {ClassName}Tests
{
    // Fields
    private readonly {Interface} _dependency;
    private readonly {ClassUnderTest} _sut; // System Under Test
    
    // Constructor with test doubles setup
    public {ClassName}Tests()
    {
        _dependency = Substitute.For<{Interface}>();
        _sut = new {ClassUnderTest}(_dependency);
    }
    
    // Test methods
    [Fact]
    public async Task MethodName_WithCondition_ShouldExpectedBehavior()
    {
        // Arrange
        // ... setup code
        
        // Act
        // ... execute method
        
        // Assert
        // ... verify behavior
    }
}
```

### Code Style (from .editorconfig)

- **File-scoped namespaces:** `true`
- **Nullable reference types:** `true`
- **Explicit types vs var:** Use `var` when type is obvious
- **Null checks:** Use `is null` and `is not null` patterns
- **Primary constructors:** `false` (use traditional constructors)
- **Records:** `true` (for DTOs, Commands, Queries, Responses)
- **Async/await:** Always use with `CancellationToken` parameter

---

## 🏗 Architecture Patterns

### CQRS Implementation (MediatR)

Each feature slice contains:

1. **Command/Query** - IRequest<TResponse> record
2. **Response DTO** - record
3. **Handler** - IRequestHandler<TRequest, TResponse> class

Example from CreateNote.cs:

```csharp
/// <summary>
/// Command to create a new note.
/// </summary>
public record CreateNoteCommand : IRequest<CreateNoteResponse>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string UserSubject { get; init; } = string.Empty;
}

/// <summary>
/// Handler for creating a new note with AI-generated summary, tags, and embeddings.
/// </summary>
public class CreateNoteHandler(INoteRepository repository, IAiService aiService) 
    : IRequestHandler<CreateNoteCommand, CreateNoteResponse>
{
    private readonly IAiService _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
    private readonly INoteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<CreateNoteResponse> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        // Implementation with parallel AI calls
        var summaryTask = _aiService.GenerateSummaryAsync(request.Content, cancellationToken);
        var tagsTask = _aiService.GenerateTagsAsync(request.Title, request.Content, cancellationToken);
        var embeddingTask = _aiService.GenerateEmbeddingAsync(request.Content, cancellationToken);
        
        await Task.WhenAll(summaryTask, tagsTask, embeddingTask);
        
        // ... create and save note
    }
}

/// <summary>
/// Response after creating a note.
/// </summary>
public record CreateNoteResponse(
    ObjectId Id,
    string Title,
    string Content,
    string? AiSummary,
    string? Tags,
    float[] Embedding,
    string OwnerSubject,
    DateTime UpdatedAt,
    DateTime CreatedAt,
    bool IsArchived
);
```

### Service Layer Abstraction

**Pattern:** NoteService abstracts MediatR from Blazor components

```csharp
public interface INoteService
{
    Task<CreateNoteResponse?> CreateNoteAsync(string title, string content, string userSubject, CancellationToken cancellationToken = default);
    Task<GetNoteDetailsResponse> GetNoteDetailsAsync(ObjectId id, string userSubject, CancellationToken cancellationToken = default);
    // ... other methods
}

public sealed class NoteService(IMediator mediator) : INoteService
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task<CreateNoteResponse?> CreateNoteAsync(string title, string content, string userSubject, CancellationToken cancellationToken = default)
    {
        var command = new CreateNoteCommand { Title = title, Content = content, UserSubject = userSubject };
        return await _mediator.Send(command, cancellationToken);
    }
}
```

### Result Pattern

From Shared.Abstractions.Result:

```csharp
// Used throughout repositories and services for error handling
Task<Result<Note?>> GetNoteByIdAsync(ObjectId id);
Task<Result<IEnumerable<Note>?>> GetNotes();
Task<Result> AddNote(Note note);
```

### Repository Pattern

```csharp
public interface INoteRepository
{
    Task<Result<Note?>> GetNoteByIdAsync(ObjectId id);
    Task<Result<IEnumerable<Note>?>> GetNotes();
    Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where);
    Task<Result> AddNote(Note note);
    Task<Result> UpdateNote(Note note);
    Task<Result> DeleteNote(Note note);
}

public class NoteRepository(IMongoDbContextFactory contextFactory) : INoteRepository
{
    public async Task<Result<Note?>> GetNoteByIdAsync(ObjectId id)
    {
        try
        {
            IMongoDbContext context = contextFactory.CreateDbContext();
            Note? note = await context.Notes.Find(a => a.Id == id).FirstOrDefaultAsync();
            return Result.Ok<Note?>(note);
        }
        catch (Exception ex)
        {
            return Result.Fail<Note?>($"Error getting note by id: {ex.Message}");
        }
    }
}
```

### Dependency Injection Extensions

Pattern from AiServiceExtensions.cs:

```csharp
public static class AiServiceExtensions
{
    public static IHostApplicationBuilder AddAiServices(this IHostApplicationBuilder builder)
    {
        // Configure options
        builder.Services.Configure<AiServiceOptions>(
            builder.Configuration.GetSection(AiServiceOptions.SectionName));

        // Register singletons for stateless clients
        builder.Services.AddSingleton<IChatClientWrapper>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
            var chatClient = new ChatClient(options.ChatModel, options.ApiKey);
            return new ChatClientWrapper(chatClient);
        });

        // Register scoped services
        builder.Services.AddScoped<IAiService, OpenAiService>();

        return builder;
    }
}
```

---

## 🧪 Test Patterns Library

### 1. Unit Test Pattern (Handler Tests)

**File:** CreateNoteHandlerTests.cs

```csharp
[ExcludeFromCodeCoverage]
public class CreateNoteHandlerTests
{
    private readonly INoteRepository _repository;
    private readonly IAiService _aiService;
    private readonly CreateNoteHandler _handler;

    public CreateNoteHandlerTests()
    {
        _repository = Substitute.For<INoteRepository>();
        _aiService = Substitute.For<IAiService>();
        _handler = new CreateNoteHandler(_repository, _aiService);
    }

    [Fact]
    public async Task Handle_ShouldCreateNoteWithAiGeneratedData()
    {
        // Arrange
        var command = TestDataBuilder.CreateNoteCommand(
            title: "Test Title",
            content: "Test Content",
            userSubject: "auth0|test123"
        );

        var expectedSummary = "AI Generated Summary";
        var expectedTags = "ai,generated,tags";
        var expectedEmbedding = new[] { 0.1f, 0.2f, 0.3f };

        _aiService.GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSummary));
        _aiService.GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedTags));
        _aiService.GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEmbedding));

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be(command.Title);
        response.Content.Should().Be(command.Content);
        response.AiSummary.Should().Be(expectedSummary);
        response.Tags.Should().Be(expectedTags);
        response.Embedding.Should().BeEquivalentTo(expectedEmbedding);
        response.OwnerSubject.Should().Be(command.UserSubject);
        response.IsArchived.Should().BeFalse();
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Handle_ShouldCallAiServiceInParallel()
    {
        // Arrange
        var command = TestDataBuilder.CreateNoteCommand();

        _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("Summary"));
        _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("tags"));
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { 0.1f }));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
        await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
        await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveNoteToRepository()
    {
        // Arrange
        var command = TestDataBuilder.CreateNoteCommand();

        _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("Summary"));
        _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("tags"));
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { 0.1f }));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddNote(Arg.Is<Note>(n =>
            n.Title == command.Title &&
            n.Content == command.Content &&
            n.OwnerSubject == command.UserSubject &&
            !n.IsArchived
        ));
    }
}
```

### 2. Integration Test Pattern (with TestContainers)

**File:** CreateNoteHandlerIntegrationTests.cs

```csharp
[ExcludeFromCodeCoverage]
public class CreateNoteHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly MongoDbFixture _fixture;
    private readonly INoteRepository _repository;
    private readonly IAiService _aiService;
    private readonly IRequestHandler<CreateNoteCommand, CreateNoteResponse> _handler;

    public CreateNoteHandlerIntegrationTests(MongoDbFixture fixture)
    {
        _fixture = fixture;

        // Create a factory that returns the test context
        var contextFactory = Substitute.For<IMongoDbContextFactory>();
        contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

        _repository = new NoteRepository(contextFactory);
        
        // Mock the AI service
        _aiService = Substitute.For<IAiService>();

        _handler = new CreateNoteHandler(_repository, _aiService);
    }

    [Fact]
    public async Task Handle_CreatesNoteWithAiGeneratedContent()
    {
        // Arrange
        await _fixture.ClearNotesAsync();

        var expectedSummary = "This is an AI-generated summary";
        var expectedTags = "tag1, tag2, tag3";
        var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f };

        _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummary);
        _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedTags);
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedEmbedding);

        var command = new CreateNoteCommand
        {
            Title = "Test Note",
            Content = "This is test content for the note",
            UserSubject = "test-user-123"
        };

        // Act
        CreateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBe(ObjectId.Empty);
        response.Title.Should().Be(command.Title);
        response.AiSummary.Should().Be(expectedSummary);
        
        // Verify AI service was called correctly
        await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PersistsNoteToDatabase()
    {
        // Arrange
        await _fixture.ClearNotesAsync();

        _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Summary");
        _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("tags");
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.5f });

        var command = new CreateNoteCommand
        {
            Title = "Persisted Note",
            Content = "This note should be in the database",
            UserSubject = "test-user-456"
        };

        // Act
        CreateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

        // Assert - verify in database
        var context = _fixture.CreateContext();
        var persistedNote = await context.Notes.Find(n => n.Id == response.Id).FirstOrDefaultAsync();

        persistedNote.Should().NotBeNull();
        persistedNote!.Title.Should().Be(command.Title);
        persistedNote.Content.Should().Be(command.Content);
        persistedNote.OwnerSubject.Should().Be(command.UserSubject);
    }
}
```

### 3. MongoDb Fixture Pattern

**File:** MongoDbFixture.cs

```csharp
[ExcludeFromCodeCoverage]
public class MongoDbFixture : IAsyncLifetime
{
    private MongoDbContainer? _mongoDbContainer;

    public string ConnectionString => _mongoDbContainer?.GetConnectionString()
        ?? throw new InvalidOperationException("MongoDB container not initialized");

    public IMongoClient MongoClient { get; private set; } = null!;
    public string DatabaseName => "NotesTestDb";
    public IMongoDatabase Database => MongoClient.GetDatabase(DatabaseName);

    public async Task InitializeAsync()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .WithCleanUp(true)
            .WithPortBinding(0, 27017)
            .Build();

        await _mongoDbContainer.StartAsync();
        MongoClient = new MongoClient(ConnectionString);

        // Verify connection
        await MongoClient.GetDatabase(DatabaseName).RunCommandAsync(
            (Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
    }

    public async Task DisposeAsync()
    {
        if (_mongoDbContainer is not null)
        {
            await _mongoDbContainer.DisposeAsync();
        }
    }

    public IMongoDbContext CreateContext()
    {
        return new MongoDbContext(MongoClient, DatabaseName);
    }

    public async Task ClearNotesAsync()
    {
        var context = CreateContext();
        await context.Notes.DeleteManyAsync(Builders<Note>.Filter.Empty);
    }

    public async Task SeedNotesAsync(params Note[] notes)
    {
        if (notes.Length == 0) return;
        
        var context = CreateContext();
        await context.Notes.InsertManyAsync(notes);
    }
}
```

### 4. Blazor Component Test Pattern (bUnit)

**File:** ErrorAlertComponentTests.cs

```csharp
[ExcludeFromCodeCoverage]
public class ErrorAlertComponentTests : BunitContext
{
    [Fact]
    public void ErrorAlertComponent_WithDefaultProps_ShouldRenderDefaultValues()
    {
        // Arrange & Act
        var cut = Render<ErrorAlertComponent>();

        // Assert
        var title = cut.Find("h3");
        title.TextContent.Should().Be("Error");

        var message = cut.Find("div.text-red-700");
        message.TextContent.Trim().Should().Be("An error occurred.");
    }

    [Fact]
    public void ErrorAlertComponent_WithCustomMessage_ShouldDisplayCustomMessage()
    {
        // Arrange & Act
        var cut = Render<ErrorAlertComponent>(parameters => parameters
            .Add(p => p.Message, "Custom error message")
        );

        // Assert
        var message = cut.Find("div.text-red-700");
        message.TextContent.Trim().Should().Be("Custom error message");
    }

    [Fact]
    public void ErrorAlertComponent_ShouldHaveErrorStyling()
    {
        // Arrange & Act
        var cut = Render<ErrorAlertComponent>();

        // Assert
        var container = cut.Find("div.bg-red-50");
        container.Should().NotBeNull();
        container.ClassList.Should().Contain("border-red-200");
        container.ClassList.Should().Contain("rounded-md");
    }
}
```

### 5. Architecture Test Pattern (NetArchTest)

**File:** ComponentArchitectureTests.cs

```csharp
[ExcludeFromCodeCoverage]
public class ComponentArchitectureTests : BaseArchitectureTest
{
    [Fact]
    public void PageComponentsShouldBeInPagesOrUserFolder()
    {
        var pageComponents = Types.InAssembly(WebAssembly)
            .That()
            .HaveCustomAttribute(typeof(Microsoft.AspNetCore.Components.RouteAttribute))
            .GetTypes();

        var invalidPages = pageComponents
            .Where(t => !t.Namespace!.Contains("Pages") && !t.Namespace.Contains("User"))
            .Select(t => t.FullName)
            .ToList();

        invalidPages.Should().BeEmpty(
            $"components with @page directive should be in Pages or User folder. Invalid: {string.Join(", ", invalidPages)}");
    }

    [Fact]
    public void ComponentsShouldNotDependOnConcreteRepositories()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .Inherit(typeof(ComponentBase))
            .Should()
            .NotHaveDependencyOn("Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"components should not directly depend on repositories. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
```

**Base Architecture Test:**

```csharp
[ExcludeFromCodeCoverage]
public abstract class BaseArchitectureTest
{
    protected static Assembly WebAssembly => typeof(global::Web.IAppMarker).Assembly;
    protected static Assembly SharedAssembly => typeof(Shared.Entities.Note).Assembly;
    protected static Assembly ServiceDefaultsAssembly => typeof(Microsoft.Extensions.Hosting.Extensions).Assembly;

    protected static Assembly[] ApplicationAssemblies =>
    [
        WebAssembly,
        SharedAssembly,
        ServiceDefaultsAssembly
    ];
}
```

### 6. Service Test Pattern with Edge Cases

**File:** OpenAiServiceTests.cs (selected patterns)

```csharp
[ExcludeFromCodeCoverage]
public class OpenAiServiceTests
{
    private readonly IOptions<AiServiceOptions> _options;
    private readonly INoteRepository _repository;
    private readonly IChatClientWrapper _chatClient;
    private readonly IEmbeddingClientWrapper _embeddingClient;
    private readonly OpenAiService _service;

    public OpenAiServiceTests()
    {
        _options = Options.Create(new AiServiceOptions
        {
            MaxSummaryTokens = 100,
            SimilarityThreshold = 0.7f,
            RelatedNotesCount = 5,
            EmbeddingModel = "text-embedding-3-small",
            ChatModel = "gpt-4"
        });
        _repository = Substitute.For<INoteRepository>();
        _chatClient = Substitute.For<IChatClientWrapper>();
        _embeddingClient = Substitute.For<IEmbeddingClientWrapper>();
        _service = new OpenAiService(_options, _repository, _chatClient, _embeddingClient);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullChatClient_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new OpenAiService(_options, _repository, null!, _embeddingClient);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("chatClient");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GenerateSummaryAsync_WithNullContent_ShouldReturnEmptyString()
    {
        // Act
        var result = await _service.GenerateSummaryAsync(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateSummaryAsync_WithWhitespace_ShouldReturnEmptyString()
    {
        // Act
        var result = await _service.GenerateSummaryAsync("   ");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task FindRelatedNotesAsync_ShouldFilterBySimilarityThreshold()
    {
        // Arrange
        var embedding = new[] { 1.0f, 0.0f, 0.0f };

        var notes = new List<Note>
        {
            new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", 
                   Embedding = new[] { 0.9f, 0.1f, 0.0f } }, // High similarity
            new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", 
                   Embedding = new[] { 0.5f, 0.5f, 0.0f } }, // Medium similarity
            new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", 
                   Embedding = new[] { 0.0f, 1.0f, 0.0f } }  // Low similarity
        };

        _repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
            .Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

        // Act
        var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

        // Assert
        result.Should().HaveCount(2); // Only notes above 0.7 threshold
    }

    #endregion
}
```

---

## 📦 Test Data Builder Pattern

**File:** TestDataBuilder.cs

```csharp
[ExcludeFromCodeCoverage]
public static class TestDataBuilder
{
    public static Note CreateNote(
        ObjectId? id = null,
        string title = "Test Note",
        string content = "Test Content",
        string? aiSummary = "Test Summary",
        string? tags = "test,note",
        float[]? embedding = null,
        string ownerSubject = "auth0|test123",
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        bool isArchived = false)
    {
        return new Note
        {
            Id = id ?? ObjectId.GenerateNewId(),
            Title = title,
            Content = content,
            AiSummary = aiSummary,
            Tags = tags,
            Embedding = embedding ?? [0.1f, 0.2f, 0.3f],
            OwnerSubject = ownerSubject,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            IsArchived = isArchived
        };
    }

    public static CreateNoteCommand CreateNoteCommand(
        string title = "Test Note",
        string content = "Test Content",
        string userSubject = "auth0|test123")
    {
        return new CreateNoteCommand
        {
            Title = title,
            Content = content,
            UserSubject = userSubject
        };
    }

    public static UpdateNoteCommand CreateUpdateNoteCommand(
        ObjectId? noteId = null,
        string title = "Updated Note",
        string content = "Updated Content",
        string userSubject = "auth0|test123")
    {
        return new UpdateNoteCommand
        {
            Id = noteId ?? ObjectId.GenerateNewId(),
            Title = title,
            Content = content,
            UserSubject = userSubject
        };
    }
}
```

---

## 🔧 Global Usings

### Unit Tests (Notes.Web.Tests.Unit/GlobalUsings.cs)

```csharp
global using Bunit;
global using Bunit.TestDoubles;
global using FluentAssertions;
global using NSubstitute;
global using Xunit;
global using MongoDB.Bson;
global using MediatR;
global using Shared.Entities;
global using Shared.Abstractions;
global using Shared.Interfaces;
global using Notes.Web.Services.Ai;
global using Notes.Web.Features.Notes.CreateNote;
global using Notes.Web.Features.Notes.UpdateNote;
global using Notes.Web.Features.Notes.DeleteNote;
global using Notes.Web.Features.Notes.ListNotes;
global using Notes.Web.Features.Notes.GetNoteDetails;
global using Notes.Web.Features.Notes.SearchNotes;
global using Notes.Web.Features.Notes.GetRelatedNotes;
global using Notes.Web.Components.Shared;
global using Notes.Web.Components.Layout;
global using Notes.Web.Components.Pages;
```

### Integration Tests (Notes.Web.Tests.Integration/GlobalUsings.cs)

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using FluentAssertions;
global using MongoDB.Bson;
global using MongoDB.Driver;
global using Notes.Web.Data;
global using Notes.Web.Data.Repositories;
global using NSubstitute;
global using Shared.Abstractions;
global using Shared.Entities;
global using Shared.Interfaces;
global using Testcontainers.MongoDb;
global using Xunit;
```

---

## 🧪 Verification Strategies

### FluentAssertions Patterns

#### Equality & Collection Assertions

```csharp
response.Should().NotBeNull();
response.Id.Should().NotBe(ObjectId.Empty);
response.Title.Should().Be(expected);
response.Tags.Should().BeEquivalentTo(expectedTags);
response.Embedding.Should().BeEmpty();
result.Should().HaveCount(3);
result.Should().OnlyHaveUniqueItems();
```

#### DateTime Assertions

```csharp
response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
response.UpdatedAt.Should().BeAfter(originalTimestamp);
```

#### Exception Assertions

```csharp
Action act = () => new Service(null!);
act.Should().Throw<ArgumentNullException>()
    .WithParameterName("paramName");
```

#### String Assertions

```csharp
cut.Markup.Should().Contain("Welcome");
message.TextContent.Trim().Should().Be("Expected text");
```

### NSubstitute Verification Patterns

#### Method Call Verification

```csharp
await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
await _aiService.DidNotReceive().GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
```

#### Argument Matching

```csharp
await _repository.Received(1).AddNote(Arg.Is<Note>(n =>
    n.Title == command.Title &&
    n.Content == command.Content &&
    !n.IsArchived
));
```

#### Return Value Setup

```csharp
_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult("Summary"));

_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
    .Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));
```

---

## 📝 Test Organization Guidelines

### Test Categories

1. **Constructor Tests** - Validate dependency injection and null checks
2. **Edge Case Tests** - Null, empty, whitespace inputs
3. **Business Logic Tests** - Core functionality
4. **Integration Tests** - Database persistence, real dependencies
5. **Architecture Tests** - Structural constraints

### Test Naming Conventions

```csharp
// Pattern 1: Method_Condition_ExpectedBehavior
GenerateSummaryAsync_WithNullContent_ShouldReturnEmptyString()

// Pattern 2: Method_ShouldExpectedBehavior
Handle_ShouldCreateNoteWithAiGeneratedData()

// Pattern 3: ComponentName_Condition_ShouldBehavior
ErrorAlertComponent_WithCustomMessage_ShouldDisplayCustomMessage()
```

### Test Method Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_Condition_ExpectedBehavior()
{
    // Arrange
    // - Setup test data
    // - Configure mocks
    // - Prepare system under test
    
    // Act
    // - Execute the method being tested
    
    // Assert
    // - Verify expected behavior
    // - Check all side effects
}
```

---

## 🏃 Build, Test, and Run Commands

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build specific project
dotnet build src/Notes.Web/Notes.Web.csproj
```

### Test Commands

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Notes.Web.Tests.Unit
dotnet test Tests/Notes.Web.Tests.Integration
dotnet test Tests/Notes.Tests.Architecture

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with filter
dotnet test --filter "FullyQualifiedName~CreateNoteHandler"
```

### Run Commands

```bash
# Run with .NET Aspire (recommended)
dotnet run --project src/Notes.AppHost/Notes.AppHost.csproj

# Run web app directly
cd src/Notes.Web
dotnet run
```

---

## 🎯 Key Testing Insights

### 1. AI Service Testing Strategy

- **Mock AI services** in unit and integration tests
- Test **parallel execution** of AI calls (Task.WhenAll)
- Verify **error handling** when AI services fail
- Test **edge cases** (null, empty content)

### 2. MongoDB Testing Strategy

- Use **TestContainers** for integration tests
- **Clear database** before each test
- Use **MongoDbFixture** for lifecycle management
- Test **actual persistence** to verify data integrity

### 3. CQRS Handler Testing

- Test **command validation**
- Verify **repository interactions**
- Check **response DTO mapping**
- Test **cancellation token propagation**

### 4. Blazor Component Testing

- Use **bUnit** for component rendering
- Test **parameter binding**
- Verify **CSS classes and styling**
- Check **event handling**
- Test **component hierarchy**

### 5. Architecture Testing

- Enforce **naming conventions**
- Validate **layer dependencies**
- Check **SOLID principles**
- Verify **component organization**

---

## 📄 Project File Patterns

### Unit Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
        
        <!-- Code Coverage -->
        <CollectCoverage>true</CollectCoverage>
        <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="bunit" />
        <PackageReference Include="FluentAssertions" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" />
        <PackageReference Include="NSubstitute" />
        <PackageReference Include="xunit" />
        <PackageReference Include="xunit.runner.visualstudio" />
        <PackageReference Include="coverlet.collector" />
        <PackageReference Include="coverlet.msbuild" />
    </ItemGroup>

    <ItemGroup>
        <Using Include="Xunit" />
        <Using Include="FluentAssertions" />
        <Using Include="NSubstitute" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\Notes.Web\Notes.Web.csproj" />
        <ProjectReference Include="..\..\src\Shared\Shared.csproj" />
    </ItemGroup>
</Project>
```

### Architecture Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
        
        <!-- Disable coverage for architecture tests -->
        <CollectCoverage>false</CollectCoverage>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="FluentAssertions" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" />
        <PackageReference Include="NetArchTest.Rules" />
        <PackageReference Include="xunit" />
        <PackageReference Include="xunit.runner.visualstudio" />
    </ItemGroup>

    <ItemGroup>
        <Using Include="Xunit" />
        <Using Include="FluentAssertions" />
        <Using Include="NetArchTest.Rules" />
    </ItemGroup>
</Project>
```

---

## 🎓 Summary & Next Steps

This research package provides:

✅ **Complete technology stack** with exact versions  
✅ **Project structure** for all test types  
✅ **Coding conventions** and naming patterns  
✅ **Architecture patterns** (CQRS, Repository, DI)  
✅ **5+ test pattern examples** with full code  
✅ **Verification strategies** (FluentAssertions, NSubstitute)  
✅ **Build/test commands** specific to this project  
✅ **MongoDbFixture** for integration testing  
✅ **TestDataBuilder** for test data creation  
✅ **Global usings** configuration  

### Recommended Implementation Order

1. **Start with Unit Tests** (fastest feedback loop)
   - Handler tests (CQRS pattern)
   - Service tests (AI, Note service)
   - Component tests (bUnit)

2. **Add Integration Tests** (database interaction)
   - Repository tests
   - Handler integration tests with real MongoDB
   - Data layer tests

3. **Implement Architecture Tests** (constraints)
   - Naming conventions
   - Layer dependencies
   - Component organization

### Key Files for Reference

- `Tests/Notes.Web.Tests.Unit/Infrastructure/TestDataBuilder.cs` - Test data patterns
- `Tests/Notes.Web.Tests.Integration/Fixtures/MongoDbFixture.cs` - Integration test setup
- `Tests/Notes.Tests.Architecture/BaseArchitectureTest.cs` - Architecture test base
- `.github/copilot-instructions.md` - Coding standards
- `Directory.Packages.props` - All package versions

---

**Document Version:** 1.0  
**Last Updated:** January 1, 2026  
**Status:** ✅ Comprehensive research complete
