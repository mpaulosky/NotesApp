// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ListNotes.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Shared.Interfaces;

namespace Notes.Web.Features.Notes.ListNotes;

/// <summary>
///   Query to list notes for a user with pagination.
/// </summary>
public record ListNotesQuery : IRequest<ListNotesResponse>
{

	/// <summary>
	///   Gets the Auth0 subject whose notes are being listed.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets the page number for pagination.
	/// </summary>
	public int PageNumber { get; init; } = 1;

	/// <summary>
	///   Gets the page size for pagination.
	/// </summary>
	public int PageSize { get; init; } = 10;

}

/// <summary>
///   Handler for listing notes for a user with pagination.
/// </summary>
/// <param name="repository">The note repository for data access.</param>
public class ListNotesHandler(INoteRepository repository) : IRequestHandler<ListNotesQuery, ListNotesResponse>
{
	private readonly INoteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

	/// <summary>
	///   Handles the query to list notes for a user.
	/// </summary>
	/// <param name="request">The query parameters.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing the list of notes and pagination info.</returns>
	public async Task<ListNotesResponse> Handle(ListNotesQuery request, CancellationToken cancellationToken)
	{
		var countResult = await _repository.GetNoteCount(n => n.OwnerSubject == request.UserSubject);
		var totalCount = countResult.Success ? countResult.Value : 0;

		var notesResult = await _repository.GetNotes(
			n => n.OwnerSubject == request.UserSubject,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize);

		var notes = notesResult.Success && notesResult.Value != null
			? notesResult.Value.Select(n => new NoteListItem
			{
				Id = n.Id,
				Title = n.Title,
				Content = n.Content,
				AiSummary = n.AiSummary,
				Tags = n.Tags,
				IsArchived = n.IsArchived,
				CreatedAt = n.CreatedAt,
				UpdatedAt = n.UpdatedAt
			}).ToList()
			: new List<NoteListItem>();

		return new ListNotesResponse
		{
			Success = true,
			Notes = notes,
			TotalCount = totalCount,
			PageNumber = request.PageNumber,
			PageSize = request.PageSize
		};
	}

}

/// <summary>
///   Response containing a list of notes with pagination information.
/// </summary>
public record ListNotesResponse
{
	/// <summary>
	///   Gets a value indicating whether the operation was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	///   Gets an optional message providing additional information.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	///   Gets the list of notes for the current page.
	/// </summary>
	public List<NoteListItem> Notes { get; init; } = new();

	/// <summary>
	///   Gets the total count of notes across all pages.
	/// </summary>
	public int TotalCount { get; init; }

	/// <summary>
	///   Gets the current page number.
	/// </summary>
	public int PageNumber { get; init; }

	/// <summary>
	///   Gets the number of items per page.
	/// </summary>
	public int PageSize { get; init; }

	/// <summary>
	///   Gets the total number of pages.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
///   Represents a single note item in a list view.
/// </summary>
public record NoteListItem
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