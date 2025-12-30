// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Integration.Fixtures;

/// <summary>
/// Base fixture for integration tests using MongoDB TestContainers.
/// Manages the lifecycle of a MongoDB container for testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbFixture : IAsyncLifetime
{
	private MongoDbContainer? _mongoDbContainer;

	/// <summary>
	/// Gets the MongoDB connection string from the running container.
	/// </summary>
	public string ConnectionString => _mongoDbContainer?.GetConnectionString()
		?? throw new InvalidOperationException("MongoDB container not initialized");

	/// <summary>
	/// Gets the MongoDB client connected to the test container.
	/// </summary>
	public IMongoClient MongoClient { get; private set; } = null!;

	/// <summary>
	/// Gets the test database name.
	/// </summary>
	public string DatabaseName => "NotesTestDb";

	/// <summary>
	/// Gets the MongoDB database for testing.
	/// </summary>
	public IMongoDatabase Database => MongoClient.GetDatabase(DatabaseName);

	/// <summary>
	/// Initializes the MongoDB container before tests run.
	/// </summary>
	public async Task InitializeAsync()
	{
		_mongoDbContainer = new MongoDbBuilder()
			.WithImage("mongo:8.0")
			.WithCleanUp(true)
			.WithPortBinding(0, 27017) // Use random available port mapped to container's 27017
			.Build();

		await _mongoDbContainer.StartAsync();

		MongoClient = new MongoClient(ConnectionString);

		// Verify connection
		await MongoClient.GetDatabase(DatabaseName).RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
	}

	/// <summary>
	/// Disposes the MongoDB container after tests complete.
	/// </summary>
	public async Task DisposeAsync()
	{
		if (_mongoDbContainer is not null)
		{
			await _mongoDbContainer.DisposeAsync();
		}
	}

	/// <summary>
	/// Creates a new MongoDbContext for testing.
	/// </summary>
	public IMongoDbContext CreateContext()
	{
		return new MongoDbContext(MongoClient, DatabaseName);
	}

	/// <summary>
	/// Clears all data from the Notes collection.
	/// </summary>
	public async Task ClearNotesAsync()
	{
		var context = CreateContext();
		await context.Notes.DeleteManyAsync(Builders<Note>.Filter.Empty);
	}

	/// <summary>
	/// Seeds the database with test notes.
	/// </summary>
	public async Task SeedNotesAsync(params Note[] notes)
	{
		if (notes.Length == 0)
		{
			return;
		}

		var context = CreateContext();
		await context.Notes.InsertManyAsync(notes);
	}

	/// <summary>
	/// Gets a note by ID from the database.
	/// </summary>
	public async Task<Note?> GetNoteByIdAsync(ObjectId id)
	{
		var context = CreateContext();
		return await context.Notes.Find(n => n.Id == id).FirstOrDefaultAsync();
	}

	/// <summary>
	/// Gets all notes for a specific user.
	/// </summary>
	public async Task<List<Note>> GetNotesByUserAsync(string ownerSubject)
	{
		var context = CreateContext();
		return await context.Notes
			.Find(n => n.OwnerSubject == ownerSubject)
			.ToListAsync();
	}

	/// <summary>
	/// Counts notes matching a filter.
	/// </summary>
	public async Task<long> CountNotesAsync(FilterDefinition<Note>? filter = null)
	{
		var context = CreateContext();
		filter ??= Builders<Note>.Filter.Empty;
		return await context.Notes.CountDocumentsAsync(filter);
	}

	/// <summary>
	/// Verifies database connection is healthy.
	/// </summary>
	public async Task<bool> IsHealthyAsync()
	{
		try
		{
			await MongoClient.GetDatabase(DatabaseName)
				.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Gets the total count of all notes in the database.
	/// </summary>
	public async Task<long> GetTotalNotesCountAsync()
	{
		var context = CreateContext();
		return await context.Notes.CountDocumentsAsync(Builders<Note>.Filter.Empty);
	}

	/// <summary>
	/// Updates a note in the database.
	/// </summary>
	public async Task<bool> UpdateNoteAsync(Note note)
	{
		var context = CreateContext();
		var result = await context.Notes.ReplaceOneAsync(
			n => n.Id == note.Id,
			note);
		return result.ModifiedCount > 0;
	}

	/// <summary>
	/// Deletes a note by ID.
	/// </summary>
	public async Task<bool> DeleteNoteAsync(ObjectId id)
	{
		var context = CreateContext();
		var result = await context.Notes.DeleteOneAsync(n => n.Id == id);
		return result.DeletedCount > 0;
	}

	/// <summary>
	/// Creates test notes for a specific user with auto-generated data.
	/// </summary>
	public async Task<Note[]> CreateTestNotesAsync(string ownerSubject, int count)
	{
		var notes = Enumerable.Range(1, count).Select(i => new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = $"Test Note {i}",
			Content = $"Test content for note {i}",
			OwnerSubject = ownerSubject,
			CreatedAt = DateTime.UtcNow.AddDays(-i),
			UpdatedAt = DateTime.UtcNow.AddDays(-i),
			IsArchived = false,
			AiSummary = $"Summary for note {i}",
			Tags = $"tag{i}, test, sample"
		}).ToArray();

		await SeedNotesAsync(notes);
		return notes;
	}
}
