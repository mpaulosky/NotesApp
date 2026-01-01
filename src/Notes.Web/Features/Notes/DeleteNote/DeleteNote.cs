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

/// <summary>
///   Command to delete (archive) a note.
/// </summary>
public record DeleteNoteCommand : IRequest<DeleteNoteResponse>
{
	/// <summary>
	///   Gets the ID of the note to delete.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;
}

/// <summary>
///   Handler for deleting (archiving) a note.
/// </summary>
/// <param name="repository">The note repository for data access.</param>
public class DeleteNoteHandler(INoteRepository repository) : IRequestHandler<DeleteNoteCommand, DeleteNoteResponse>
{
	private readonly INoteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

	/// <summary>
	///   Handles the command to delete (archive) a note.
	/// </summary>
	/// <param name="request">The command containing the note ID to delete.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>A response indicating success or failure.</returns>
	public async Task<DeleteNoteResponse> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
	{
		var noteResult = await _repository.GetNoteByIdAsync(request.Id);

		if (!noteResult.Success || noteResult.Value == null || noteResult.Value.OwnerSubject != request.UserSubject)
		{
			return new DeleteNoteResponse { Success = false, Message = "Note not found or access denied." };
		}

		await _repository.ArchiveNote(noteResult.Value);

		return new DeleteNoteResponse
		{
			Success = true,
			Message = "Note archived successfully."
		};
	}

}

/// <summary>
///   Response after deleting (archiving) a note.
/// </summary>
public record DeleteNoteResponse
{
	/// <summary>
	///   Gets a value indicating whether the deletion was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	///   Gets a message providing additional information about the operation result.
	/// </summary>
	public string Message { get; init; } = string.Empty;
}