// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteRepositoryIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Repositories;

/// <summary>
/// Integration tests for NoteRepository using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteRepositoryIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;

	public NoteRepositoryIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		// Create a simple factory that returns the test context
		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
	}

	[Fact]
	public async Task GetNoteByIdAsync_WhenNoteExists_ReturnsNote()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		// Act
		Result<Note?> result = await _repository.GetNoteByIdAsync(note.Id);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(note.Id);
		result.Value.Title.Should().Be(note.Title);
		result.Value.Content.Should().Be(note.Content);
	}

	[Fact]
	public async Task GetNoteByIdAsync_WhenNoteDoesNotExist_ReturnsNull()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		Result<Note?> result = await _repository.GetNoteByIdAsync(nonExistentId);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeNull();
	}

	[Fact]
	public async Task GetNotes_ReturnsAllNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 1",
				Content = "Content 1",
				OwnerSubject = "user-1",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 2",
				Content = "Content 2",
				OwnerSubject = "user-2",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		// Act
		Result<IEnumerable<Note>?> result = await _repository.GetNotes();

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetNotes_WithPredicate_ReturnsFilteredNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var targetOwner = "target-user";
		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "User Note",
				Content = "Content",
				OwnerSubject = targetOwner,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Other Note",
				Content = "Content",
				OwnerSubject = "other-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		// Act
		Result<IEnumerable<Note>?> result = await _repository.GetNotes(n => n.OwnerSubject == targetOwner);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(1);
		result.Value!.First().OwnerSubject.Should().Be(targetOwner);
	}

	[Fact]
	public async Task GetNotes_WithPagination_ReturnsPagedResults()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = Enumerable.Range(1, 10).Select(i => new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = $"Note {i}",
			Content = $"Content {i}",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow.AddMinutes(-i),
			UpdatedAt = DateTime.UtcNow
		}).ToArray();

		await _fixture.SeedNotesAsync(notes);

		// Act
		Result<IEnumerable<Note>?> result = await _repository.GetNotes(
			n => n.OwnerSubject == "test-user",
			skip: 2,
			take: 3);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetNoteCount_ReturnsCorrectCount()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var targetOwner = "target-user";
		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 1",
				Content = "Content",
				OwnerSubject = targetOwner,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 2",
				Content = "Content",
				OwnerSubject = targetOwner,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 3",
				Content = "Content",
				OwnerSubject = "other-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		// Act
		Result<int> result = await _repository.GetNoteCount(n => n.OwnerSubject == targetOwner);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(2);
	}

	[Fact]
	public async Task AddNote_AddsNoteToDatabase()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "New Note",
			Content = "New Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		// Act
		Result result = await _repository.AddNote(note);

		// Assert
		result.Success.Should().BeTrue();

		// Verify it was added
		var context = _fixture.CreateContext();
		var savedNote = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();
		savedNote.Should().NotBeNull();
		savedNote!.Title.Should().Be(note.Title);
	}

	[Fact]
	public async Task UpdateNote_UpdatesExistingNote()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original Title",
			Content = "Original Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		note.Title = "Updated Title";
		note.Content = "Updated Content";
		note.UpdatedAt = DateTime.UtcNow;

		// Act
		Result result = await _repository.UpdateNote(note);

		// Assert
		result.Success.Should().BeTrue();

		// Verify it was updated
		var context = _fixture.CreateContext();
		var updatedNote = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();
		updatedNote.Should().NotBeNull();
		updatedNote!.Title.Should().Be("Updated Title");
		updatedNote.Content.Should().Be("Updated Content");
	}

	[Fact]
	public async Task DeleteNote_RemovesNoteFromDatabase()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "To Delete",
			Content = "Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		// Act
		Result result = await _repository.DeleteNote(note);

		// Assert
		result.Success.Should().BeTrue();

		// Verify it was deleted
		var context = _fixture.CreateContext();
		var deletedNote = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();
		deletedNote.Should().BeNull();
	}

	[Fact]
	public async Task ArchiveNote_SetsIsArchivedToTrue()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "To Archive",
			Content = "Content",
			OwnerSubject = "test-user",
			IsArchived = false,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		// Act
		Result result = await _repository.ArchiveNote(note);

		// Assert
		result.Success.Should().BeTrue();

		// Verify it was archived
		var context = _fixture.CreateContext();
		var archivedNote = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();
		archivedNote.Should().NotBeNull();
		archivedNote!.IsArchived.Should().BeTrue();
	}
}
