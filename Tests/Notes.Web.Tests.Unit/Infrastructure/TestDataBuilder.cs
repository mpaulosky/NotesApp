using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Infrastructure;

/// <summary>
/// Test data builders for creating test entities.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestDataBuilder
{
	public static Note CreateNote(
		ObjectId? id = null,
		string title = "Test Note",
		string content = "Test Content",
		string? aiSummary = "Test Summary",
		string? tags = "test,note",
		float[]? embedding = null,
		string ownerSubject = "auth0|test123",
		DateTime? createdAt = null,
		DateTime? updatedAt = null,
		bool isArchived = false)
	{
		return new Note
		{
			Id = id ?? ObjectId.GenerateNewId(),
			Title = title,
			Content = content,
			AiSummary = aiSummary,
			Tags = tags,
			Embedding = embedding ?? [0.1f, 0.2f, 0.3f],
			OwnerSubject = ownerSubject,
			CreatedAt = createdAt ?? DateTime.UtcNow,
			UpdatedAt = updatedAt ?? DateTime.UtcNow,
			IsArchived = isArchived
		};
	}

	public static CreateNoteCommand CreateNoteCommand(
		string title = "Test Note",
		string content = "Test Content",
		string userSubject = "auth0|test123")
	{
		return new CreateNoteCommand
		{
			Title = title,
			Content = content,
			UserSubject = userSubject
		};
	}

	public static UpdateNoteCommand CreateUpdateNoteCommand(
		ObjectId? noteId = null,
		string title = "Updated Note",
		string content = "Updated Content",
		string userSubject = "auth0|test123")
	{
		return new UpdateNoteCommand
		{
			Id = noteId ?? ObjectId.GenerateNewId(),
			Title = title,
			Content = content,
			UserSubject = userSubject
		};
	}

	public static AppUser CreateAppUser(
		string auth0Subject = "auth0|test123",
		string? email = "test@example.com",
		string? name = "Test User",
		DateTime? createdUtc = null)
	{
		return new AppUser
		{
			Auth0Subject = auth0Subject,
			Email = email,
			Name = name,
			CreatedUtc = createdUtc ?? DateTime.UtcNow,
			Notes = new List<Note>()
		};
	}
}
