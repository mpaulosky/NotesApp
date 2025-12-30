// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbContextIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Integration.Fixtures;

namespace Notes.Web.Tests.Integration.Data;

/// <summary>
/// Integration tests for MongoDbContext using MongoDB TestContainers.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbContextIntegrationTests : IClassFixture<MongoDbFixture>
{
	private readonly MongoDbFixture _fixture;

	public MongoDbContextIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public void CreateDbContext_ReturnsValidContext()
	{
		// Act
		var context = _fixture.CreateContext();

		// Assert
		context.Should().NotBeNull();
		context.Notes.Should().NotBeNull();
		context.Database.Should().NotBeNull();
	}

	[Fact]
	public async Task Notes_Collection_CanInsertAndRetrieve()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		// Act
		await context.Notes.InsertOneAsync(note);
		var retrieved = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();

		// Assert
		retrieved.Should().NotBeNull();
		retrieved!.Title.Should().Be(note.Title);
		retrieved.Content.Should().Be(note.Content);
	}

	[Fact]
	public async Task Database_Property_AllowsDirectAccess()
	{
		// Arrange
		var context = _fixture.CreateContext();

		// Act
		var collectionNames = await context.Database.ListCollectionNamesAsync();
		var names = await collectionNames.ToListAsync();

		// Assert
		names.Should().NotBeNull();
		// The collection might not exist yet if no data has been inserted
	}

	[Fact]
	public async Task Notes_Collection_SupportsComplexQueries()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "First Note",
				Content = "Content 1",
				OwnerSubject = "user-1",
				Tags = "tag1, tag2",
				CreatedAt = DateTime.UtcNow.AddDays(-2),
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Second Note",
				Content = "Content 2",
				OwnerSubject = "user-1",
				Tags = "tag2, tag3",
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Third Note",
				Content = "Content 3",
				OwnerSubject = "user-2",
				Tags = "tag1",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await context.Notes.InsertManyAsync(notes);

		// Act - Complex query: user-1's notes sorted by creation date descending
		var results = await context.Notes
			.Find(n => n.OwnerSubject == "user-1")
			.SortByDescending(n => n.CreatedAt)
			.ToListAsync();

		// Assert
		results.Should().HaveCount(2);
		results[0].Title.Should().Be("Second Note");
		results[1].Title.Should().Be("First Note");
	}

	[Fact]
	public async Task Notes_Collection_SupportsTextSearch()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "MongoDB Integration",
				Content = "This is about MongoDB integration testing",
				OwnerSubject = "user-1",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Unit Testing",
				Content = "This is about unit testing in .NET",
				OwnerSubject = "user-1",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await context.Notes.InsertManyAsync(notes);

		// Act - Search by title containing "MongoDB"
		var results = await context.Notes
			.Find(n => n.Title.Contains("MongoDB"))
			.ToListAsync();

		// Assert
		results.Should().HaveCount(1);
		results[0].Title.Should().Be("MongoDB Integration");
	}

	[Fact]
	public async Task Notes_Collection_SupportsUpdate()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original Title",
			Content = "Original Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await context.Notes.InsertOneAsync(note);

		// Act
		var update = Builders<Note>.Update
			.Set(n => n.Title, "Updated Title")
			.Set(n => n.UpdatedAt, DateTime.UtcNow);

		await context.Notes.UpdateOneAsync(n => n.Id == note.Id, update);

		var updated = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();

		// Assert
		updated.Should().NotBeNull();
		updated!.Title.Should().Be("Updated Title");
		updated.Content.Should().Be("Original Content");
	}

	[Fact]
	public async Task Notes_Collection_SupportsDelete()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = "To Delete",
			Content = "Content",
			OwnerSubject = "test-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await context.Notes.InsertOneAsync(note);

		// Act
		await context.Notes.DeleteOneAsync(n => n.Id == note.Id);
		var deleted = await context.Notes.Find(n => n.Id == note.Id).FirstOrDefaultAsync();

		// Assert
		deleted.Should().BeNull();
	}

	[Fact]
	public async Task Notes_Collection_SupportsBulkOperations()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var notes = Enumerable.Range(1, 100).Select(i => new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = $"Note {i}",
			Content = $"Content {i}",
			OwnerSubject = "bulk-user",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		}).ToArray();

		// Act
		await context.Notes.InsertManyAsync(notes);
		var count = await context.Notes.CountDocumentsAsync(n => n.OwnerSubject == "bulk-user");

		// Assert
		count.Should().Be(100);
	}

	[Fact]
	public async Task Notes_Collection_SupportsAggregation()
	{
		// Arrange
		await _fixture.ClearNotesAsync();

		var context = _fixture.CreateContext();
		var notes = new[]
		{
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 1",
				Content = "Content",
				OwnerSubject = "user-1",
				IsArchived = false,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 2",
				Content = "Content",
				OwnerSubject = "user-1",
				IsArchived = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = ObjectId.GenerateNewId(),
				Title = "Note 3",
				Content = "Content",
				OwnerSubject = "user-2",
				IsArchived = false,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		await context.Notes.InsertManyAsync(notes);

		// Act - Count notes per user
		var pipeline = context.Notes.Aggregate()
			.Group(
				n => n.OwnerSubject,
				g => new { Owner = g.Key, Count = g.Count() }
			)
			.SortByDescending(x => x.Count);

		var results = await pipeline.ToListAsync();

		// Assert
		results.Should().HaveCount(2);
		results[0].Owner.Should().Be("user-1");
		results[0].Count.Should().Be(2);
		results[1].Owner.Should().Be("user-2");
		results[1].Count.Should().Be(1);
	}
}
