// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DeleteNoteHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.DeleteNote;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for DeleteNoteHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteNoteHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;
	private readonly IRequestHandler<DeleteNoteCommand, DeleteNoteResponse> _handler;

	public DeleteNoteHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
		_handler = new DeleteNoteHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithValidNote_DeletesSuccessfully()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Note to Delete",
			Content = "This note will be deleted",
			OwnerSubject = "test-user",
			IsArchived = false,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var command = new DeleteNoteCommand
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		DeleteNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Message.Should().Be("Note archived successfully.");

		// Verify note is actually archived, not deleted
		var archivedNote = await _fixture.GetNoteByIdAsync(note.Id);
		archivedNote.Should().NotBeNull();
		archivedNote!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WithNonExistentNote_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var command = new DeleteNoteCommand
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "test-user"
		};

		// Act
		DeleteNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
	}

	[Fact]
	public async Task Handle_WithWrongOwner_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "User A's Note",
			Content = "Content",
			OwnerSubject = "user-a",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var command = new DeleteNoteCommand
		{
			Id = note.Id,
			UserSubject = "user-b" // Different user
		};

		// Act
		DeleteNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");

		// Verify note was NOT archived (should remain unchanged)
		var unchangedNote = await _fixture.GetNoteByIdAsync(note.Id);
		unchangedNote.Should().NotBeNull();
		unchangedNote!.Title.Should().Be("User A's Note");
		unchangedNote.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_DeletesOnlySpecifiedNote()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = await _fixture.CreateTestNotesAsync("test-user", 3);

		var command = new DeleteNoteCommand
		{
			Id = notes[1].Id, // Archive middle note
			UserSubject = "test-user"
		};

		// Act
		DeleteNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeTrue();

		// Verify only the specified note was archived
		var allNotes = await _fixture.GetNotesByUserAsync("test-user");
		allNotes.Should().HaveCount(3); // All notes still exist
		
		var archivedNote = allNotes.FirstOrDefault(n => n.Id == notes[1].Id);
		archivedNote.Should().NotBeNull();
		archivedNote!.IsArchived.Should().BeTrue();
		
		// Verify other notes are NOT archived
		allNotes.Where(n => n.Id != notes[1].Id).Should().OnlyContain(n => !n.IsArchived);
	}

	[Fact]
	public async Task Handle_WithArchivedNote_DeletesSuccessfully()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Archived Note",
			Content = "This is archived",
			OwnerSubject = "test-user",
			IsArchived = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var command = new DeleteNoteCommand
		{
			Id = note.Id,
			UserSubject = "test-user"
		};

		// Act
		DeleteNoteResponse response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeTrue();
		
		// Verify note still exists and remains archived
		var archivedNote = await _fixture.GetNoteByIdAsync(note.Id);
		archivedNote.Should().NotBeNull();
		archivedNote!.IsArchived.Should().BeTrue();
	}
}
