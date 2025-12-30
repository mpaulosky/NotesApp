// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ListNotesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.ListNotes;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for ListNotesHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class ListNotesHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;
	private readonly IRequestHandler<ListNotesQuery, ListNotesResponse> _handler;

	public ListNotesHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
		_handler = new ListNotesHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithNoNotes_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(0);
		response.PageNumber.Should().Be(1);
		response.PageSize.Should().Be(10);
		response.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithMultipleNotes_ReturnsPaginatedList()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		var notes = await _fixture.CreateTestNotesAsync("test-user", 5);

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 3
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(3);
		response.TotalCount.Should().Be(5);
		response.PageNumber.Should().Be(1);
		response.PageSize.Should().Be(3);
		response.TotalPages.Should().Be(2);
	}

	[Fact]
	public async Task Handle_SecondPage_ReturnsRemainingNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("test-user", 5);

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 2,
			PageSize = 3
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(2); // Remaining notes
		response.TotalCount.Should().Be(5);
		response.PageNumber.Should().Be(2);
		response.TotalPages.Should().Be(2);
	}

	[Fact]
	public async Task Handle_PageBeyondAvailable_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("test-user", 5);

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 10,
			PageSize = 10
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(5);
		response.PageNumber.Should().Be(10);
	}

	[Fact]
	public async Task Handle_FiltersNotesByUserSubject()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("user-a", 3);
		await _fixture.CreateTestNotesAsync("user-b", 2);

		var query = new ListNotesQuery
		{
			UserSubject = "user-a",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(3);
		response.TotalCount.Should().Be(3);
	}

	[Fact]
	public async Task Handle_IncludesNoteListItemFields()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Sample Note",
			Content = "This is the content",
			OwnerSubject = "test-user",
			AiSummary = "AI generated summary",
			Tags = "tag1, tag2",
			CreatedAt = DateTime.UtcNow.AddDays(-2),
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};

		await _fixture.SeedNotesAsync(note);

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		var item = response.Notes[0];
		item.Id.Should().Be(note.Id);
		item.Title.Should().Be("Sample Note");
		item.AiSummary.Should().Be("AI generated summary");
		item.CreatedAt.Should().BeCloseTo(note.CreatedAt, TimeSpan.FromSeconds(1));
		item.UpdatedAt.Should().BeCloseTo(note.UpdatedAt, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task Handle_CalculatesTotalPagesCorrectly()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("test-user", 25);

		var query = new ListNotesQuery
		{
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		ListNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.TotalCount.Should().Be(25);
		response.TotalPages.Should().Be(3); // 25 notes / 10 per page = 3 pages
	}
}
