// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetNoteDetails.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Shared.Interfaces;

namespace Notes.Web.Features.Notes.GetNoteDetails;

/// <summary>
///   Query to get details of a specific note.
/// </summary>
public record GetNoteDetailsQuery : IRequest<GetNoteDetailsResponse?>
{
	/// <summary>
	///   Gets the ID of the note to retrieve.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;
}

/// <summary>
///   Handler for getting note details.
/// </summary>
/// <param name="repository">The note repository for data access.</param>
public class GetNoteDetailsHandler(INoteRepository repository) : IRequestHandler<GetNoteDetailsQuery, GetNoteDetailsResponse?>
{
	private readonly INoteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

	/// <summary>
	///   Handles the query to get note details.
	/// </summary>
	/// <param name="request">The query containing the note ID.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>A response containing the note details or null if not found.</returns>
	public async Task<GetNoteDetailsResponse?> Handle(GetNoteDetailsQuery request, CancellationToken cancellationToken)
	{
		var noteResult = await _repository.GetNoteByIdAsync(request.Id);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject)
		{
			return new GetNoteDetailsResponse
			{
				Success = false,
				Message = "Note not found or access denied."
			};
		}

		var note = noteResult.Value;

		return new GetNoteDetailsResponse
		{
			Success = true,
			Note = new NoteDetailsDto
			{
				Id = note.Id,
				Title = note.Title,
				Content = note.Content,
				AiSummary = note.AiSummary,
				Tags = note.Tags,
				IsArchived = note.IsArchived,
				CreatedAt = note.CreatedAt,
				UpdatedAt = note.UpdatedAt
			}
		};
	}

}

/// <summary>
///   Response containing note details.
/// </summary>
public record GetNoteDetailsResponse
{
	/// <summary>
	///   Gets a value indicating whether the retrieval was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	///   Gets an optional message providing additional information.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	///   Gets the note details if found.
	/// </summary>
	public NoteDetailsDto? Note { get; init; }
}

/// <summary>
///   Data transfer object for note details.
/// </summary>
public record NoteDetailsDto
{
	/// <summary>
	///   Gets the unique identifier for the note.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the note title.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the note content.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the AI-generated summary of the note.
	/// </summary>
	public string? AiSummary { get; init; }

	/// <summary>
	///   Gets the comma-separated tags for the note.
	/// </summary>
	public string? Tags { get; init; }

	/// <summary>
	///   Gets a value indicating whether the note is archived.
	/// </summary>
	public bool IsArchived { get; init; }

	/// <summary>
	///   Gets the creation timestamp.
	/// </summary>
	public DateTime CreatedAt { get; init; }

	/// <summary>
	///   Gets the last update timestamp.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

}