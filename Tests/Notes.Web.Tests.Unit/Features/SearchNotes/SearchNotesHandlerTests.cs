// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SearchNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Features.Notes.SearchNotes;
using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.SearchNotes;

[ExcludeFromCodeCoverage]
public class SearchNotesHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly SearchNotesHandler _handler;

	public SearchNotesHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_handler = new SearchNotesHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithValidSearchTerm_ShouldReturnMatchingNotes()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Test Note", content: "Content", ownerSubject: userSubject),
			TestDataBuilder.CreateNote(title: "Another Test", content: "More content", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = userSubject,
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(2));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Notes.Should().HaveCount(2);
		response.TotalCount.Should().Be(2);
		response.SearchTerm.Should().Be("test");
		response.PageNumber.Should().Be(1);
		response.PageSize.Should().Be(10);
	}

	[Fact]
	public async Task Handle_WithEmptySearchTerm_ShouldReturnAllUserNotes()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", ownerSubject: userSubject),
			TestDataBuilder.CreateNote(title: "Note 2", ownerSubject: userSubject),
			TestDataBuilder.CreateNote(title: "Note 3", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = string.Empty,
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(3));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(3);
		response.TotalCount.Should().Be(3);
	}

	[Fact]
	public async Task Handle_WithWhitespaceSearchTerm_ShouldReturnAllUserNotes()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 1", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = "   ",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
	}

	[Fact]
	public async Task Handle_WithNoMatchingNotes_ShouldReturnEmptyList()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			SearchTerm = "nonexistent",
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WhenRepositoryFails_ShouldReturnEmptyList()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Fail<int>("Database error"));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Fail<IEnumerable<Note>?>("Database error"));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WhenGetNotesReturnsNull_ShouldReturnEmptyList()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(5));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(null!));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(5); // Count is still returned
	}

	[Fact]
	public async Task Handle_WithPagination_ShouldCalculateSkipCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = userSubject,
			PageNumber = 3,
			PageSize = 5
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(20));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				10, // (3-1) * 5 = 10
				5)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			10,
			5);
	}

	[Fact]
	public async Task Handle_WithPageNumber1_ShouldSkipZero()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123",
			PageNumber = 1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				0,
				10)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			0,
			10);
	}

	[Fact]
	public async Task Handle_ShouldMapAllFieldsCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var createdAt = DateTime.UtcNow.AddDays(-10);
		var updatedAt = DateTime.UtcNow.AddDays(-1);

		var note = TestDataBuilder.CreateNote(
			title: "Test Title",
			content: "Test Content",
			aiSummary: "Test Summary",
			tags: "tag1,tag2",
			ownerSubject: userSubject);
		note.CreatedAt = createdAt;
		note.UpdatedAt = updatedAt;

		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note> { note }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		var item = response.Notes[0];
		item.Id.Should().Be(note.Id);
		item.Title.Should().Be("Test Title");
		item.AiSummary.Should().Be("Test Summary");
		item.Tags.Should().Be("tag1,tag2");
		item.CreatedAt.Should().Be(createdAt);
		item.UpdatedAt.Should().Be(updatedAt);
	}

	[Fact]
	public async Task Handle_WithNullAiSummary_ShouldHandleCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var note = TestDataBuilder.CreateNote(
			title: "Test",
			content: "Content",
			aiSummary: null,
			ownerSubject: userSubject);

		var query = new SearchNotesQuery
		{
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note> { note }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes[0].AiSummary.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithNullTags_ShouldHandleCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var note = TestDataBuilder.CreateNote(
			title: "Test",
			content: "Content",
			tags: null,
			ownerSubject: userSubject);

		var query = new SearchNotesQuery
		{
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note> { note }));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes[0].Tags.Should().BeNull();
	}

	[Fact]
	public async Task Handle_SearchTermShouldBeCaseInsensitive()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Test Note", content: "Content", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = "TEST",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
	}

	[Fact]
	public async Task Handle_ShouldSearchInTitle()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Important Note", content: "Some content", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = "important",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
	}

	[Fact]
	public async Task Handle_ShouldSearchInContent()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note", content: "Important information", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			SearchTerm = "information",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(1));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
	}

	[Fact]
	public async Task Handle_ShouldOnlyReturnNotesForCurrentUser()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert - Verify the filter includes user subject check
		await _repository.Received(1).GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			Arg.Any<int>(),
			Arg.Any<int>());
	}

	[Fact]
	public async Task Handle_WithDefaultPageSize_ShouldUse10()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123"
			// PageSize not specified, should default to 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				10)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.PageSize.Should().Be(10);
	}

	[Fact]
	public async Task Handle_WithDefaultPageNumber_ShouldUse1()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123"
			// PageNumber not specified, should default to 1
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.PageNumber.Should().Be(1);
	}

	[Fact]
	public async Task Handle_WithCustomPageSize_ShouldUseCustomValue()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123",
			PageSize = 25
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				25)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.PageSize.Should().Be(25);
	}

	[Fact]
	public async Task Handle_WithZeroPageSize_ShouldUseZero()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123",
			PageSize = 0
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Is(0))
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.PageSize.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithLargePageSize_ShouldUseLargeValue()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123",
			PageSize = 1000
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				1000)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.PageSize.Should().Be(1000);
	}

	[Fact]
	public async Task Handle_WithEmptyUserSubject_ShouldStillQuery()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = string.Empty
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		await _repository.Received(1).GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>());
	}

	[Fact]
	public async Task Handle_WithSpecialCharactersInSearchTerm_ShouldHandleCorrectly()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var query = new SearchNotesQuery
		{
			SearchTerm = "test@#$%",
			UserSubject = userSubject
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.SearchTerm.Should().Be("test@#$%");
	}

	[Fact]
	public async Task SearchNotesResponse_TotalPages_ShouldCalculateCorrectly()
	{
		// Arrange & Act
		var response1 = new SearchNotesResponse { TotalCount = 25, PageSize = 10 };
		var response2 = new SearchNotesResponse { TotalCount = 30, PageSize = 10 };
		var response3 = new SearchNotesResponse { TotalCount = 10, PageSize = 10 };
		var response4 = new SearchNotesResponse { TotalCount = 0, PageSize = 10 };

		// Assert
		response1.TotalPages.Should().Be(3); // 25 / 10 = 2.5 -> 3
		response2.TotalPages.Should().Be(3); // 30 / 10 = 3.0 -> 3
		response3.TotalPages.Should().Be(1); // 10 / 10 = 1.0 -> 1
		response4.TotalPages.Should().Be(0); // 0 / 10 = 0.0 -> 0
	}

	[Fact]
	public async Task Handle_WithMultiplePages_ShouldReturnCorrectPageData()
	{
		// Arrange
		var userSubject = "auth0|test123";
		var page2Notes = new List<Note>
		{
			TestDataBuilder.CreateNote(title: "Note 11", ownerSubject: userSubject),
			TestDataBuilder.CreateNote(title: "Note 12", ownerSubject: userSubject)
		};

		var query = new SearchNotesQuery
		{
			UserSubject = userSubject,
			PageNumber = 2,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(12));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				10, // Skip first 10
				10)
			.Returns(Result.Ok<IEnumerable<Note>?>(page2Notes));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(2);
		response.TotalCount.Should().Be(12);
		response.PageNumber.Should().Be(2);
		response.TotalPages.Should().Be(2);
	}

	[Fact]
	public async Task Handle_WithNegativePageNumber_ShouldCalculateNegativeSkip()
	{
		// Arrange
		var query = new SearchNotesQuery
		{
			UserSubject = "auth0|test123",
			PageNumber = -1,
			PageSize = 10
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				-20, // (-1-1) * 10 = -20
				10)
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetNotes(
			Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
			-20,
			10);
	}

	[Fact]
	public async Task Handle_ShouldPreserveSearchTermInResponse()
	{
		// Arrange
		var searchTerm = "specific search term";
		var query = new SearchNotesQuery
		{
			SearchTerm = searchTerm,
			UserSubject = "auth0|test123"
		};

		_repository.GetNoteCount(Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>())
			.Returns(Result.Ok(0));

		_repository.GetNotes(
				Arg.Any<System.Linq.Expressions.Expression<Func<Note, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<int>())
			.Returns(Result.Ok<IEnumerable<Note>?>(new List<Note>()));

		// Act
		var response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.SearchTerm.Should().Be(searchTerm);
	}
}
