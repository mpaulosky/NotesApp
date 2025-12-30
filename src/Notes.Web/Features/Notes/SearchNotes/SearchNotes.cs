using MediatR;

// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SearchNotes.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using Shared.Interfaces;

namespace Notes.Web.Features.Notes.SearchNotes;

/// <summary>
///   Query to search notes by title and content.
/// </summary>
public record SearchNotesQuery : IRequest<SearchNotesResponse>
{

	/// <summary>
	///   Gets the search term to filter notes.
	/// </summary>
	public string SearchTerm { get; init; } = string.Empty;

	/// <summary>
	///   Gets the Auth0 subject for authorization.
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
///   Handler for searching notes by title and content.
/// </summary>
public class SearchNotesHandler : IRequestHandler<SearchNotesQuery, SearchNotesResponse>
{

	private readonly INoteRepository _repository;

	public SearchNotesHandler(INoteRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	///   Handles the search notes query.
	/// </summary>
	public async Task<SearchNotesResponse> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
	{
		System.Linq.Expressions.Expression<Func<Note, bool>> filter = n => n.OwnerSubject == request.UserSubject;

		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var searchTerm = request.SearchTerm.ToLower();
			filter = n => n.OwnerSubject == request.UserSubject &&
			              (n.Title.ToLower().Contains(searchTerm) ||
			               n.Content.ToLower().Contains(searchTerm));
		}

		var countResult = await _repository.GetNoteCount(filter);
		var totalCount = countResult.Success ? countResult.Value : 0;

		var notesResult = await _repository.GetNotes(
			filter,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize);

		var notes = notesResult.Success && notesResult.Value != null
			? notesResult.Value.Select(n => new SearchNoteItem
				{
					Id = n.Id,
					Title = n.Title,
					AiSummary = n.AiSummary,
					Tags = n.Tags,
					CreatedAt = n.CreatedAt,
					UpdatedAt = n.UpdatedAt
				}).ToList()
			: new List<SearchNoteItem>();

		return new SearchNotesResponse
		{
				Notes = notes,
				TotalCount = totalCount,
				PageNumber = request.PageNumber,
				PageSize = request.PageSize,
				SearchTerm = request.SearchTerm
		};
	}

}

/// <summary>
///   Response containing search results.
/// </summary>
public record SearchNotesResponse
{

	/// <summary>
	///   Gets the list of notes matching the search.
	/// </summary>
	public List<SearchNoteItem> Notes { get; init; } = new();

	/// <summary>
	///   Gets the total count of matching notes.
	/// </summary>
	public int TotalCount { get; init; }

	/// <summary>
	///   Gets the current page number.
	/// </summary>
	public int PageNumber { get; init; }

	/// <summary>
	///   Gets the page size.
	/// </summary>
	public int PageSize { get; init; }

	/// <summary>
	///   Gets the total number of pages.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

	/// <summary>
	///   Gets the search term used.
	/// </summary>
	public string SearchTerm { get; init; } = string.Empty;

}

/// <summary>
///   Information about a note in search results.
/// </summary>
public record SearchNoteItem
{

	/// <summary>
	///   Gets the note ID.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the note title.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the AI-generated summary.
	/// </summary>
	public string? AiSummary { get; init; }

	/// <summary>
	///   Gets the AI-generated tags.
	/// </summary>
	public string? Tags { get; init; }

	/// <summary>
	///   Gets the creation date.
	/// </summary>
	public DateTime CreatedAt { get; init; }

	/// <summary>
	///   Gets the last update date.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

}