using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.UpdateNote;

[ExcludeFromCodeCoverage]
public class UpdateNoteHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly UpdateNoteHandler _handler;

	public UpdateNoteHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_aiService = Substitute.For<IAiService>();
		_handler = new UpdateNoteHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_WithValidNote_ShouldUpdateNoteWithAiData()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(
			title: "Old Title",
			content: "Old Content",
			ownerSubject: "auth0|test123"
		);

		var command = TestDataBuilder.CreateUpdateNoteCommand(
			noteId: existingNote.Id,
			title: "New Title",
			content: "New Content",
			userSubject: "auth0|test123"
		);

		var expectedSummary = "Updated Summary";
		var expectedTags = "updated,tags";
		var expectedEmbedding = new[] { 0.4f, 0.5f, 0.6f };

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Ok<Note?>(existingNote)));

		_aiService.GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedSummary));
		_aiService.GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedTags));
		_aiService.GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedEmbedding));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Title.Should().Be(command.Title);
		response.Content.Should().Be(command.Content);
		response.AiSummary.Should().Be(expectedSummary);
		response.Tags.Should().Be(expectedTags);
		response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Handle_WhenNoteNotFound_ShouldReturnFailure()
	{
		// Arrange
		var command = TestDataBuilder.CreateUpdateNoteCommand();

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Fail<Note?>("Note not found")));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
		await _repository.DidNotReceive().UpdateNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_WhenUserNotOwner_ShouldReturnFailure()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|owner123");
		var command = TestDataBuilder.CreateUpdateNoteCommand(
			noteId: existingNote.Id,
			userSubject: "auth0|different456"
		);

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Ok<Note?>(existingNote)));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
		await _repository.DidNotReceive().UpdateNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_ShouldCallUpdateNoteInRepository()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|test123");
		var command = TestDataBuilder.CreateUpdateNoteCommand(
			noteId: existingNote.Id,
			userSubject: "auth0|test123"
		);

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Ok<Note?>(existingNote)));

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).UpdateNote(Arg.Is<Note>(n =>
			n.Id == existingNote.Id &&
			n.Title == command.Title &&
			n.Content == command.Content
		));
	}

	[Fact]
	public async Task Handle_ShouldRegenerateAllAiData()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|test123");
		var command = TestDataBuilder.CreateUpdateNoteCommand(
			noteId: existingNote.Id,
			userSubject: "auth0|test123"
		);

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Ok<Note?>(existingNote)));

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldUpdateTimestamp()
	{
		// Arrange
		var oldDate = DateTime.UtcNow.AddDays(-5);
		var existingNote = TestDataBuilder.CreateNote(
			ownerSubject: "auth0|test123",
			createdAt: oldDate,
			updatedAt: oldDate
		);

		var command = TestDataBuilder.CreateUpdateNoteCommand(
			noteId: existingNote.Id,
			userSubject: "auth0|test123"
		);

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult(Result.Ok<Note?>(existingNote)));

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.UpdatedAt.Should().BeAfter(oldDate);
		response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}
}
