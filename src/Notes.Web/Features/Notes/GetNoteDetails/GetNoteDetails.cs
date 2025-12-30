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

public record GetNoteDetailsQuery : IRequest<GetNoteDetailsResponse?>
{
	public ObjectId Id { get; init; }

	public string UserSubject { get; init; } = string.Empty;

}

public class GetNoteDetailsHandler : IRequestHandler<GetNoteDetailsQuery, GetNoteDetailsResponse?>
{
	private readonly INoteRepository _repository;

	public GetNoteDetailsHandler(INoteRepository repository)
	{
		_repository = repository;
	}

	public async Task<GetNoteDetailsResponse?> Handle(GetNoteDetailsQuery request, CancellationToken cancellationToken)
	{
		var noteResult = await _repository.GetNoteByIdAsync(request.Id);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject)
		{
			return null;
		}

		var note = noteResult.Value;

		return new GetNoteDetailsResponse
		{
			Id = note.Id,
			Title = note.Title,
			Content = note.Content,
			AiSummary = note.AiSummary,
			Tags = note.Tags,
			CreatedAt = note.CreatedAt,
			UpdatedAt = note.UpdatedAt
		};
	}

}

public record GetNoteDetailsResponse
{
	public ObjectId Id { get; init; }

	public string Title { get; init; } = string.Empty;

	public string Content { get; init; } = string.Empty;

	public string? AiSummary { get; init; }

	public string? Tags { get; init; }

	public DateTime CreatedAt { get; init; }

	public DateTime UpdatedAt { get; init; }

}