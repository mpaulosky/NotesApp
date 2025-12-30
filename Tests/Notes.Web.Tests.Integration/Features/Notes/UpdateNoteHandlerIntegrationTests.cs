// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     UpdateNoteHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.UpdateNote;
using Notes.Web.Services.Ai;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for UpdateNoteHandler using MongoDB TestContainers and NSubstitute.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateNoteHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly IRequestHandler<UpdateNoteCommand, UpdateNoteResponse> _handler;

	public UpdateNoteHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
		_aiService = Substitute.For<IAiService>();

		_handler = new UpdateNoteHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_WithValidNote_UpdatesNoteSuccessfully()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var existingNote = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original Title",
			Content = "Original Content",
			OwnerSubject = "test-user-123",
			AiSummary = "Old summary",
			Tags = "old, tags",
			Embedding = new float[] { 0.1f },
			CreatedAt = DateTime.UtcNow.AddDays(-1),
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};

		await _fixture.SeedNotesAsync(existingNote);

		var newSummary = "New AI summary";
		var newTags = "new, updated, tags";
		var newEmbedding = new float[] { 0.5f, 0.6f };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(newSummary);
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(newTags);
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(newEmbedding);

		var command = new UpdateNoteCommand
		{
			Id = existingNote.Id,
			Title = "Updated Title",
			Content = "Updated Content",
			UserSubject = "test-user-123"
		};

		// Act
		UpdateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Id.Should().Be(existingNote.Id);
		response.Title.Should().Be(command.Title);
		response.Content.Should().Be(command.Content);
		response.AiSummary.Should().Be(newSummary);
		response.Tags.Should().Be(newTags);
		response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

		// Verify AI service was called
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_PersistsUpdatesToDatabase()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var existingNote = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original",
			Content = "Original",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(existingNote);

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("New Summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("new-tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new float[] { 0.9f });

		var command = new UpdateNoteCommand
		{
			Id = existingNote.Id,
			Title = "Updated Title",
			Content = "Updated Content",
			UserSubject = "test-user"
		};

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		var context = _fixture.CreateContext();
		var updatedNote = await context.Notes.Find(n => n.Id == existingNote.Id).FirstOrDefaultAsync();

		updatedNote.Should().NotBeNull();
		updatedNote!.Title.Should().Be("Updated Title");
		updatedNote.Content.Should().Be("Updated Content");
		updatedNote.AiSummary.Should().Be("New Summary");
		updatedNote.Tags.Should().Be("new-tags");
		updatedNote.Embedding.Should().BeEquivalentTo(new float[] { 0.9f });
		updatedNote.UpdatedAt.Should().BeAfter(existingNote.UpdatedAt);
	}

	[Fact]
	public async Task Handle_WithNonExistentNote_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var command = new UpdateNoteCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Title",
			Content = "Content",
			UserSubject = "test-user"
		};

		// Act
		UpdateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");

		// AI service should not be called
		await _aiService.DidNotReceive().GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithWrongOwner_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var existingNote = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "User A's Note",
			Content = "Content",
			OwnerSubject = "user-a",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(existingNote);

		var command = new UpdateNoteCommand
		{
			Id = existingNote.Id,
			Title = "Trying to update",
			Content = "Should fail",
			UserSubject = "user-b"  // Different user
		};

		// Act
		UpdateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");

		// Verify note was not modified
		var context = _fixture.CreateContext();
		var unchangedNote = await context.Notes.Find(n => n.Id == existingNote.Id).FirstOrDefaultAsync();
		unchangedNote!.Title.Should().Be("User A's Note");
		unchangedNote.Content.Should().Be("Content");
	}

	[Fact]
	public async Task Handle_RegeneratesAiContentOnEveryUpdate()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var existingNote = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Title",
			Content = "Original Content",
			OwnerSubject = "test-user",
			AiSummary = "Old summary",
			Tags = "old",
			Embedding = new float[] { 0.1f },
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(existingNote);

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("Regenerated summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("regenerated, tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new float[] { 0.8f, 0.9f });

		var command = new UpdateNoteCommand
		{
			Id = existingNote.Id,
			Title = "Same Title",
			Content = "New Content",
			UserSubject = "test-user"
		};

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert - AI content should be regenerated
		await _aiService.Received(1).GenerateSummaryAsync("New Content", Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync("Same Title", "New Content", Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync("New Content", Arg.Any<CancellationToken>());

		var context = _fixture.CreateContext();
		var updatedNote = await context.Notes.Find(n => n.Id == existingNote.Id).FirstOrDefaultAsync();
		updatedNote!.AiSummary.Should().Be("Regenerated summary");
		updatedNote.Tags.Should().Be("regenerated, tags");
		updatedNote.Embedding.Should().BeEquivalentTo(new float[] { 0.8f, 0.9f });
	}

	[Fact]
	public async Task Handle_PreservesCreatedAtTimestamp()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var originalCreatedAt = DateTime.UtcNow.AddDays(-7);
		var existingNote = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Title",
			Content = "Content",
			OwnerSubject = "test-user",
			CreatedAt = originalCreatedAt,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(existingNote);

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("Summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new float[] { 0.5f });

		var command = new UpdateNoteCommand
		{
			Id = existingNote.Id,
			Title = "Updated",
			Content = "Updated",
			UserSubject = "test-user"
		};

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		var context = _fixture.CreateContext();
		var updatedNote = await context.Notes.Find(n => n.Id == existingNote.Id).FirstOrDefaultAsync();

		updatedNote!.CreatedAt.Should().BeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(10));
		updatedNote.UpdatedAt.Should().BeAfter(originalCreatedAt);
	}
}
