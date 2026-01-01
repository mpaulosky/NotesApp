// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     UpdateNote.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Notes.Web.Services.Ai;

using Shared.Interfaces;

// using Microsoft.EntityFrameworkCore; (duplicate removed)

namespace Notes.Web.Features.Notes.UpdateNote;

/// <summary>
///   Command to update an existing note.
/// </summary>
public record UpdateNoteCommand : IRequest<UpdateNoteResponse>
{

	/// <summary>
	///   Gets the ID of the note to update.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the updated title.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the updated content.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets or sets whether the note is archived.
	/// </summary>
	public bool IsArchived { get; init; }

}

/// <summary>
///   Handler for updating a note with AI regeneration.
/// </summary>
/// <param name="repository">The note repository for data access.</param>
/// <param name="aiService">The AI service for regenerating summaries, tags, and embeddings.</param>
public class UpdateNoteHandler(INoteRepository repository, IAiService aiService) : IRequestHandler<UpdateNoteCommand, UpdateNoteResponse>
{
	private readonly IAiService _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
	private readonly INoteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

	/// <summary>
	///   Handles the command to update a note.
	/// </summary>
	/// <param name="request">The command containing updated note data.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>A response indicating success or failure with updated note details.</returns>
	public async Task<UpdateNoteResponse> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
	{
		var noteResult = await _repository.GetNoteByIdAsync(request.Id);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject)
		{
			return new UpdateNoteResponse { Success = false, Message = "Note not found or access denied." };
		}

		var note = noteResult.Value;

		// Regenerate AI summary, tags, and embedding if content changed
		var summaryTask = _aiService.GenerateSummaryAsync(request.Content, cancellationToken);
		var tagsTask = _aiService.GenerateTagsAsync(request.Title, request.Content, cancellationToken);
		var embeddingTask = _aiService.GenerateEmbeddingAsync(request.Content, cancellationToken);

		await Task.WhenAll(summaryTask, tagsTask, embeddingTask);
		var summary = await summaryTask;
		var tags = await tagsTask;
		var embedding = await embeddingTask;

		note.Title = request.Title;
		note.Content = request.Content;
		note.AiSummary = summary;
		note.Tags = tags;
		note.Embedding = embedding;
		note.IsArchived = request.IsArchived;
		note.UpdatedAt = System.DateTime.UtcNow;

		await _repository.UpdateNote(note);

		return new UpdateNoteResponse
		{
			Success = true,
			Id = note.Id,
			Title = note.Title,
			Content = note.Content,
			AiSummary = note.AiSummary,
			Tags = note.Tags,
			IsArchived = note.IsArchived,
			UpdatedAt = note.UpdatedAt
		};
	}

}

/// <summary>
///   Response after updating a note.
/// </summary>
public record UpdateNoteResponse
{

	/// <summary>
	///   Gets whether the update was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	///   Gets the error message if unsuccessful.
	/// </summary>
	public string Message { get; init; } = string.Empty;

	/// <summary>
	///   Gets the ID of the updated note.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the title of the note.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the content of the note.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the AI-generated summary.
	/// </summary>
	public string? AiSummary { get; init; }

	/// <summary>
	///   Gets the AI-generated tags.
	/// </summary>
	public string? Tags { get; init; }

	/// <summary>
	///   Gets whether the note is archived.
	/// </summary>
	public bool IsArchived { get; init; }

	/// <summary>
	///   Gets the update timestamp.
	/// </summary>
	public System.DateTime UpdatedAt { get; init; }

}