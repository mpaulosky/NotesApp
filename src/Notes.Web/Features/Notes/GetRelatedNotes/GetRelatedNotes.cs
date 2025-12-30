// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetRelatedNotes.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Shared.Interfaces;

using Notes.Web.Services.Ai;

// using Microsoft.EntityFrameworkCore; (duplicate removed)

namespace Notes.Web.Features.Notes.GetRelatedNotes;

/// <summary>
///   Query to get related notes based on semantic similarity.
/// </summary>
public record GetRelatedNotesQuery : IRequest<GetRelatedNotesResponse>
{

	/// <summary>
	///   Gets the ID of the note to find related notes for.
	/// </summary>
	public ObjectId NoteId { get; init; }

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets the number of related notes to return.
	/// </summary>
	public int TopN { get; init; } = 5;

}

/// <summary>
///   Handler for finding related notes using AI embeddings.
/// </summary>
public class GetRelatedNotesHandler : IRequestHandler<GetRelatedNotesQuery, GetRelatedNotesResponse>
{

	private readonly IAiService _aiService;

	private readonly INoteRepository _repository;

	public GetRelatedNotesHandler(INoteRepository repository, IAiService aiService)
	{
		_repository = repository;
		_aiService = aiService;
	}

	/// <summary>
	///   Handles the query to find related notes.
	/// </summary>
	public async Task<GetRelatedNotesResponse> Handle(GetRelatedNotesQuery request, CancellationToken cancellationToken)
	{
		// Get the current note
		var noteResult = await _repository.GetNoteByIdAsync(request.NoteId);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject || noteResult.Value.Embedding == null)
		{
			return new GetRelatedNotesResponse();
		}

		var currentNote = noteResult.Value;

		// Find related notes using AI service
		var relatedNoteIds = await _aiService.FindRelatedNotesAsync(
			currentNote.Embedding,
			request.UserSubject,
			request.NoteId,
			request.TopN,
			cancellationToken);

		if (!relatedNoteIds.Any())
		{
			return new GetRelatedNotesResponse();
		}

		// Get the related notes with details
		var relatedNotesResult = await _repository.GetNotes(n => relatedNoteIds.Contains(n.Id));

		if (!relatedNotesResult.Success || relatedNotesResult.Value == null)
		{
			return new GetRelatedNotesResponse();
		}

		var relatedNotes = relatedNotesResult.Value.ToList();

		// Sort by the order returned from AI service (by similarity)
		var sortedNotes = relatedNoteIds
			.Select(id => relatedNotes.FirstOrDefault(n => n.Id == id))
			.Where(n => n != null)
			.Select(n => new RelatedNoteItem
			{
				Id = n!.Id,
				Title = n.Title,
				AiSummary = n.AiSummary,
				UpdatedAt = n.UpdatedAt
			})
			.ToList();

		return new GetRelatedNotesResponse
		{
			RelatedNotes = sortedNotes
		};
	}

}

/// <summary>
///   Response containing related notes.
/// </summary>
public record GetRelatedNotesResponse
{

	/// <summary>
	///   Gets the list of related notes.
	/// </summary>
	public List<RelatedNoteItem> RelatedNotes { get; init; } = new();

}

/// <summary>
///   Information about a related note.
/// </summary>
public record RelatedNoteItem
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
	///   Gets the last update timestamp.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

}