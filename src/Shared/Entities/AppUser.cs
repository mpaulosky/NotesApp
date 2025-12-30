// Copyright (c) 2025 AINotesApp. All rights reserved.

namespace Shared.Entities;

/// <summary>
/// Represents a user (not persisted - Auth0 handles user data).
/// This is a DTO for application use only.
/// </summary>
public class AppUser
{
	/// <summary>
	/// Gets or sets the Auth0 subject identifier (unique user ID).
	/// </summary>
	public string Auth0Subject { get; set; } = null!;

	/// <summary>
	/// Gets or sets the user's display name.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the user's email address.
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// Gets or sets the UTC timestamp when the user was created.
	/// </summary>
	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Gets or sets the collection of notes owned by this user (not persisted).
	/// </summary>
	public ICollection<Note> Notes { get; set; } = new List<Note>();
}