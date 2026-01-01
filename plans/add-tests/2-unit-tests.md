# Unit Tests Implementation

## Goal

Implement comprehensive unit tests for NoteService, AI wrapper services, and extension methods to validate business logic, error handling, and service integration patterns.

## Prerequisites

**Branch:** Ensure you are on the `add-tests` branch before beginning implementation.

- If not on the branch, switch to it: `git checkout add-tests`
- Complete Architecture Tests (1-architecture-tests.md) before starting these unit tests

**Technology Stack:**

- .NET 10.0, C# 14.0
- xUnit 2.9.3
- FluentAssertions 7.1.0
- NSubstitute 5.3.0

---

## Step-by-Step Instructions

### Step 1: Create NoteService Unit Tests

Add comprehensive unit tests for NoteService to validate it correctly wraps MediatR operations.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Services/Notes/NoteServiceTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Services/Notes/NoteServiceTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Notes.Web.Services.Notes;

namespace Notes.Web.Tests.Unit.Services.Notes;

/// <summary>
/// Unit tests for NoteService validating MediatR integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteServiceTests
{
 private readonly IMediator _mediator;
 private readonly INoteService _sut;

 public NoteServiceTests()
 {
  _mediator = Substitute.For<IMediator>();
  _sut = new NoteService(_mediator);
 }

 [Fact]
 public void Constructor_WithNullMediator_ThrowsArgumentNullException()
 {
  // Arrange & Act
  var act = () => new NoteService(null!);

  // Assert
  act.Should().Throw<ArgumentNullException>()
   .WithParameterName("mediator");
 }

 [Fact]
 public async Task CreateNoteAsync_WithValidParameters_SendsCreateNoteCommand()
 {
  // Arrange
  const string title = "Test Note";
  const string content = "Test content";
  const string userSubject = "user123";
  var expectedResponse = new CreateNoteResponse { Success = true, NoteId = ObjectId.GenerateNewId() };
  
  _mediator.Send(Arg.Any<CreateNoteCommand>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  var result = await _sut.CreateNoteAsync(title, content, userSubject);

  // Assert
  result.Should().Be(expectedResponse);
  await _mediator.Received(1).Send(
   Arg.Is<CreateNoteCommand>(cmd => 
    cmd.Title == title && 
    cmd.Content == content && 
    cmd.UserSubject == userSubject),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task CreateNoteAsync_PassesCancellationToken()
 {
  // Arrange
  var cts = new CancellationTokenSource();
  var expectedResponse = new CreateNoteResponse { Success = true };
  
  _mediator.Send(Arg.Any<CreateNoteCommand>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  await _sut.CreateNoteAsync("title", "content", "user123", cts.Token);

  // Assert
  await _mediator.Received(1).Send(
   Arg.Any<CreateNoteCommand>(),
   cts.Token);
 }

 [Fact]
 public async Task GetNoteDetailsAsync_WithValidId_SendsGetNoteDetailsQuery()
 {
  // Arrange
  var noteId = ObjectId.GenerateNewId();
  const string userSubject = "user123";
  var expectedResponse = new GetNoteDetailsResponse { Success = true };
  
  _mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  var result = await _sut.GetNoteDetailsAsync(noteId, userSubject);

  // Assert
  result.Should().Be(expectedResponse);
  await _mediator.Received(1).Send(
   Arg.Is<GetNoteDetailsQuery>(q => 
    q.Id == noteId && 
    q.UserSubject == userSubject),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task GetNoteDetailsAsync_WhenMediatorReturnsNull_ReturnsFailureResponse()
 {
  // Arrange
  _mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
   .Returns((GetNoteDetailsResponse?)null);

  // Act
  var result = await _sut.GetNoteDetailsAsync(ObjectId.GenerateNewId(), "user123");

  // Assert
  result.Should().NotBeNull();
  result.Success.Should().BeFalse();
  result.Message.Should().Be("Failed to retrieve note details.");
 }

 [Fact]
 public async Task UpdateNoteAsync_WithValidParameters_SendsUpdateNoteCommand()
 {
  // Arrange
  var noteId = ObjectId.GenerateNewId();
  const string title = "Updated Title";
  const string content = "Updated content";
  const bool isArchived = true;
  const string userSubject = "user123";
  var expectedResponse = new UpdateNoteResponse { Success = true };
  
  _mediator.Send(Arg.Any<UpdateNoteCommand>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  var result = await _sut.UpdateNoteAsync(noteId, title, content, isArchived, userSubject);

  // Assert
  result.Should().Be(expectedResponse);
  await _mediator.Received(1).Send(
   Arg.Is<UpdateNoteCommand>(cmd => 
    cmd.Id == noteId && 
    cmd.Title == title && 
    cmd.Content == content && 
    cmd.IsArchived == isArchived && 
    cmd.UserSubject == userSubject),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task ListNotesAsync_WithUserSubject_SendsListNotesQuery()
 {
  // Arrange
  const string userSubject = "user123";
  var expectedResponse = new ListNotesResponse { Success = true, Notes = [] };
  
  _mediator.Send(Arg.Any<ListNotesQuery>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  var result = await _sut.ListNotesAsync(userSubject);

  // Assert
  result.Should().Be(expectedResponse);
  await _mediator.Received(1).Send(
   Arg.Is<ListNotesQuery>(q => q.UserSubject == userSubject),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task DeleteNoteAsync_WithValidId_SendsDeleteNoteCommand()
 {
  // Arrange
  var noteId = ObjectId.GenerateNewId();
  const string userSubject = "user123";
  var expectedResponse = new DeleteNoteResponse { Success = true };
  
  _mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>())
   .Returns(expectedResponse);

  // Act
  var result = await _sut.DeleteNoteAsync(noteId, userSubject);

  // Assert
  result.Should().Be(expectedResponse);
  await _mediator.Received(1).Send(
   Arg.Is<DeleteNoteCommand>(cmd => 
    cmd.Id == noteId && 
    cmd.UserSubject == userSubject),
   Arg.Any<CancellationToken>());
 }
}
```

#### Step 1 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~NoteServiceTests"`
- [ ] All NoteServiceTests pass successfully
- [ ] No build errors or warnings

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 2: Create AI Wrapper Unit Tests

Add unit tests for ChatClientWrapper and EmbeddingClientWrapper to validate OpenAI client integration.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Services/Ai/ChatClientWrapperTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Services/Ai/ChatClientWrapperTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ChatClientWrapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Notes.Web.Services.Ai;
using OpenAI.Chat;

namespace Notes.Web.Tests.Unit.Services.Ai;

/// <summary>
/// Unit tests for ChatClientWrapper validating OpenAI chat client integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChatClientWrapperTests
{
 [Fact]
 public void Constructor_WithNullChatClient_ThrowsArgumentNullException()
 {
  // Arrange & Act
  var act = () => new ChatClientWrapper(null!);

  // Assert
  act.Should().Throw<ArgumentNullException>()
   .WithParameterName("chatClient");
 }

 [Fact]
 public void Constructor_WithValidChatClient_CreatesInstance()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();

  // Act
  var wrapper = new ChatClientWrapper(chatClient);

  // Assert
  wrapper.Should().NotBeNull();
  wrapper.Should().BeAssignableTo<IChatClientWrapper>();
 }

 [Fact]
 public async Task CompleteChatAsync_WithValidMessages_CallsChatClient()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  var messages = new List<ChatMessage>
  {
   ChatMessage.CreateUserMessage("Test message")
  };
  var expectedCompletion = Substitute.For<ChatCompletion>();
  
  chatClient.CompleteChatAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatCompletionOptions>(), Arg.Any<CancellationToken>())
   .Returns(Task.FromResult(expectedCompletion));

  // Act
  var result = await wrapper.CompleteChatAsync(messages);

  // Assert
  result.Should().Be(expectedCompletion);
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Is<IEnumerable<ChatMessage>>(m => m.SequenceEqual(messages)),
   Arg.Any<ChatCompletionOptions>(),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task CompleteChatAsync_PassesCancellationToken()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  var cts = new CancellationTokenSource();
  var messages = new List<ChatMessage> { ChatMessage.CreateUserMessage("Test") };

  // Act
  try
  {
   await wrapper.CompleteChatAsync(messages, null, cts.Token);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Any<IEnumerable<ChatMessage>>(),
   Arg.Any<ChatCompletionOptions>(),
   cts.Token);
 }

 [Fact]
 public async Task CompleteChatAsync_WithOptions_PassesOptionsToClient()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  var messages = new List<ChatMessage> { ChatMessage.CreateUserMessage("Test") };
  var options = new ChatCompletionOptions { Temperature = 0.7f };

  // Act
  try
  {
   await wrapper.CompleteChatAsync(messages, options);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Any<IEnumerable<ChatMessage>>(),
   Arg.Is<ChatCompletionOptions>(o => o != null && o.Temperature == 0.7f),
   Arg.Any<CancellationToken>());
 }
}
```

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Services/Ai/EmbeddingClientWrapperTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Services/Ai/EmbeddingClientWrapperTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EmbeddingClientWrapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Notes.Web.Services.Ai;
using OpenAI.Embeddings;

namespace Notes.Web.Tests.Unit.Services.Ai;

/// <summary>
/// Unit tests for EmbeddingClientWrapper validating OpenAI embedding client integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class EmbeddingClientWrapperTests
{
 [Fact]
 public void Constructor_WithNullEmbeddingClient_ThrowsArgumentNullException()
 {
  // Arrange & Act
  var act = () => new EmbeddingClientWrapper(null!);

  // Assert
  act.Should().Throw<ArgumentNullException>()
   .WithParameterName("embeddingClient");
 }

 [Fact]
 public void Constructor_WithValidEmbeddingClient_CreatesInstance()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();

  // Act
  var wrapper = new EmbeddingClientWrapper(embeddingClient);

  // Assert
  wrapper.Should().NotBeNull();
  wrapper.Should().BeAssignableTo<IEmbeddingClientWrapper>();
 }

 [Fact]
 public async Task GenerateEmbeddingAsync_WithValidInput_CallsEmbeddingClient()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();
  var wrapper = new EmbeddingClientWrapper(embeddingClient);
  const string input = "Test input for embedding";
  var expectedEmbedding = Substitute.For<Embedding>();
  
  embeddingClient.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
   .Returns(Task.FromResult(expectedEmbedding));

  // Act
  var result = await wrapper.GenerateEmbeddingAsync(input);

  // Assert
  result.Should().Be(expectedEmbedding);
  await embeddingClient.Received(1).GenerateEmbeddingAsync(
   input,
   Arg.Any<EmbeddingGenerationOptions>(),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task GenerateEmbeddingAsync_PassesCancellationToken()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();
  var wrapper = new EmbeddingClientWrapper(embeddingClient);
  var cts = new CancellationTokenSource();

  // Act
  try
  {
   await wrapper.GenerateEmbeddingAsync("Test", null, cts.Token);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await embeddingClient.Received(1).GenerateEmbeddingAsync(
   Arg.Any<string>(),
   Arg.Any<EmbeddingGenerationOptions>(),
   cts.Token);
 }

 [Fact]
 public async Task GenerateEmbeddingAsync_WithOptions_PassesOptionsToClient()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();
  var wrapper = new EmbeddingClientWrapper(embeddingClient);
  var options = new EmbeddingGenerationOptions { Dimensions = 1536 };

  // Act
  try
  {
   await wrapper.GenerateEmbeddingAsync("Test", options);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await embeddingClient.Received(1).GenerateEmbeddingAsync(
   Arg.Any<string>(),
   Arg.Is<EmbeddingGenerationOptions>(o => o != null && o.Dimensions == 1536),
   Arg.Any<CancellationToken>());
 }
}
```

#### Step 2 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~ChatClientWrapperTests"`
- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~EmbeddingClientWrapperTests"`
- [ ] All AI wrapper tests pass successfully
- [ ] No build errors or warnings

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 3: Create Extension Method Unit Tests

Add unit tests for MongoDbExtensions and AiServiceExtensions to validate service registration.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Extensions/MongoDbExtensionsTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Extensions/MongoDbExtensionsTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notes.Web.Tests.Unit.Extensions;

/// <summary>
/// Unit tests for MongoDbExtensions validating service registration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbExtensionsTests
{
 [Fact]
 public void AddMongoDb_RegistersMongoDbContext()
 {
  // Arrange
  var services = new ServiceCollection();
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var context = serviceProvider.GetService<IMongoDbContext>();
  context.Should().NotBeNull("MongoDbContext should be registered");
 }

 [Fact]
 public void AddMongoDb_RegistersNoteRepository()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var repository = serviceProvider.GetService<INoteRepository>();
  repository.Should().NotBeNull("INoteRepository should be registered");
 }

 [Fact]
 public void AddMongoDb_ReturnsBuilderForChaining()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  var result = builder.AddMongoDb();

  // Assert
  result.Should().BeSameAs(builder, "Extension method should return builder for method chaining");
 }

 [Fact]
 public void AddMongoDb_RegistersServicesAsScoped()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var contextDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IMongoDbContext));
  contextDescriptor.Should().NotBeNull();
  contextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

  var repositoryDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(INoteRepository));
  repositoryDescriptor.Should().NotBeNull();
  repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
 }
}
```

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Extensions/AiServiceExtensionsTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Extensions/AiServiceExtensionsTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AiServiceExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Microsoft.Extensions.DependencyInjection;
using Notes.Web.Services.Ai;

namespace Notes.Web.Tests.Unit.Extensions;

/// <summary>
/// Unit tests for AiServiceExtensions validating AI service registration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AiServiceExtensionsTests
{
 [Fact]
 public void AddAiServices_RegistersChatClientWrapper()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";
  builder.Configuration["AiService:ChatModel"] = "gpt-4";
  builder.Configuration["AiService:EmbeddingModel"] = "text-embedding-3-small";

  // Act
  builder.AddAiServices();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var chatWrapper = serviceProvider.GetService<IChatClientWrapper>();
  chatWrapper.Should().NotBeNull("IChatClientWrapper should be registered");
 }

 [Fact]
 public void AddAiServices_RegistersEmbeddingClientWrapper()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";
  builder.Configuration["AiService:ChatModel"] = "gpt-4";
  builder.Configuration["AiService:EmbeddingModel"] = "text-embedding-3-small";

  // Act
  builder.AddAiServices();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var embeddingWrapper = serviceProvider.GetService<IEmbeddingClientWrapper>();
  embeddingWrapper.Should().NotBeNull("IEmbeddingClientWrapper should be registered");
 }

 [Fact]
 public void AddAiServices_RegistersAiService()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";
  builder.Configuration["AiService:ChatModel"] = "gpt-4";
  builder.Configuration["AiService:EmbeddingModel"] = "text-embedding-3-small";

  // Act
  builder.AddAiServices();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var aiService = serviceProvider.GetService<IAiService>();
  aiService.Should().NotBeNull("IAiService should be registered");
 }

 [Fact]
 public void AddAiServices_ReturnsBuilderForChaining()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";

  // Act
  var result = builder.AddAiServices();

  // Assert
  result.Should().BeSameAs(builder, "Extension method should return builder for method chaining");
 }

 [Fact]
 public void AddAiServices_RegistersChatWrapperAsSingleton()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";

  // Act
  builder.AddAiServices();

  // Assert
  var chatDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IChatClientWrapper));
  chatDescriptor.Should().NotBeNull();
  chatDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
 }

 [Fact]
 public void AddAiServices_RegistersEmbeddingWrapperAsSingleton()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";

  // Act
  builder.AddAiServices();

  // Assert
  var embeddingDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IEmbeddingClientWrapper));
  embeddingDescriptor.Should().NotBeNull();
  embeddingDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
 }

 [Fact]
 public void AddAiServices_RegistersAiServiceAsScoped()
 {
  // Arrange
  var builder = new HostApplicationBuilder();
  builder.Configuration["AiService:ApiKey"] = "test-key";

  // Act
  builder.AddAiServices();

  // Assert
  var aiServiceDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IAiService));
  aiServiceDescriptor.Should().NotBeNull();
  aiServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
 }
}
```

#### Step 3 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~MongoDbExtensionsTests"`
- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~AiServiceExtensionsTests"`
- [ ] All extension method tests pass successfully
- [ ] No build errors or warnings
- [ ] Run all unit tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj`

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

## Final Verification

After completing all steps:

- [ ] All unit tests pass
- [ ] No build errors in the test project
- [ ] Code coverage includes new test files
- [ ] Tests follow project naming and structure conventions
- [ ] All tests have XML documentation comments

## Commands Reference

```powershell
# Run all unit tests
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~NoteServiceTests"

# Run with code coverage
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj /p:CollectCoverage=true

# Run with detailed output
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --logger "console;verbosity=normal"
```
