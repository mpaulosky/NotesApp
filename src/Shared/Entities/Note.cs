using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Entities;

/// <summary>
/// Represents a user note with AI-generated metadata.
/// </summary>
public class Note
{
	/// <summary>
	/// Gets or sets the unique identifier for the note.
	/// </summary>
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public ObjectId Id { get; set; }

	/// <summary>
	/// Gets or sets the note title.
	/// </summary>
	[BsonElement("title")]
	[BsonRequired]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the note content.
	/// </summary>
	[BsonElement("content")]
	[BsonRequired]
	public string Content { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the AI-generated summary of the note.
	/// </summary>
	[BsonElement("aiSummary")]
	public string? AiSummary { get; set; }

	/// <summary>
	/// Gets or sets the comma-separated tags for the note.
	/// </summary>
	[BsonElement("tags")]
	public string? Tags { get; set; }

	/// <summary>
	/// Gets or sets the AI-generated embedding vector for semantic search.
	/// </summary>
	[BsonElement("embedding")]
	public float[]? Embedding { get; set; }

	/// <summary>
	/// Gets or sets the creation timestamp.
	/// </summary>
	[BsonElement("createdAt")]
	[BsonRequired]
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets the last update timestamp.
	/// </summary>
	[BsonElement("updatedAt")]
	[BsonRequired]
	public DateTime UpdatedAt { get; set; }

	/// <summary>
	/// Gets or sets the Auth0 subject identifier of the note owner.
	/// </summary>
	[BsonElement("ownerSubject")]
	[BsonRequired]
	public string OwnerSubject { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the owner navigation property (not persisted to MongoDB).
	/// </summary>
	[BsonIgnore]
	public AppUser? Owner { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the note is archived.
	/// </summary>
	[BsonElement("isArchived")]
	public bool IsArchived { get; set; }
}