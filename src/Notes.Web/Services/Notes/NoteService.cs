// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web
// =======================================================

namespace Notes.Web.Services.Notes;

/// <summary>
///   Service for note operations that abstracts MediatR from Blazor components.
/// </summary>
/// <param name="mediator">The MediatR mediator for sending commands and queries.</param>
public sealed class NoteService(IMediator mediator) : INoteService
{
	private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

	/// <inheritdoc />
	public async Task<CreateNoteResponse?> CreateNoteAsync(string title, string content, string userSubject, CancellationToken cancellationToken = default)
	{
		var command = new CreateNoteCommand
		{
			Title = title,
			Content = content,
			UserSubject = userSubject
		};

		return await _mediator.Send(command, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<GetNoteDetailsResponse> GetNoteDetailsAsync(ObjectId id, string userSubject, CancellationToken cancellationToken = default)
	{
		var query = new GetNoteDetailsQuery
		{
			Id = id,
			UserSubject = userSubject
		};

		return await _mediator.Send(query, cancellationToken) ?? new GetNoteDetailsResponse
		{
			Success = false,
			Message = "Failed to retrieve note details."
		};
	}

	/// <inheritdoc />
	public async Task<UpdateNoteResponse> UpdateNoteAsync(ObjectId id, string title, string content, bool isArchived, string userSubject, CancellationToken cancellationToken = default)
	{
		var command = new UpdateNoteCommand
		{
			Id = id,
			Title = title,
			Content = content,
			IsArchived = isArchived,
			UserSubject = userSubject
		};

		return await _mediator.Send(command, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<ListNotesResponse> ListNotesAsync(string userSubject, CancellationToken cancellationToken = default)
	{
		var query = new ListNotesQuery
		{
			UserSubject = userSubject
		};

		return await _mediator.Send(query, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<DeleteNoteResponse> DeleteNoteAsync(ObjectId id, string userSubject, CancellationToken cancellationToken = default)
	{
		var command = new DeleteNoteCommand
		{
			Id = id,
			UserSubject = userSubject
		};

		return await _mediator.Send(command, cancellationToken);
	}
}
