// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     INoteService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web
// =======================================================

namespace Notes.Web.Services.Notes;

/// <summary>
///   Service interface for note operations.
/// </summary>
public interface INoteService
{
	/// <summary>
	///   Creates a new note.
	/// </summary>
	/// <param name="title">The note title.</param>
	/// <param name="content">The note content.</param>
	/// <param name="userSubject">The user's Auth0 subject.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The created note response or null if creation failed.</returns>
	Task<CreateNoteResponse?> CreateNoteAsync(string title, string content, string userSubject, CancellationToken cancellationToken = default);

	/// <summary>
	///   Gets the details of a specific note.
	/// </summary>
	/// <param name="id">The note ID.</param>
	/// <param name="userSubject">The user's Auth0 subject.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The note details response.</returns>
	Task<GetNoteDetailsResponse> GetNoteDetailsAsync(ObjectId id, string userSubject, CancellationToken cancellationToken = default);

	/// <summary>
	///   Updates an existing note.
	/// </summary>
	/// <param name="id">The note ID.</param>
	/// <param name="title">The note title.</param>
	/// <param name="content">The note content.</param>
	/// <param name="isArchived">Whether the note is archived.</param>
	/// <param name="userSubject">The user's Auth0 subject.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The update response.</returns>
	Task<UpdateNoteResponse> UpdateNoteAsync(ObjectId id, string title, string content, bool isArchived, string userSubject, CancellationToken cancellationToken = default);

	/// <summary>
	///   Lists all notes for a user.
	/// </summary>
	/// <param name="userSubject">The user's Auth0 subject.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The list of notes response.</returns>
	Task<ListNotesResponse> ListNotesAsync(string userSubject, CancellationToken cancellationToken = default);

	/// <summary>
	///   Archives or deletes a note.
	/// </summary>
	/// <param name="id">The note ID.</param>
	/// <param name="userSubject">The user's Auth0 subject.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The delete response.</returns>
	Task<DeleteNoteResponse> DeleteNoteAsync(ObjectId id, string userSubject, CancellationToken cancellationToken = default);
}
