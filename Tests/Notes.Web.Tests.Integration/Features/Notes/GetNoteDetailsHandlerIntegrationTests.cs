// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetNoteDetailsHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.GetNoteDetails;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for GetNoteDetailsHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetNoteDetailsHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;
	private readonly IRequestHandler<GetNoteDetailsQuery, GetNoteDetailsResponse?> _handler;

	public GetNoteDetailsHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
		_handler = new GetNoteDetailsHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithValidNote_ReturnsNoteDetails()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Note",
			Content = "Test content for the note",
			OwnerSubject = "test-user",
			AiSummary = "This is a test summary",
			Tags = "test, sample, integration",
			CreatedAt = DateTime.UtcNow.AddDays(-1),
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeTrue();
		response.Note.Should().NotBeNull();
		response.Note!.Id.Should().Be(note.Id);
		response.Note.Title.Should().Be("Test Note");
		response.Note.Content.Should().Be("Test content for the note");
		response.Note.AiSummary.Should().Be("This is a test summary");
		response.Note.Tags.Should().Be("test, sample, integration");
		response.Note.CreatedAt.Should().BeCloseTo(note.CreatedAt, TimeSpan.FromSeconds(1));
		response.Note.UpdatedAt.Should().BeCloseTo(note.UpdatedAt, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task Handle_WithNonExistentNote_ReturnsNull()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var query = new GetNoteDetailsQuery
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "test-user"
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeFalse();
		response.Note.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithWrongOwner_ReturnsNull()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "User A's Note",
			Content = "Private content",
			OwnerSubject = "user-a",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "user-b" // Different user
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeFalse();
		response.Note.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithNullAiFields_ReturnsNoteWithNullFields()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Note without AI data",
			Content = "Content",
			OwnerSubject = "test-user",
			AiSummary = null,
			Tags = null,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeTrue();
		response.Note.Should().NotBeNull();
		response.Note!.AiSummary.Should().BeNull();
		response.Note.Tags.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithVeryLongContent_ReturnsCompleteContent()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var longContent = new string('x', 10000); // 10KB of content
		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Long Note",
			Content = longContent,
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeTrue();
		response.Note.Should().NotBeNull();
		response.Note!.Content.Should().HaveLength(10000);
		response.Note.Content.Should().Be(longContent);
	}

	[Fact]
	public async Task Handle_WithSpecialCharacters_ReturnsCorrectData()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Special: <>&\"'\\/@#$%",
			Content = "Content with émojis 🎉 and unicode ñ ü",
			OwnerSubject = "test-user",
			Tags = "tag1, tag&special, \"quoted\"",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		GetNoteDetailsResponse? response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeTrue();
		response.Note.Should().NotBeNull();
		response.Note!.Title.Should().Be("Special: <>&\"'\\/@#$%");
		response.Note.Content.Should().Be("Content with émojis 🎉 and unicode ñ ü");
		response.Note.Tags.Should().Be("tag1, tag&special, \"quoted\"");
	}
}
