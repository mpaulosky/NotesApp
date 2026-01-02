using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.DeleteNote;

[ExcludeFromCodeCoverage]
public class DeleteNoteHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly DeleteNoteHandler _handler;

	public DeleteNoteHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_handler = new DeleteNoteHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithValidNote_ShouldDeleteNote()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|test123");
		var command = new DeleteNoteCommand
		{
			Id = existingNote.Id,
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult<Result<Note?>>(Result.Ok<Note?>(existingNote)));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Message.Should().Be("Note archived successfully.");
	}

	[Fact]
	public async Task Handle_WhenNoteNotFound_ShouldReturnFailure()
	{
		// Arrange
		var command = new DeleteNoteCommand
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(command.Id)
		.Returns(Task.FromResult<Result<Note?>>(Result.Fail<Note?>("Note not found")));
		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
		await _repository.DidNotReceive().ArchiveNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_WhenUserNotOwner_ShouldReturnFailure()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|owner123");
		var command = new DeleteNoteCommand
		{
			Id = existingNote.Id,
			UserSubject = "auth0|different456"
		};

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult<Result<Note?>>(Result.Ok<Note?>(existingNote)));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
		response.Message.Should().Be("Note not found or access denied.");
		await _repository.DidNotReceive().ArchiveNote(Arg.Any<Note>());
	}

	[Fact]
	public async Task Handle_ShouldCallRepositoryDelete()
	{
		// Arrange
		var existingNote = TestDataBuilder.CreateNote(ownerSubject: "auth0|test123");
		var command = new DeleteNoteCommand
		{
			Id = existingNote.Id,
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(command.Id)
			.Returns(Task.FromResult<Result<Note?>>(Result.Ok<Note?>(existingNote)));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).ArchiveNote(Arg.Is<Note>(n => n.Id == existingNote.Id));
	}

	[Fact]
	public async Task Handle_WhenResultValueIsNull_ShouldReturnFailure()
	{
		// Arrange
		var command = new DeleteNoteCommand
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(command.Id)
		.Returns(Task.FromResult<Result<Note?>>(Result.Ok<Note?>(null!)));
		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Success.Should().BeFalse();
		response.Message.Should().Be("Note not found or access denied.");
		await _repository.DidNotReceive().ArchiveNote(Arg.Any<Note>());
	}
}
