// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetRelatedNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Features.Notes.GetRelatedNotes;
using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.GetRelatedNotes;

[ExcludeFromCodeCoverage]
public class GetRelatedNotesHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly GetRelatedNotesHandler _handler;

	public GetRelatedNotesHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_aiService = Substitute.For<IAiService>();
		_handler = new GetRelatedNotesHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_WithValidNoteAndRelatedNotes_ShouldReturnRelatedNotes()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(
			title: "Current Note",
			content: "Content",
			ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var relatedNote1 = TestDataBuilder.CreateNote(
			title: "Related Note 1",
			content: "Content 1",
			aiSummary: "Summary 1",
			ownerSubject: userSubject);
		var relatedNote2 = TestDataBuilder.CreateNote(
			title: "Related Note 2",
			content: "Content 2",
			aiSummary: "Summary 2",
			ownerSubject: userSubject);

		var relatedNoteIds = new List<ObjectId> { relatedNote1.Id, relatedNote2.Id };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject,
			TopN = 5
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				embedding,
				userSubject,
				noteId,
				5,
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { relatedNote1, relatedNote2 }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().HaveCount(2);
		response.RelatedNotes[0].Id.Should().Be(relatedNote1.Id);
		response.RelatedNotes[0].Title.Should().Be("Related Note 1");
		response.RelatedNotes[0].AiSummary.Should().Be("Summary 1");
		response.RelatedNotes[1].Id.Should().Be(relatedNote2.Id);
		response.RelatedNotes[1].Title.Should().Be("Related Note 2");
	}

	[Fact]
	public async Task Handle_WhenNoteNotFound_ShouldReturnEmptyResponse()
	{
		// Arrange
		var query = new GetRelatedNotesQuery
		{
			NoteId = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(query.NoteId)
			.Returns(Result.Fail<Note>("Note not found"));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
		await _aiService.DidNotReceive().FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenRepositoryReturnsNull_ShouldReturnEmptyResponse()
	{
		// Arrange
		var query = new GetRelatedNotesQuery
		{
			NoteId = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(query.NoteId)
			.Returns(Result.Ok<Note>(null!));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WhenUserNotOwner_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var currentNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|owner123");
		currentNote.Id = noteId;
		currentNote.Embedding = new[] { 0.1f, 0.2f };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = "auth0|different_user"
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
		await _aiService.DidNotReceive().FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenNoteHasNoEmbedding_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var userSubject = "auth0|test123";
		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = null;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
		await _aiService.DidNotReceive().FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenNoRelatedNotesFound_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject,
			TopN = 5
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				embedding,
				userSubject,
				noteId,
				5,
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
		await _repository.DidNotReceive().GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>());
	}

	[Fact]
	public async Task Handle_WhenGetNotesFailsForRelatedNotes_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var relatedNoteIds = new List<ObjectId> { ObjectId.GenerateNewId() };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Fail<IEnumerable<Note>>("Database error"));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WhenGetNotesReturnsNull_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var relatedNoteIds = new List<ObjectId> { ObjectId.GenerateNewId() };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(null!));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.RelatedNotes.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_ShouldPreserveSortOrderFromAiService()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var note1 = TestDataBuilder.CreateNote(title: "Note 1", ownerSubject: userSubject);
		var note2 = TestDataBuilder.CreateNote(title: "Note 2", ownerSubject: userSubject);
		var note3 = TestDataBuilder.CreateNote(title: "Note 3", ownerSubject: userSubject);

		// AI service returns in specific order (by similarity)
		var relatedNoteIds = new List<ObjectId> { note3.Id, note1.Id, note2.Id };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		// Repository returns in different order
		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note1, note2, note3 }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.RelatedNotes.Should().HaveCount(3);
		response.RelatedNotes[0].Id.Should().Be(note3.Id); // AI order preserved
		response.RelatedNotes[1].Id.Should().Be(note1.Id);
		response.RelatedNotes[2].Id.Should().Be(note2.Id);
	}

	[Fact]
	public async Task Handle_WithTopN_ShouldPassToAiService()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject,
			TopN = 10
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				10,
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _aiService.Received(1).FindRelatedNotesAsync(
			embedding,
			userSubject,
			noteId,
			10,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithDefaultTopN_ShouldUse5()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
			// TopN not specified, should default to 5
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				5,
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _aiService.Received(1).FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			5,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldPassCancellationToken()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";
		var cts = new CancellationTokenSource();
		var cancellationToken = cts.Token;

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				cancellationToken)
			.Returns(new List<ObjectId>());

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _aiService.Received(1).FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenSomeRelatedNotesNotFoundInRepository_ShouldReturnOnlyFoundNotes()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var existingNote = TestDataBuilder.CreateNote(title: "Existing Note", ownerSubject: userSubject);
		var missingNoteId = ObjectId.GenerateNewId();

		// AI returns 2 IDs, but only 1 exists in repository
		var relatedNoteIds = new List<ObjectId> { existingNote.Id, missingNoteId };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		// Repository only returns the existing note
		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { existingNote }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.RelatedNotes.Should().HaveCount(1);
		response.RelatedNotes[0].Id.Should().Be(existingNote.Id);
		response.RelatedNotes[0].Title.Should().Be("Existing Note");
	}

	[Fact]
	public async Task Handle_ShouldMapAllFieldsCorrectly()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";
		var updatedAt = DateTime.UtcNow.AddDays(-5);

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var relatedNote = TestDataBuilder.CreateNote(
			title: "Test Title",
			content: "Test Content",
			aiSummary: "Test Summary",
			ownerSubject: userSubject);
		relatedNote.UpdatedAt = updatedAt;

		var relatedNoteIds = new List<ObjectId> { relatedNote.Id };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { relatedNote }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.RelatedNotes.Should().HaveCount(1);
		var item = response.RelatedNotes[0];
		item.Id.Should().Be(relatedNote.Id);
		item.Title.Should().Be("Test Title");
		item.AiSummary.Should().Be("Test Summary");
		item.UpdatedAt.Should().Be(updatedAt);
	}

	[Fact]
	public async Task Handle_WithNullAiSummary_ShouldHandleCorrectly()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var relatedNote = TestDataBuilder.CreateNote(
			title: "Note Without Summary",
			content: "Content",
			aiSummary: null,
			ownerSubject: userSubject);

		var relatedNoteIds = new List<ObjectId> { relatedNote.Id };

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(relatedNoteIds);

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { relatedNote }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.RelatedNotes.Should().HaveCount(1);
		response.RelatedNotes[0].AiSummary.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithEmptyUserSubject_ShouldStillProcess()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = string.Empty;

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				userSubject,
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		await _aiService.Received(1).FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			userSubject,
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithEmptyEmbedding_ShouldReturnEmptyResponse()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var userSubject = "auth0|test123";
		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = Array.Empty<float>();

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Array.Empty<float>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		// Should still call AI service even with empty embedding
		await _aiService.Received(1).FindRelatedNotesAsync(
			Array.Empty<float>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			Arg.Any<int>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithZeroTopN_ShouldPassZeroToAiService()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject,
			TopN = 0
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				0,
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _aiService.Received(1).FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			0,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithLargeTopN_ShouldPassToAiService()
	{
		// Arrange
		var noteId = ObjectId.GenerateNewId();
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var userSubject = "auth0|test123";

		var currentNote = TestDataBuilder.CreateNote(ownerSubject: userSubject);
		currentNote.Id = noteId;
		currentNote.Embedding = embedding;

		var query = new GetRelatedNotesQuery
		{
			NoteId = noteId,
			UserSubject = userSubject,
			TopN = 100
		};

		_repository.GetNoteByIdAsync(noteId)
			.Returns(Result.Ok(currentNote));

		_aiService.FindRelatedNotesAsync(
				Arg.Any<float[]>(),
				Arg.Any<string>(),
				Arg.Any<ObjectId>(),
				100,
				Arg.Any<CancellationToken>())
			.Returns(new List<ObjectId>());

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _aiService.Received(1).FindRelatedNotesAsync(
			Arg.Any<float[]>(),
			Arg.Any<string>(),
			Arg.Any<ObjectId>(),
			100,
			Arg.Any<CancellationToken>());
	}
}
