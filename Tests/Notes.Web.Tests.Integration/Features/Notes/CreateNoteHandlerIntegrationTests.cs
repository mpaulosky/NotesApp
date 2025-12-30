// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateNoteHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.CreateNote;
using Notes.Web.Services.Ai;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for CreateNoteHandler using MongoDB TestContainers and NSubstitute.
/// </summary>
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

		// Create a simple factory that returns the test context
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
		response.Content.Should().Be(command.Content);
		response.AiSummary.Should().Be(expectedSummary);
		response.Tags.Should().Be(expectedTags);
		response.Embedding.Should().BeEquivalentTo(expectedEmbedding);
		response.OwnerSubject.Should().Be(command.UserSubject);
		response.IsArchived.Should().BeFalse();
		response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
		response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

		// Verify AI service was called correctly
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
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

		// Assert
		var context = _fixture.CreateContext();
		var persistedNote = await context.Notes.Find(n => n.Id == response.Id).FirstOrDefaultAsync();

		persistedNote.Should().NotBeNull();
		persistedNote!.Title.Should().Be(command.Title);
		persistedNote.Content.Should().Be(command.Content);
		persistedNote.OwnerSubject.Should().Be(command.UserSubject);
		persistedNote.AiSummary.Should().Be("Summary");
		persistedNote.Tags.Should().Be("tags");
		persistedNote.Embedding.Should().BeEquivalentTo(new float[] { 0.5f });
	}

	[Fact]
	public async Task Handle_CallsAiServicesInParallel()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var summaryDelay = TimeSpan.FromMilliseconds(100);
		var tagsDelay = TimeSpan.FromMilliseconds(100);
		var embeddingDelay = TimeSpan.FromMilliseconds(100);

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(async x =>
			{
				await Task.Delay(summaryDelay);
				return "Summary";
			});
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(async x =>
			{
				await Task.Delay(tagsDelay);
				return "tags";
			});
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(async x =>
			{
				await Task.Delay(embeddingDelay);
				return new float[] { 0.5f };
			});

		var command = new CreateNoteCommand
		{
			Title = "Parallel Test",
			Content = "Testing parallel AI calls",
			UserSubject = "test-user"
		};

		// Act
		var startTime = DateTime.UtcNow;
		await _handler.Handle(command, CancellationToken.None);
		var endTime = DateTime.UtcNow;
		var elapsed = endTime - startTime;

		// Assert - if running in parallel, should take ~100ms, not ~300ms
		elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(250));
	}

	[Fact]
	public async Task Handle_WithEmptyContent_CreatesNoteWithEmptyAiFields()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(string.Empty);
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(string.Empty);
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Array.Empty<float>());

		var command = new CreateNoteCommand
		{
			Title = "Empty Note",
			Content = string.Empty,
			UserSubject = "test-user"
		};

		// Act
		CreateNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.AiSummary.Should().BeEmpty();
		response.Tags.Should().BeEmpty();
		response.Embedding.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_MultipleNotes_AllPersistedCorrectly()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(x => $"Summary for: {x.ArgAt<string>(0)}");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new float[] { 0.1f, 0.2f });

		// Act
		var responses = new List<CreateNoteResponse>();
		for (int i = 1; i <= 3; i++)
		{
			var command = new CreateNoteCommand
			{
				Title = $"Note {i}",
				Content = $"Content for note {i}",
				UserSubject = "test-user"
			};

			var response = await _handler.Handle(command, CancellationToken.None);
			responses.Add(response);
		}

		// Assert
		responses.Should().HaveCount(3);
		responses.Select(r => r.Id).Should().OnlyHaveUniqueItems();

		var context = _fixture.CreateContext();
		var allNotes = await context.Notes.Find(_ => true).ToListAsync();
		allNotes.Should().HaveCount(3);
	}
}
