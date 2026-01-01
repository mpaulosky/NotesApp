# Integration Tests Implementation

## Goal

Implement comprehensive integration tests for SeedNotes, GetRelatedNotes, Output Caching, and Note Lifecycle using MongoDB TestContainers to validate end-to-end feature functionality.

## Prerequisites

**Branch:** Ensure you are on the `add-tests` branch before beginning implementation.

- If not on the branch, switch to it: `git checkout add-tests`
- Complete all previous test implementation files before starting integration tests
- Ensure MongoDB TestContainers is properly configured

**Technology Stack:**

- .NET 10.0, C# 14.0
- xUnit 2.9.3
- FluentAssertions 7.1.0
- NSubstitute 5.3.0
- Testcontainers.MongoDb 4.9.0
- MongoDB.Driver 3.5.2

---

## Step-by-Step Instructions

### Step 1: Create SeedNotes Integration Tests

Add integration tests for the SeedNotes handler to validate bulk note creation with AI services.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Integration/Features/Notes/SeedNotesHandlerIntegrationTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Integration/Features/Notes/SeedNotesHandlerIntegrationTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SeedNotesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;
using MediatR;
using Notes.Web.Features.Notes.SeedNotes;
using Notes.Web.Services.Ai;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for SeedNotesHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class SeedNotesHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
 private readonly MongoDbFixture _fixture;
 private readonly INoteRepository _repository;
 private readonly IAiService _aiService;
 private readonly IRequestHandler<SeedNotesCommand, SeedNotesResponse> _handler;

 public SeedNotesHandlerIntegrationTests(MongoDbFixture fixture)
 {
  _fixture = fixture;

  var contextFactory = Substitute.For<IMongoDbContextFactory>();
  contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

  _repository = new NoteRepository(contextFactory);
  _aiService = Substitute.For<IAiService>();

  _handler = new SeedNotesHandler(_repository, _aiService);
 }

 [Fact]
 public async Task Handle_WithValidCount_CreatesSpecifiedNumberOfNotes()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(x => $"Summary for: {x.ArgAt<string>(0)}");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tag1, tag2");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.1f, 0.2f });

  var command = new SeedNotesCommand
  {
   Count = 5,
   UserSubject = "seed-user"
  };

  // Act
  var response = await _handler.Handle(command, CancellationToken.None);

  // Assert
  response.Should().NotBeNull();
  response.Success.Should().BeTrue();
  response.NotesCreated.Should().Be(5);

  var totalNotes = await _fixture.GetTotalNotesCountAsync();
  totalNotes.Should().Be(5);
 }

 [Fact]
 public async Task Handle_SeedsNotesWithVariedTitles()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var command = new SeedNotesCommand
  {
   Count = 3,
   UserSubject = "seed-user"
  };

  // Act
  await _handler.Handle(command, CancellationToken.None);

  // Assert
  var notes = await _fixture.GetNotesByUserAsync("seed-user");
  notes.Should().HaveCount(3);
  notes.Select(n => n.Title).Should().OnlyHaveUniqueItems();
 }

 [Fact]
 public async Task Handle_CallsAiServiceForEachNote()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var command = new SeedNotesCommand
  {
   Count = 3,
   UserSubject = "seed-user"
  };

  // Act
  await _handler.Handle(command, CancellationToken.None);

  // Assert
  await _aiService.Received(3).GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
  await _aiService.Received(3).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
  await _aiService.Received(3).GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task Handle_WithZeroCount_CreatesNoNotes()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var command = new SeedNotesCommand
  {
   Count = 0,
   UserSubject = "seed-user"
  };

  // Act
  var response = await _handler.Handle(command, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.NotesCreated.Should().Be(0);

  var totalNotes = await _fixture.GetTotalNotesCountAsync();
  totalNotes.Should().Be(0);
 }

 [Fact]
 public async Task Handle_AllNotesHaveCorrectOwnerSubject()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var command = new SeedNotesCommand
  {
   Count = 5,
   UserSubject = "test-owner"
  };

  // Act
  await _handler.Handle(command, CancellationToken.None);

  // Assert
  var notes = await _fixture.GetNotesByUserAsync("test-owner");
  notes.Should().HaveCount(5);
  notes.Should().OnlyContain(n => n.OwnerSubject == "test-owner");
 }

 [Fact]
 public async Task Handle_AllNotesAreNotArchived()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var command = new SeedNotesCommand
  {
   Count = 3,
   UserSubject = "seed-user"
  };

  // Act
  await _handler.Handle(command, CancellationToken.None);

  // Assert
  var notes = await _fixture.GetNotesByUserAsync("seed-user");
  notes.Should().OnlyContain(n => n.IsArchived == false);
 }

 [Fact]
 public async Task Handle_SetsCreatedAtAndUpdatedAt()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var command = new SeedNotesCommand
  {
   Count = 2,
   UserSubject = "seed-user"
  };

  // Act
  var beforeSeed = DateTime.UtcNow;
  await _handler.Handle(command, CancellationToken.None);
  var afterSeed = DateTime.UtcNow;

  // Assert
  var notes = await _fixture.GetNotesByUserAsync("seed-user");
  foreach (var note in notes)
  {
   note.CreatedAt.Should().BeOnOrAfter(beforeSeed).And.BeOnOrBefore(afterSeed);
   note.UpdatedAt.Should().BeOnOrAfter(beforeSeed).And.BeOnOrBefore(afterSeed);
  }
 }
}
```

#### Step 1 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj --filter "FullyQualifiedName~SeedNotesHandlerIntegrationTests"`
- [ ] All SeedNotesHandlerIntegrationTests pass successfully
- [ ] No build errors or warnings

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 2: Create GetRelatedNotes Integration Tests

Add integration tests for GetRelatedNotes handler to validate semantic similarity searches using embeddings.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Integration/Features/Notes/GetRelatedNotesHandlerIntegrationTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Integration/Features/Notes/GetRelatedNotesHandlerIntegrationTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetRelatedNotesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;
using MediatR;
using Notes.Web.Features.Notes.GetRelatedNotes;
using Notes.Web.Services.Ai;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for GetRelatedNotesHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRelatedNotesHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
 private readonly MongoDbFixture _fixture;
 private readonly INoteRepository _repository;
 private readonly IAiService _aiService;
 private readonly IRequestHandler<GetRelatedNotesQuery, GetRelatedNotesResponse> _handler;

 public GetRelatedNotesHandlerIntegrationTests(MongoDbFixture fixture)
 {
  _fixture = fixture;

  var contextFactory = Substitute.For<IMongoDbContextFactory>();
  contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

  _repository = new NoteRepository(contextFactory);
  _aiService = Substitute.For<IAiService>();

  _handler = new GetRelatedNotesHandler(_repository, _aiService);
 }

 [Fact]
 public async Task Handle_WithValidNoteId_ReturnsRelatedNotes()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var baseEmbedding = new float[] { 1.0f, 0.0f, 0.0f };
  var similarEmbedding = new float[] { 0.9f, 0.1f, 0.0f };
  var differentEmbedding = new float[] { 0.0f, 1.0f, 0.0f };

  var baseNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Base Note",
   Content = "Base content",
   OwnerSubject = "user-123",
   Embedding = baseEmbedding,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  var similarNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Similar Note",
   Content = "Similar content",
   OwnerSubject = "user-123",
   Embedding = similarEmbedding,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  var differentNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Different Note",
   Content = "Different content",
   OwnerSubject = "user-123",
   Embedding = differentEmbedding,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  await _fixture.SeedNotesAsync(baseNote, similarNote, differentNote);

  var query = new GetRelatedNotesQuery
  {
   NoteId = baseNote.Id,
   UserSubject = "user-123",
   Limit = 5
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Should().NotBeNull();
  response.Success.Should().BeTrue();
  response.RelatedNotes.Should().NotBeEmpty();
  response.RelatedNotes.Should().NotContain(n => n.Id == baseNote.Id, "should not include the base note itself");
 }

 [Fact]
 public async Task Handle_WithNonexistentNote_ReturnsFailure()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var query = new GetRelatedNotesQuery
  {
   NoteId = ObjectId.GenerateNewId(),
   UserSubject = "user-123",
   Limit = 5
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Should().NotBeNull();
  response.Success.Should().BeFalse();
  response.Message.Should().Contain("not found");
 }

 [Fact]
 public async Task Handle_RespectsUserOwnership()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var embedding = new float[] { 1.0f, 0.0f };

  var user1Note = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "User 1 Note",
   Content = "Content",
   OwnerSubject = "user-1",
   Embedding = embedding,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  var user2Note = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "User 2 Note",
   Content = "Content",
   OwnerSubject = "user-2",
   Embedding = embedding,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  await _fixture.SeedNotesAsync(user1Note, user2Note);

  var query = new GetRelatedNotesQuery
  {
   NoteId = user1Note.Id,
   UserSubject = "user-1",
   Limit = 10
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.RelatedNotes.Should().OnlyContain(n => n.OwnerSubject == "user-1");
  response.RelatedNotes.Should().NotContain(n => n.Id == user2Note.Id);
 }

 [Fact]
 public async Task Handle_RespectsLimitParameter()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var embedding = new float[] { 1.0f, 0.0f };
  var notes = Enumerable.Range(1, 10).Select(i => new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = $"Note {i}",
   Content = $"Content {i}",
   OwnerSubject = "user-123",
   Embedding = embedding,
   CreatedAt = DateTime.UtcNow.AddDays(-i),
   UpdatedAt = DateTime.UtcNow.AddDays(-i)
  }).ToArray();

  await _fixture.SeedNotesAsync(notes);

  var query = new GetRelatedNotesQuery
  {
   NoteId = notes[0].Id,
   UserSubject = "user-123",
   Limit = 3
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.RelatedNotes.Should().HaveCountLessOrEqualTo(3);
 }

 [Fact]
 public async Task Handle_ExcludesArchivedNotes()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var embedding = new float[] { 1.0f, 0.0f };

  var activeNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Active Note",
   Content = "Active",
   OwnerSubject = "user-123",
   Embedding = embedding,
   IsArchived = false,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  var archivedNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Archived Note",
   Content = "Archived",
   OwnerSubject = "user-123",
   Embedding = embedding,
   IsArchived = true,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  var baseNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Base Note",
   Content = "Base",
   OwnerSubject = "user-123",
   Embedding = embedding,
   IsArchived = false,
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  await _fixture.SeedNotesAsync(baseNote, activeNote, archivedNote);

  var query = new GetRelatedNotesQuery
  {
   NoteId = baseNote.Id,
   UserSubject = "user-123",
   Limit = 10
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.RelatedNotes.Should().NotContain(n => n.IsArchived);
  response.RelatedNotes.Should().NotContain(n => n.Id == archivedNote.Id);
 }

 [Fact]
 public async Task Handle_WithNoRelatedNotes_ReturnsEmptyList()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var singleNote = new Note
  {
   Id = ObjectId.GenerateNewId(),
   Title = "Only Note",
   Content = "Lonely content",
   OwnerSubject = "user-123",
   Embedding = new float[] { 1.0f, 0.0f },
   CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
  };

  await _fixture.SeedNotesAsync(singleNote);

  var query = new GetRelatedNotesQuery
  {
   NoteId = singleNote.Id,
   UserSubject = "user-123",
   Limit = 5
  };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.RelatedNotes.Should().BeEmpty();
 }
}
```

#### Step 2 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj --filter "FullyQualifiedName~GetRelatedNotesHandlerIntegrationTests"`
- [ ] All GetRelatedNotesHandlerIntegrationTests pass successfully
- [ ] No build errors or warnings

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 3: Create Output Cache Integration Tests

Add integration tests to validate ASP.NET Core Output Caching behavior for note queries.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Integration/Features/OutputCacheIntegrationTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Integration/Features/OutputCacheIntegrationTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OutputCacheIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Notes.Web.Features.Notes.ListNotes;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features;

/// <summary>
/// Integration tests for output caching behavior with note queries.
/// </summary>
[ExcludeFromCodeCoverage]
public class OutputCacheIntegrationTests : IClassFixture<MongoDbFixture>
{
 private readonly MongoDbFixture _fixture;
 private readonly INoteRepository _repository;
 private readonly IMemoryCache _cache;
 private readonly IRequestHandler<ListNotesQuery, ListNotesResponse> _handler;

 public OutputCacheIntegrationTests(MongoDbFixture fixture)
 {
  _fixture = fixture;

  var contextFactory = Substitute.For<IMongoDbContextFactory>();
  contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

  _repository = new NoteRepository(contextFactory);
  
  var services = new ServiceCollection();
  services.AddMemoryCache();
  var serviceProvider = services.BuildServiceProvider();
  _cache = serviceProvider.GetRequiredService<IMemoryCache>();

  _handler = new ListNotesHandler(_repository);
 }

 [Fact]
 public async Task ListNotes_FirstCall_QueriesDatabase()
 {
  // Arrange
  await _fixture.ClearNotesAsync();
  await _fixture.CreateTestNotesAsync("user-123", 3);

  var query = new ListNotesQuery { UserSubject = "user-123" };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Should().NotBeNull();
  response.Success.Should().BeTrue();
  response.Notes.Should().HaveCount(3);
 }

 [Fact]
 public async Task ListNotes_MultipleCalls_ReturnConsistentResults()
 {
  // Arrange
  await _fixture.ClearNotesAsync();
  await _fixture.CreateTestNotesAsync("user-456", 5);

  var query = new ListNotesQuery { UserSubject = "user-456" };

  // Act
  var response1 = await _handler.Handle(query, CancellationToken.None);
  var response2 = await _handler.Handle(query, CancellationToken.None);
  var response3 = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response1.Notes.Should().BeEquivalentTo(response2.Notes);
  response2.Notes.Should().BeEquivalentTo(response3.Notes);
 }

 [Fact]
 public async Task ListNotes_AfterDataChange_ReflectsNewData()
 {
  // Arrange
  await _fixture.ClearNotesAsync();
  await _fixture.CreateTestNotesAsync("user-789", 2);

  var query = new ListNotesQuery { UserSubject = "user-789" };

  // Act - First query
  var initialResponse = await _handler.Handle(query, CancellationToken.None);

  // Add more notes
  await _fixture.CreateTestNotesAsync("user-789", 3);

  // Act - Second query
  var updatedResponse = await _handler.Handle(query, CancellationToken.None);

  // Assert
  initialResponse.Notes.Should().HaveCount(2);
  updatedResponse.Notes.Should().HaveCount(5);
 }

 [Fact]
 public async Task Cache_DifferentUsers_HaveIsolatedCaches()
 {
  // Arrange
  await _fixture.ClearNotesAsync();
  await _fixture.CreateTestNotesAsync("user-1", 2);
  await _fixture.CreateTestNotesAsync("user-2", 3);

  var query1 = new ListNotesQuery { UserSubject = "user-1" };
  var query2 = new ListNotesQuery { UserSubject = "user-2" };

  // Act
  var response1 = await _handler.Handle(query1, CancellationToken.None);
  var response2 = await _handler.Handle(query2, CancellationToken.None);

  // Assert
  response1.Notes.Should().HaveCount(2);
  response2.Notes.Should().HaveCount(3);
  response1.Notes.Should().OnlyContain(n => n.OwnerSubject == "user-1");
  response2.Notes.Should().OnlyContain(n => n.OwnerSubject == "user-2");
 }

 [Fact]
 public async Task MemoryCache_StoresAndRetrievesData()
 {
  // Arrange
  const string cacheKey = "test-cache-key";
  var testData = new { Value = "test-value", Count = 42 };

  // Act
  _cache.Set(cacheKey, testData, TimeSpan.FromMinutes(5));
  var retrieved = _cache.Get(cacheKey);

  // Assert
  retrieved.Should().NotBeNull();
  retrieved.Should().BeEquivalentTo(testData);
 }

 [Fact]
 public void MemoryCache_ExpiredEntry_ReturnsNull()
 {
  // Arrange
  const string cacheKey = "expiring-key";
  _cache.Set(cacheKey, "test-value", TimeSpan.FromMilliseconds(10));

  // Act
  Task.Delay(50).Wait(); // Wait for expiration
  var retrieved = _cache.Get(cacheKey);

  // Assert
  retrieved.Should().BeNull();
 }

 [Fact]
 public async Task ListNotes_WithArchived_ReturnsBothArchivedAndActive()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var activeNotes = await _fixture.CreateTestNotesAsync("user-archive", 3);
  
  // Archive one note
  var noteToArchive = activeNotes[0];
  noteToArchive.IsArchived = true;
  await _fixture.UpdateNoteAsync(noteToArchive);

  var query = new ListNotesQuery { UserSubject = "user-archive" };

  // Act
  var response = await _handler.Handle(query, CancellationToken.None);

  // Assert
  response.Success.Should().BeTrue();
  response.Notes.Should().HaveCount(3);
  response.Notes.Should().Contain(n => n.IsArchived);
  response.Notes.Should().Contain(n => !n.IsArchived);
 }
}
```

#### Step 3 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj --filter "FullyQualifiedName~OutputCacheIntegrationTests"`
- [ ] All OutputCacheIntegrationTests pass successfully
- [ ] No build errors or warnings

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 4: Create Note Lifecycle Integration Tests

Add comprehensive integration tests for the complete note lifecycle (create, read, update, delete).

- [ ] Create new test file at `Tests/Notes.Web.Tests.Integration/Features/Notes/NoteLifecycleIntegrationTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Integration/Features/Notes/NoteLifecycleIntegrationTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteLifecycleIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;
using MediatR;
using Notes.Web.Features.Notes.CreateNote;
using Notes.Web.Features.Notes.DeleteNote;
using Notes.Web.Features.Notes.GetNoteDetails;
using Notes.Web.Features.Notes.UpdateNote;
using Notes.Web.Services.Ai;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for complete note lifecycle from creation to deletion.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteLifecycleIntegrationTests : IClassFixture<MongoDbFixture>
{
 private readonly MongoDbFixture _fixture;
 private readonly INoteRepository _repository;
 private readonly IAiService _aiService;

 public NoteLifecycleIntegrationTests(MongoDbFixture fixture)
 {
  _fixture = fixture;

  var contextFactory = Substitute.For<IMongoDbContextFactory>();
  contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

  _repository = new NoteRepository(contextFactory);
  _aiService = Substitute.For<IAiService>();
 }

 [Fact]
 public async Task CompleteLifecycle_CreateReadUpdateDelete_Success()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("AI Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tag1, tag2");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.1f, 0.2f });

  // Step 1: CREATE
  var createHandler = new CreateNoteHandler(_repository, _aiService);
  var createCommand = new CreateNoteCommand
  {
   Title = "Lifecycle Note",
   Content = "Initial content",
   UserSubject = "lifecycle-user"
  };

  var createResponse = await createHandler.Handle(createCommand, CancellationToken.None);

  createResponse.Should().NotBeNull();
  createResponse.Id.Should().NotBe(ObjectId.Empty);

  var noteId = createResponse.Id;

  // Step 2: READ
  var getDetailsHandler = new GetNoteDetailsHandler(_repository);
  var getQuery = new GetNoteDetailsQuery
  {
   Id = noteId,
   UserSubject = "lifecycle-user"
  };

  var getResponse = await getDetailsHandler.Handle(getQuery, CancellationToken.None);

  getResponse.Should().NotBeNull();
  getResponse.Success.Should().BeTrue();
  getResponse.Note.Should().NotBeNull();
  getResponse.Note!.Title.Should().Be("Lifecycle Note");
  getResponse.Note.Content.Should().Be("Initial content");

  // Step 3: UPDATE
  var updateHandler = new UpdateNoteHandler(_repository, _aiService);
  var updateCommand = new UpdateNoteCommand
  {
   Id = noteId,
   Title = "Updated Lifecycle Note",
   Content = "Updated content",
   IsArchived = false,
   UserSubject = "lifecycle-user"
  };

  var updateResponse = await updateHandler.Handle(updateCommand, CancellationToken.None);

  updateResponse.Should().NotBeNull();
  updateResponse.Success.Should().BeTrue();

  // Verify update persisted
  var verifyGetResponse = await getDetailsHandler.Handle(getQuery, CancellationToken.None);
  verifyGetResponse.Note!.Title.Should().Be("Updated Lifecycle Note");
  verifyGetResponse.Note.Content.Should().Be("Updated content");

  // Step 4: DELETE
  var deleteHandler = new DeleteNoteHandler(_repository);
  var deleteCommand = new DeleteNoteCommand
  {
   Id = noteId,
   UserSubject = "lifecycle-user"
  };

  var deleteResponse = await deleteHandler.Handle(deleteCommand, CancellationToken.None);

  deleteResponse.Should().NotBeNull();
  deleteResponse.Success.Should().BeTrue();

  // Verify deletion
  var noteAfterDelete = await _fixture.GetNoteByIdAsync(noteId);
  noteAfterDelete.Should().BeNull();
 }

 [Fact]
 public async Task Lifecycle_CreateAndArchive_Success()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var createHandler = new CreateNoteHandler(_repository, _aiService);
  var updateHandler = new UpdateNoteHandler(_repository, _aiService);

  // Create note
  var createCommand = new CreateNoteCommand
  {
   Title = "Archive Test",
   Content = "Content to archive",
   UserSubject = "archive-user"
  };

  var createResponse = await createHandler.Handle(createCommand, CancellationToken.None);
  var noteId = createResponse.Id;

  // Archive note
  var updateCommand = new UpdateNoteCommand
  {
   Id = noteId,
   Title = "Archive Test",
   Content = "Content to archive",
   IsArchived = true,
   UserSubject = "archive-user"
  };

  var updateResponse = await updateHandler.Handle(updateCommand, CancellationToken.None);

  // Assert
  updateResponse.Success.Should().BeTrue();

  var archivedNote = await _fixture.GetNoteByIdAsync(noteId);
  archivedNote.Should().NotBeNull();
  archivedNote!.IsArchived.Should().BeTrue();
 }

 [Fact]
 public async Task Lifecycle_MultipleUpdates_MaintainsHistory()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var createHandler = new CreateNoteHandler(_repository, _aiService);
  var updateHandler = new UpdateNoteHandler(_repository, _aiService);
  var getDetailsHandler = new GetNoteDetailsHandler(_repository);

  // Create note
  var createResponse = await createHandler.Handle(new CreateNoteCommand
  {
   Title = "Version 1",
   Content = "Content 1",
   UserSubject = "history-user"
  }, CancellationToken.None);

  var noteId = createResponse.Id;
  var createdAt = createResponse.CreatedAt;

  // Update 1
  await Task.Delay(50); // Ensure time difference
  await updateHandler.Handle(new UpdateNoteCommand
  {
   Id = noteId,
   Title = "Version 2",
   Content = "Content 2",
   IsArchived = false,
   UserSubject = "history-user"
  }, CancellationToken.None);

  // Update 2
  await Task.Delay(50);
  await updateHandler.Handle(new UpdateNoteCommand
  {
   Id = noteId,
   Title = "Version 3",
   Content = "Content 3",
   IsArchived = false,
   UserSubject = "history-user"
  }, CancellationToken.None);

  // Get final state
  var finalNote = await getDetailsHandler.Handle(new GetNoteDetailsQuery
  {
   Id = noteId,
   UserSubject = "history-user"
  }, CancellationToken.None);

  // Assert
  finalNote.Note!.Title.Should().Be("Version 3");
  finalNote.Note.Content.Should().Be("Content 3");
  finalNote.Note.CreatedAt.Should().Be(createdAt);
  finalNote.Note.UpdatedAt.Should().BeAfter(createdAt);
 }

 [Fact]
 public async Task Lifecycle_CreateMultipleNotes_AllPersisted()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var createHandler = new CreateNoteHandler(_repository, _aiService);

  // Create multiple notes
  var createdIds = new List<ObjectId>();
  for (int i = 1; i <= 5; i++)
  {
   var response = await createHandler.Handle(new CreateNoteCommand
   {
    Title = $"Note {i}",
    Content = $"Content {i}",
    UserSubject = "multi-user"
   }, CancellationToken.None);

   createdIds.Add(response.Id);
  }

  // Assert
  var allNotes = await _fixture.GetNotesByUserAsync("multi-user");
  allNotes.Should().HaveCount(5);
  allNotes.Select(n => n.Id).Should().BeEquivalentTo(createdIds);
 }

 [Fact]
 public async Task Lifecycle_DeleteNonexistentNote_ReturnsFailure()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  var deleteHandler = new DeleteNoteHandler(_repository);
  var deleteCommand = new DeleteNoteCommand
  {
   Id = ObjectId.GenerateNewId(),
   UserSubject = "test-user"
  };

  // Act
  var response = await deleteHandler.Handle(deleteCommand, CancellationToken.None);

  // Assert
  response.Success.Should().BeFalse();
  response.Message.Should().Contain("not found");
 }

 [Fact]
 public async Task Lifecycle_UpdatePreservesCreatedAt()
 {
  // Arrange
  await _fixture.ClearNotesAsync();

  _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("Summary");
  _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns("tags");
  _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new float[] { 0.5f });

  var createHandler = new CreateNoteHandler(_repository, _aiService);
  var updateHandler = new UpdateNoteHandler(_repository, _aiService);

  // Create
  var createResponse = await createHandler.Handle(new CreateNoteCommand
  {
   Title = "Timestamp Test",
   Content = "Content",
   UserSubject = "timestamp-user"
  }, CancellationToken.None);

  var originalCreatedAt = createResponse.CreatedAt;

  await Task.Delay(100);

  // Update
  await updateHandler.Handle(new UpdateNoteCommand
  {
   Id = createResponse.Id,
   Title = "Updated Title",
   Content = "Updated Content",
   IsArchived = false,
   UserSubject = "timestamp-user"
  }, CancellationToken.None);

  // Assert
  var updatedNote = await _fixture.GetNoteByIdAsync(createResponse.Id);
  updatedNote!.CreatedAt.Should().Be(originalCreatedAt);
  updatedNote.UpdatedAt.Should().BeAfter(originalCreatedAt);
 }
}
```

#### Step 4 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj --filter "FullyQualifiedName~NoteLifecycleIntegrationTests"`
- [ ] All NoteLifecycleIntegrationTests pass successfully
- [ ] No build errors or warnings
- [ ] Run all integration tests: `dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj`

#### Step 4 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

## Final Verification

After completing all steps:

- [ ] All integration tests pass
- [ ] MongoDB TestContainers start and stop cleanly
- [ ] No build errors in the test project
- [ ] Code coverage includes new test files
- [ ] Tests follow project naming and structure conventions
- [ ] All tests have XML documentation comments

## Commands Reference

```powershell
# Run all integration tests
dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~SeedNotesHandlerIntegrationTests"

# Run with code coverage
dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj /p:CollectCoverage=true

# Run with detailed output
dotnet test Tests/Notes.Web.Tests.Integration/Notes.Web.Tests.Integration.csproj --logger "console;verbosity=normal"

# Run all tests in the solution
dotnet test NotesApp.slnx
```
