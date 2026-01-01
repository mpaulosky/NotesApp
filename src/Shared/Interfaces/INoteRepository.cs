// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     INoteRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Shared
// =======================================================

using System.Linq.Expressions;

using MongoDB.Bson;

using Shared.Abstractions;
using Shared.Entities;

namespace Shared.Interfaces;

/// <summary>
///   Repository interface for note data access operations.
/// </summary>
public interface INoteRepository
{
	/// <summary>
	///   Gets a note by its unique identifier.
	/// </summary>
	/// <param name="id">The note identifier.</param>
	/// <returns>A result containing the note if found, or null if not found.</returns>
	Task<Result<Note?>> GetNoteByIdAsync(ObjectId id);

	/// <summary>
	///   Gets all notes.
	/// </summary>
	/// <returns>A result containing all notes.</returns>
	Task<Result<IEnumerable<Note>?>> GetNotes();

	/// <summary>
	///   Gets notes matching the specified criteria.
	/// </summary>
	/// <param name="where">The filter expression.</param>
	/// <returns>A result containing notes matching the criteria.</returns>
	Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where);

	/// <summary>
	///   Gets notes matching the specified criteria with pagination.
	/// </summary>
	/// <param name="where">The filter expression.</param>
	/// <param name="skip">The number of notes to skip.</param>
	/// <param name="take">The number of notes to take.</param>
	/// <returns>A result containing paginated notes matching the criteria.</returns>
	Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where, int skip, int take);

	/// <summary>
	///   Gets the count of notes matching the specified criteria.
	/// </summary>
	/// <param name="where">The filter expression.</param>
	/// <returns>A result containing the count of matching notes.</returns>
	Task<Result<int>> GetNoteCount(Expression<Func<Note, bool>> where);

	/// <summary>
	///   Adds a new note to the repository.
	/// </summary>
	/// <param name="post">The note to add.</param>
	/// <returns>A result indicating success or failure.</returns>
	Task<Result> AddNote(Note post);

	/// <summary>
	///   Updates an existing note in the repository.
	/// </summary>
	/// <param name="post">The note to update.</param>
	/// <returns>A result indicating success or failure.</returns>
	Task<Result> UpdateNote(Note post);

	/// <summary>
	///   Deletes a note from the repository.
	/// </summary>
	/// <param name="post">The note to delete.</param>
	/// <returns>A result indicating success or failure.</returns>
	Task<Result> DeleteNote(Note post);

	/// <summary>
	///   Archives a note in the repository.
	/// </summary>
	/// <param name="post">The note to archive.</param>
	/// <returns>A result indicating success or failure.</returns>
	Task<Result> ArchiveNote(Note post);
}