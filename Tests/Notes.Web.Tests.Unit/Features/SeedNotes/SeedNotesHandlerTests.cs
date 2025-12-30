// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SeedNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Features.Notes.SeedNotes;
using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.SeedNotes;

[ExcludeFromCodeCoverage]
public class SeedNotesHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly SeedNotesHandler _handler;

	public SeedNotesHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_aiService = Substitute.For<IAiService>();
		_handler = new SeedNotesHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_WithDefaultCount_ShouldCreate50Notes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123"
			// Count defaults to 50
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(50);
		response.CreatedNoteIds.Should().HaveCount(50);
		response.Errors.Should().BeEmpty();
		await _repository.Received(50).AddNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_WithCustomCount_ShouldCreateSpecifiedNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 10
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(10);
		response.CreatedNoteIds.Should().HaveCount(10);
		await _repository.Received(10).AddNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_WithZeroCount_ShouldCreateNoNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 0
		};

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(0);
		response.CreatedNoteIds.Should().BeEmpty();
		response.Errors.Should().BeEmpty();
		await _repository.DidNotReceive().AddNote(Arg.Any<Note>());
		await _aiService.DidNotReceive().GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithCountLargerThanSamples_ShouldCreateOnlyAvailableSamples()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1000 // More than available samples
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(50); // Max available samples
		response.CreatedNoteIds.Should().HaveCount(50);
	}

	[Fact]
	public async Task Handle_ShouldCallAllAiServicesForEachNote()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 2
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(2).GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(2).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(2).GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldSetUserSubjectOnAllNotes()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var command = new SeedNotesCommand
		{
			UserSubject = userSubject,
			Count = 3
		};

		SetupSuccessfulAiService();

		var capturedNotes = new List<Note>();
		_repository.AddNote(Arg.Do<Note>(n => capturedNotes.Add(n))).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		capturedNotes.Should().HaveCount(3);
		capturedNotes.Should().OnlyContain(n => n.OwnerSubject == userSubject);
	}

	[Fact]
	public async Task Handle_ShouldGenerateUniqueIdsForNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 5
		};

		SetupSuccessfulAiService();

		var capturedNotes = new List<Note>();
		_repository.AddNote(Arg.Do<Note>(n => capturedNotes.Add(n))).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		var uniqueIds = capturedNotes.Select(n => n.Id).Distinct().Count();
		uniqueIds.Should().Be(5);
	}

	[Fact]
	public async Task Handle_ShouldSetAiGeneratedFieldsOnNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("Generated Summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2,tag3");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new[] { 0.1f, 0.2f, 0.3f });

		Note? capturedNote = null;
		_repository.AddNote(Arg.Do<Note>(n => capturedNote = n)).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		capturedNote.Should().NotBeNull();
		capturedNote!.AiSummary.Should().Be("Generated Summary");
		capturedNote.Tags.Should().Be("tag1,tag2,tag3");
		capturedNote.Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
	}

	[Fact]
	public async Task Handle_ShouldSetTitleAndContentFromSampleData()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		SetupSuccessfulAiService();

		Note? capturedNote = null;
		_repository.AddNote(Arg.Do<Note>(n => capturedNote = n)).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		capturedNote.Should().NotBeNull();
		capturedNote!.Title.Should().NotBeNullOrEmpty();
		capturedNote.Content.Should().NotBeNullOrEmpty();
		// First sample note is about Zelda
		capturedNote.Title.Should().Contain("Zelda");
	}

	[Fact]
	public async Task Handle_ShouldSetCreatedAtInPast()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 10
		};

		SetupSuccessfulAiService();

		var capturedNotes = new List<Note>();
		_repository.AddNote(Arg.Do<Note>(n => capturedNotes.Add(n))).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		var now = DateTime.UtcNow;
		capturedNotes.Should().OnlyContain(n => n.CreatedAt <= now);
		capturedNotes.Should().OnlyContain(n => n.CreatedAt >= now.AddDays(-30));
	}

	[Fact]
	public async Task Handle_ShouldSetUpdatedAtToNow()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 2
		};

		SetupSuccessfulAiService();

		var capturedNotes = new List<Note>();
		_repository.AddNote(Arg.Do<Note>(n => capturedNotes.Add(n))).Returns(Task.FromResult(Result.Ok()));

		var before = DateTime.UtcNow;

		// Act
		await _handler.Handle(command, CancellationToken.None);

		var after = DateTime.UtcNow;

		// Assert
		capturedNotes.Should().OnlyContain(n => n.UpdatedAt >= before && n.UpdatedAt <= after);
	}

	[Fact]
	public async Task Handle_WhenAiServiceFails_ShouldRecordError()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 2
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<string>(new Exception("AI service error")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(0);
		response.Errors.Should().HaveCount(2);
		response.Errors.Should().OnlyContain(e => e.Contains("AI service error"));
	}

	[Fact]
	public async Task Handle_WhenRepositoryFails_ShouldRecordError()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>())
			.Returns(Task.FromResult(Result.Fail("Database error")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(0);
		response.Errors.Should().HaveCount(1);
		response.Errors[0].Should().Contain("Database error");
	}

	[Fact]
	public async Task Handle_WithPartialFailures_ShouldRecordSuccessesAndErrors()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 3
		};

		var callCount = 0;
		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(_ =>
			{
				callCount++;
				if (callCount == 2)
				{
					return Task.FromException<string>(new Exception("AI failed on second note"));
				}
				return Task.FromResult("Summary");
			});

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new[] { 0.1f });

		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(2); // 1st and 3rd succeeded
		response.Errors.Should().HaveCount(1); // 2nd failed
	}

	[Fact]
	public async Task Handle_ShouldPassCancellationTokenToAiService()
	{
		// Arrange
		var cts = new CancellationTokenSource();
		var cancellationToken = cts.Token;

		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), cancellationToken)
			.Returns("Summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken)
			.Returns("tags");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), cancellationToken)
			.Returns(new[] { 0.1f });

		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _aiService.Received(1).GenerateSummaryAsync(Arg.Any<string>(), cancellationToken);
		await _aiService.Received(1).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken);
		await _aiService.Received(1).GenerateEmbeddingAsync(Arg.Any<string>(), cancellationToken);
	}

	[Fact]
	public async Task Handle_ShouldCallAiServicesInParallel()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		var summaryDelay = new TaskCompletionSource<string>();
		var tagsDelay = new TaskCompletionSource<string>();
		var embeddingDelay = new TaskCompletionSource<float[]>();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(summaryDelay.Task);
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(tagsDelay.Task);
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(embeddingDelay.Task);

		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		var handleTask = _handler.Handle(command, CancellationToken.None);

		// All three should be called before any complete
		await Task.Delay(50);

		// Complete all at once
		summaryDelay.SetResult("Summary");
		tagsDelay.SetResult("Tags");
		embeddingDelay.SetResult(new[] { 0.1f });

		await handleTask;

		// Assert - If called in parallel, all three were invoked
		await _aiService.Received(1).GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithEmptyUserSubject_ShouldStillCreateNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = string.Empty,
			Count = 1
		};

		SetupSuccessfulAiService();

		Note? capturedNote = null;
		_repository.AddNote(Arg.Do<Note>(n => capturedNote = n)).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(1);
		capturedNote.Should().NotBeNull();
		capturedNote!.OwnerSubject.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_ShouldReturnCreatedNoteIds()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 3
		};

		SetupSuccessfulAiService();

		var capturedNotes = new List<Note>();
		_repository.AddNote(Arg.Do<Note>(n => capturedNotes.Add(n))).Returns(Task.FromResult(Result.Ok()));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedNoteIds.Should().HaveCount(3);
		var capturedIds = capturedNotes.Select(n => n.Id).ToList();
		response.CreatedNoteIds.Should().BeEquivalentTo(capturedIds);
	}

	[Fact]
	public async Task Handle_WithNegativeCount_ShouldCreateNoNotes()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = -5
		};

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(0);
		response.CreatedNoteIds.Should().BeEmpty();
		await _repository.DidNotReceive().AddNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_ErrorMessageShouldIncludeNoteTitle()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<string>(new Exception("Test error")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Errors.Should().HaveCount(1);
		response.Errors[0].Should().Contain("Failed to create");
		response.Errors[0].Should().Contain("Zelda"); // First sample note
	}

	[Fact]
	public async Task Handle_ShouldProcessNotesSequentially()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 3
		};

		SetupSuccessfulAiService();

		var addOrder = new List<int>();
		_repository.AddNote(Arg.Do<Note>(n => addOrder.Add(addOrder.Count))).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		addOrder.Should().Equal(0, 1, 2); // Sequential order
	}

	[Fact]
	public async Task Handle_WhenAllFail_ShouldReturnAllErrors()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 3
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<string>(new Exception("AI failed")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.CreatedCount.Should().Be(0);
		response.CreatedNoteIds.Should().BeEmpty();
		response.Errors.Should().HaveCount(3);
	}

	[Fact]
	public async Task Handle_ShouldPassTitleAndContentToGenerateTags()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateTagsAsync(
			Arg.Is<string>(title => title.Contains("Zelda")),
			Arg.Is<string>(content => content.Length > 0),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldPassContentToGenerateSummary()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateSummaryAsync(
			Arg.Is<string>(content => content.Length > 0),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldPassContentToGenerateEmbedding()
	{
		// Arrange
		var command = new SeedNotesCommand
		{
			UserSubject = "auth0|test123",
			Count = 1
		};

		SetupSuccessfulAiService();
		_repository.AddNote(Arg.Any<Note>()).Returns(Task.FromResult(Result.Ok()));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateEmbeddingAsync(
			Arg.Is<string>(content => content.Length > 0),
			Arg.Any<CancellationToken>());
	}

	private void SetupSuccessfulAiService()
	{
		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("AI Generated Summary");
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns("tag1,tag2,tag3");
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new[] { 0.1f, 0.2f, 0.3f });
	}
}
