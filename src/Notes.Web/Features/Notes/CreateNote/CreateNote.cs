// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateNote.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Notes.Web.Services.Ai;

namespace Notes.Web.Features.Notes.CreateNote;

/// <summary>
///   Command to create a new note.
/// </summary>
public record CreateNoteCommand : IRequest<CreateNoteResponse>
{

	/// <summary>
	///   Gets the title of the note.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the content of the note.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the Auth0 subject for the note owner.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

}

/// <summary>
///   Handler for creating a new note with AI-generated summary, tags, and embeddings.
/// </summary>
public class CreateNoteHandler : IRequestHandler<CreateNoteCommand, CreateNoteResponse>
{

	private readonly IAiService _aiService;

	private readonly INoteRepository _repository;

	public CreateNoteHandler(INoteRepository repository, IAiService aiService)
	{
		_repository = repository;
		_aiService = aiService;
	}

	/// <summary>
	///   Handles the command to create a note.
	/// </summary>
	public async Task<CreateNoteResponse> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
	{
		// Generate AI summary, tags, and embedding in parallel
		var summaryTask = _aiService.GenerateSummaryAsync(request.Content, cancellationToken);
		var tagsTask = _aiService.GenerateTagsAsync(request.Title, request.Content, cancellationToken);
		var embeddingTask = _aiService.GenerateEmbeddingAsync(request.Content, cancellationToken);

		await Task.WhenAll(summaryTask, tagsTask, embeddingTask);
		var summary = await summaryTask;
		var tags = await tagsTask;
		var embedding = await embeddingTask;

		var note = new Note
		{
			Id = ObjectId.GenerateNewId(),
			Title = request.Title,
			Content = request.Content,
			AiSummary = summary,
			Tags = tags,
			Embedding = embedding,
			OwnerSubject = request.UserSubject,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow, 
			IsArchived = false
		};

		await _repository.AddNote(note);

		return new CreateNoteResponse
		(
			note.Id,
			note.Title,
			note.Content,
			note.AiSummary,
			note.Tags,
			note.Embedding,
			note.OwnerSubject,
			note.UpdatedAt,
			note.CreatedAt,
			note.IsArchived
		);
	}

}

/// <summary>
///   Response after creating a note.
/// </summary>
public record CreateNoteResponse(
	ObjectId Id,
	string Title,
	string Content,
	string? AiSummary,
	string? Tags,
	float[] Embedding,
	string OwnerSubject,
	DateTime UpdatedAt,
	DateTime CreatedAt,
	bool IsArchived
);