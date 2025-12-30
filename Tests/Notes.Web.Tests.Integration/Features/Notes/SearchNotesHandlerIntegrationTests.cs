// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SearchNotesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using MediatR;

using Notes.Web.Features.Notes.SearchNotes;
using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Features.Notes;

/// <summary>
/// Integration tests for SearchNotesHandler using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class SearchNotesHandlerIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;
	private readonly INoteRepository _repository;
	private readonly IRequestHandler<SearchNotesQuery, SearchNotesResponse> _handler;

	public SearchNotesHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;

		var contextFactory = Substitute.For<IMongoDbContextFactory>();
		contextFactory.CreateDbContext().Returns(_fixture.CreateContext());

		_repository = new NoteRepository(contextFactory);
		_handler = new SearchNotesHandler(_repository);
	}

	[Fact]
	public async Task Handle_WithEmptySearchTerm_ReturnsAllUserNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("test-user", 5);

		var query = new SearchNotesQuery
		{
			SearchTerm = "",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(5);
		response.TotalCount.Should().Be(5);
		response.SearchTerm.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_SearchByTitle_ReturnsMatchingNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Meeting Notes",
				Content = "Content about something else",
				OwnerSubject = "test-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Shopping List",
				Content = "Buy groceries",
				OwnerSubject = "test-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		var query = new SearchNotesQuery
		{
			SearchTerm = "meeting",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		response.Notes[0].Title.Should().Be("Meeting Notes");
	}

	[Fact]
	public async Task Handle_SearchByContent_ReturnsMatchingNotes()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Random Title",
				Content = "This contains important information",
				OwnerSubject = "test-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Another Note",
				Content = "Just regular content",
				OwnerSubject = "test-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		var query = new SearchNotesQuery
		{
			SearchTerm = "important",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		response.Notes[0].Title.Should().Be("Random Title");
	}

	[Fact]
	public async Task Handle_SearchIsCaseInsensitive()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "UPPERCASE TITLE",
			Content = "lowercase content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _fixture.SeedNotesAsync(note);

		var query = new SearchNotesQuery
		{
			SearchTerm = "uppercase",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
	}

	[Fact]
	public async Task Handle_WithPagination_ReturnsCorrectPage()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		// Create 5 notes with "test" in title
		for (int i = 1; i <= 5; i++)
		{
			await _fixture.SeedNotesAsync(new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = $"Test Note {i}",
				Content = "Content",
				OwnerSubject = "test-user",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			});
		}

		var query = new SearchNotesQuery
		{
			SearchTerm = "test",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 3
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(3);
		response.TotalCount.Should().Be(5);
		response.TotalPages.Should().Be(2);
	}

	[Fact]
	public async Task Handle_FiltersNotesByUserSubject()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "User A Note",
				Content = "search term",
				OwnerSubject = "user-a",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "User B Note",
				Content = "search term",
				OwnerSubject = "user-b",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await _fixture.SeedNotesAsync(notes);

		var query = new SearchNotesQuery
		{
			SearchTerm = "search",
			UserSubject = "user-a",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		response.Notes[0].Title.Should().Be("User A Note");
	}

	[Fact]
	public async Task Handle_IncludesSearchNoteItemFields()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Sample Note",
			Content = "Sample content",
			OwnerSubject = "test-user",
			AiSummary = "AI summary",
			Tags = "tag1, tag2, tag3",
			CreatedAt = DateTime.UtcNow.AddDays(-2),
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};

		await _fixture.SeedNotesAsync(note);

		var query = new SearchNotesQuery
		{
			SearchTerm = "sample",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().HaveCount(1);
		var item = response.Notes[0];
		item.Id.Should().Be(note.Id);
		item.Title.Should().Be("Sample Note");
		item.AiSummary.Should().Be("AI summary");
		item.Tags.Should().Be("tag1, tag2, tag3");
		item.CreatedAt.Should().BeCloseTo(note.CreatedAt, TimeSpan.FromSeconds(1));
		item.UpdatedAt.Should().BeCloseTo(note.UpdatedAt, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task Handle_WithNoMatchingResults_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearNotesAsync();
		await _fixture.CreateTestNotesAsync("test-user", 3);

		var query = new SearchNotesQuery
		{
			SearchTerm = "nonexistentterm",
			UserSubject = "test-user",
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		SearchNotesResponse response = await _handler.Handle(query, CancellationToken.None);

		// Assert
		response.Notes.Should().BeEmpty();
		response.TotalCount.Should().Be(0);
		response.SearchTerm.Should().Be("nonexistentterm");
	}
}
