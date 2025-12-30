using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.GetNoteDetails;

[ExcludeFromCodeCoverage]
public class GetNoteDetailsHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly GetNoteDetailsHandler _handler;

	public GetNoteDetailsHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_handler = new GetNoteDetailsHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithValidNote_ShouldReturnNoteDetails()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(
			title: "Test Title",
			content: "Test Content",
			aiSummary: "Test Summary",
			tags: "test,tags",
			ownerSubject: "auth0|test123"
		);

		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(query.Id)
			.Returns(Task.FromResult(Result.Ok(note)));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeTrue();
		response.Note.Should().NotBeNull();
		response.Note!.Id.Should().Be(note.Id);
		response.Note.Title.Should().Be(note.Title);
		response.Note.Content.Should().Be(note.Content);
		response.Note.AiSummary.Should().Be(note.AiSummary);
		response.Note.Tags.Should().Be(note.Tags);
		response.Note.IsArchived.Should().Be(note.IsArchived);
		response.Note.CreatedAt.Should().Be(note.CreatedAt);
		response.Note.UpdatedAt.Should().Be(note.UpdatedAt);
	}

	[Fact]
	public async Task Handle_WhenNoteNotFound_ShouldReturnNull()
	{
		// Arrange
		var query = new GetNoteDetailsQuery
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(query.Id)
			.Returns(Task.FromResult(Result.Fail<Note>("Note not found")));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeFalse();
		response.Note.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WhenUserNotOwner_ShouldReturnNull()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(ownerSubject: "auth0|owner123");
		var query = new GetNoteDetailsQuery
		{
			Id = note.Id,
			UserSubject = "auth0|different456"
		};

		_repository.GetNoteByIdAsync(query.Id)
			.Returns(Task.FromResult(Result.Ok(note)));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeFalse();
		response.Note.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WhenResultValueIsNull_ShouldReturnNull()
	{
		// Arrange
		var query = new GetNoteDetailsQuery
		{
			Id = ObjectId.GenerateNewId(),
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(query.Id)
			.Returns(Task.FromResult(Result.Ok<Note>(null!)));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response!.Success.Should().BeFalse();
		response.Note.Should().BeNull();
	}

	[Fact]
	public async Task Handle_ShouldCallRepositoryWithCorrectId()
	{
		// Arrange
		var expectedId = ObjectId.GenerateNewId();
		var query = new GetNoteDetailsQuery
		{
			Id = expectedId,
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteByIdAsync(expectedId)
			.Returns(Task.FromResult(Result.Fail<Note>("Not found")));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetNoteByIdAsync(expectedId);
	}
}
