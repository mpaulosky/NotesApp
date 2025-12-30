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

public interface INoteRepository
{

	Task<Result<Note?>> GetNoteByIdAsync(ObjectId id);

	Task<Result<IEnumerable<Note>?>> GetNotes();

	Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where);

	Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where, int skip, int take);

	Task<Result<int>> GetNoteCount(Expression<Func<Note, bool>> where);

	Task<Result> AddNote(Note post);

	Task<Result> UpdateNote(Note post);

	Task<Result> DeleteNote(Note post);

	Task<Result> ArchiveNote(Note post);

}