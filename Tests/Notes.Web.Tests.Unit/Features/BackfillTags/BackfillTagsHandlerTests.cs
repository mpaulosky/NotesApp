// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BackfillTagsHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Features.Notes.BackfillTags;
using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.BackfillTags;

[ExcludeFromCodeCoverage]
public class BackfillTagsHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly BackfillTagsHandler _handler;

	public BackfillTagsHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_aiService = Substitute.For<IAiService>();
		_handler = new BackfillTagsHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_WithValidNotes_ShouldBackfillTags()
	{
		// Arrange
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Note 2", content: "Content 2", tags: "", ownerSubject: "auth0|test123")
		};

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123",
			OnlyMissing = true
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2,tag3");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.ProcessedCount.Should().Be(2);
		response.TotalNotes.Should().Be(2);
		response.Errors.Should().BeEmpty();

		await _repository.Received(2).UpdateNote(Arg.Any<Note>());
		await _aiService.Received(2).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithOnlyMissingTrue_ShouldOnlyProcessNotesWithoutTags()
	{
		// Arrange
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Note 2", content: "Content 2", tags: "existing,tags", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Note 3", content: "Content 3", tags: null!, ownerSubject: "auth0|test123")
		};

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123",
			OnlyMissing = true
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(2); // Only notes without tags
		response.TotalNotes.Should().Be(2);
		await _aiService.Received(2).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithOnlyMissingFalse_ShouldProcessAllNotes()
	{
		// Arrange
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Note 2", content: "Content 2", tags: "existing,tags", ownerSubject: "auth0|test123")
		};

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123",
			OnlyMissing = false
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("new,tags");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(2); // All notes processed
		response.TotalNotes.Should().Be(2);
	}

	[Fact]
	public async Task Handle_WhenRepositoryFails_ShouldReturnError()
	{
		// Arrange
		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Fail<IEnumerable<Note>>("Database connection failed"));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.ProcessedCount.Should().Be(0);
		response.TotalNotes.Should().Be(0);
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Be("Database connection failed");
	}

	[Fact]
	public async Task Handle_WhenRepositoryReturnsNull_ShouldReturnError()
	{
		// Arrange
		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(null!));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Be("Unknown error");
	}

	[Fact]
	public async Task Handle_WhenNoNotesFound_ShouldReturnZeroCounts()
	{
		// Arrange
		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note>()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(0);
		response.TotalNotes.Should().Be(0);
		response.Errors.Should().BeEmpty();
		await _aiService.DidNotReceive().GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenAiServiceFails_ShouldRecordError()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<string>(new Exception("AI service timeout")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(0);
		response.TotalNotes.Should().Be(1);
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Contain("Failed to generate tags for note 'Note 1'");
		response.Errors[0].Should().Contain("AI service timeout");
	}

	[Fact]
	public async Task Handle_WhenUpdateNoteFails_ShouldRecordError()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Fail<Note>("Update failed"));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(0);
		response.TotalNotes.Should().Be(1);
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Contain("Failed to update note 'Note 1'");
		response.Errors[0].Should().Contain("Update failed");
	}

	[Fact]
	public async Task Handle_WithMixedSuccessAndFailure_ShouldTrackBoth()
	{
		// Arrange
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Success Note", content: "Content 1", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Fail Note", content: "Content 2", tags: "", ownerSubject: "auth0|test123")
		};

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2");

		_repository.UpdateNote(Arg.Is<Note>(n => n.Title == "Success Note"))
			.Returns(Result.Ok(new Note()));

		_repository.UpdateNote(Arg.Is<Note>(n => n.Title == "Fail Note"))
			.Returns(Result.Fail<Note>("Database error"));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(1);
		response.TotalNotes.Should().Be(2);
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Contain("Failed to update note 'Fail Note'");
	}

	[Fact]
	public async Task Handle_ShouldUpdateNoteUpdatedAtTimestamp()
	{
		// Arrange
		var originalTime = DateTime.UtcNow.AddDays(-1);
		var note = TestDataBuilder.CreateNote(
			title: "Note 1",
			content: "Content 1",
			tags: "",
			ownerSubject: "auth0|test123");
		note.UpdatedAt = originalTime;

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2");

		Note? capturedNote = null;
		_repository.UpdateNote(Arg.Do<Note>(n => capturedNote = n))
			.Returns(Result.Ok(new Note()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		capturedNote.Should().NotBeNull();
		capturedNote!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
		capturedNote.UpdatedAt.Should().BeAfter(originalTime);
	}

	[Fact]
	public async Task Handle_ShouldSetGeneratedTagsOnNote()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync("Note 1", "Content 1", Arg.Any<CancellationToken>())
			.Returns("generated,ai,tags");

		Note? capturedNote = null;
		_repository.UpdateNote(Arg.Do<Note>(n => capturedNote = n))
			.Returns(Result.Ok(new Note()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		capturedNote.Should().NotBeNull();
		capturedNote!.Tags.Should().Be("generated,ai,tags");
	}

	[Fact]
	public async Task Handle_ShouldPassCorrectParametersToAiService()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(
			title: "Test Title",
			content: "Test Content",
			tags: "",
			ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tags");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateTagsAsync("Test Title", "Test Content", Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithEmptyUserSubject_ShouldStillQuery()
	{
		// Arrange
		var command = new BackfillTagsCommand
		{
			UserSubject = string.Empty
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note>()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		await _repository.Received(1).GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>());
	}

	[Fact]
	public async Task Handle_WithLargeNumberOfNotes_ShouldProcessAll()
	{
		// Arrange
		var notes = Enumerable.Range(1, 100)
			.Select(i => TestDataBuilder.CreateNote(
				title: $"Note {i}",
				content: $"Content {i}",
				tags: "",
				ownerSubject: "auth0|test123"))
			.ToList();

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(100);
		response.TotalNotes.Should().Be(100);
		await _aiService.Received(100).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenMultipleNotesFail_ShouldRecordAllErrors()
	{
		// Arrange
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Fail Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Fail Note 2", content: "Content 2", tags: "", ownerSubject: "auth0|test123"),
			TestDataBuilder.CreateNote(title: "Fail Note 3", content: "Content 3", tags: "", ownerSubject: "auth0|test123")
		};

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(notes));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<string>(new Exception("AI Error")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(0);
		response.TotalNotes.Should().Be(3);
		response.Errors.Should().HaveCount(3);
	}

	[Fact]
	public async Task Handle_WithCancellationToken_ShouldPassToAiService()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123");
		var cts = new CancellationTokenSource();
		var cancellationToken = cts.Token;

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken)
			.Returns("tags");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _aiService.Received(1).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken);
	}

	[Fact]
	public async Task Handle_WithNullTags_ShouldTreatAsEmpty()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: null!, ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123",
			OnlyMissing = true
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("new,tags");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Ok(new Note()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.ProcessedCount.Should().Be(1);
		response.TotalNotes.Should().Be(1);
	}

	[Fact]
	public async Task Handle_WhenUpdateReturnsNullError_ShouldUseUnknownErrorMessage()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(title: "Note 1", content: "Content 1", tags: "", ownerSubject: "auth0|test123");

		var command = new BackfillTagsCommand
		{
			UserSubject = "auth0|test123"
		};

		_repository.GetNotes(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok<IEnumerable<Note>>(new List<Note> { note }));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tags");

		_repository.UpdateNote(Arg.Any<Note>())
			.Returns(Result.Fail<Note>(null!));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Errors.Should().ContainSingle();
		response.Errors[0].Should().Contain("Unknown error");
	}
}
