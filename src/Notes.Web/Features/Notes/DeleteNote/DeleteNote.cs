// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DeleteNote.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using MediatR;

using Shared.Interfaces;

namespace Notes.Web.Features.Notes.DeleteNote;

public record DeleteNoteCommand : IRequest<DeleteNoteResponse>
{
	public ObjectId Id { get; init; }

	public string UserSubject { get; init; } = string.Empty;

}

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, DeleteNoteResponse>
{
	private readonly INoteRepository _repository;

	public DeleteNoteHandler(INoteRepository repository)
	{
		_repository = repository;
	}

	public async Task<DeleteNoteResponse> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
	{
		var noteResult = await _repository.GetNoteByIdAsync(request.Id);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject)
		{
			return new DeleteNoteResponse { Success = false, Message = "Note not found or access denied." };
		}

		await _repository.DeleteNote(noteResult.Value);

		return new DeleteNoteResponse
		{
			Success = true,
			Message = "Note deleted successfully."
		};
	}

}

public record DeleteNoteResponse
{
	public bool Success { get; init; }

	public string Message { get; init; } = string.Empty;

}