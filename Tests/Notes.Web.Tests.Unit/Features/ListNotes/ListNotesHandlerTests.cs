using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.ListNotes;

[ExcludeFromCodeCoverage]
public class ListNotesHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly ListNotesHandler _handler;

	public ListNotesHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_handler = new ListNotesHandler(_repository);
	}

	[Fact]
	public async Task Handle_ShouldReturnPaginatedNotes()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", ownerSubject: userSubject),
			TestDataBuilder.CreateNote(title: "Note 2", ownerSubject: userSubject)
		};

		var query = new ListNotesQuery
		{
			UserSubject = userSubject,
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok(notes.Count)));

		_repository.GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>>(notes)));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Notes.Should().HaveCount(2);
		response.TotalCount.Should().Be(2);
		response.PageNumber.Should().Be(1);
		response.PageSize.Should().Be(10);
	}

	[Fact]
	public async Task Handle_WithNoNotes_ShouldReturnEmptyList()
	{
		// Arrange
		var query = new ListNotesQuery
		{
			UserSubject = "auth0|test123",
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok(0)));

		_repository.GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>>(new List<Note>())));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task Handle_ShouldCalculateCorrectSkipForPagination()
	{
		// Arrange
		var query = new ListNotesQuery
		{
			UserSubject = "auth0|test123",
			PageNumber = 3,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok(50)));

		_repository.GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>>(new List<Note>())));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			20, // (3 - 1) * 10
			10
		);
	}

	[Fact]
	public async Task Handle_WhenCountFails_ShouldReturnZeroTotalCount()
	{
		// Arrange
		var query = new ListNotesQuery
		{
			UserSubject = "auth0|test123",
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Fail<int>("Error")));

		_repository.GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>>(new List<Note>())));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task Handle_ShouldMapNotePropertiesCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var note = TestDataBuilder.CreateNote(
			title: "Test Title",
			aiSummary: "Test Summary",
			ownerSubject: userSubject
		);

		var query = new ListNotesQuery
		{
			UserSubject = userSubject,
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok(1)));

		_repository.GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>>(new List<Note> { note })));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		var noteItem = response.Notes.First();
		noteItem.Id.Should().Be(note.Id);
		noteItem.Title.Should().Be(note.Title);
		noteItem.AiSummary.Should().Be(note.AiSummary);
		noteItem.CreatedAt.Should().Be(note.CreatedAt);
		noteItem.UpdatedAt.Should().Be(note.UpdatedAt);
	}
}
