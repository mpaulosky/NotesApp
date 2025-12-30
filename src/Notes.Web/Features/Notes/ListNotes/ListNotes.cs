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
public class ListNotesHandler : IRequestHandler<ListNotesQuery, ListNotesResponse>
{

	private readonly INoteRepository _repository;

	/// <summary>
	///   Initializes a new instance of the <see cref="ListNotesHandler" /> class.
	/// </summary>
	/// <param name="repository">The database context.</param>
	public ListNotesHandler(INoteRepository repository)
	{
		_repository = repository;
	}

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

public record ListNotesResponse
{

	public bool Success { get; init; }

	public string? Message { get; init; }

	public List<NoteListItem> Notes { get; init; } = new();

	public int TotalCount { get; init; }

	public int PageNumber { get; init; }

	public int PageSize { get; init; }

	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

}

public record NoteListItem
{

	public ObjectId Id { get; init; }

	public string Title { get; init; } = string.Empty;

	public string Content { get; init; } = string.Empty;

	public string? AiSummary { get; init; }

	public string? Tags { get; init; }

	public bool IsArchived { get; init; }

	public DateTime CreatedAt { get; init; }

	public DateTime UpdatedAt { get; init; }

}