// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Web
// =======================================================

using System.Linq.Expressions;

namespace Notes.Web.Data.Repositories;

/// <summary>
/// Note repository implementation using native MongoDB.Driver with a factory pattern.
/// </summary>
public class NoteRepository(IMongoDbContextFactory contextFactory) : INoteRepository
{

	/// <summary>
	/// Gets an article by its unique identifier.
	/// </summary>
	/// <param name="id">The ObjectId of the article.</param>
	/// <returns>A <see cref="Result"/> containing the article if found, or an error message.</returns>
	public async Task<Result<Note?>> GetNoteByIdAsync(ObjectId id)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			Note? article = await context.Notes
					.Find(a => a.Id == id)
					.FirstOrDefaultAsync();

			return Result.Ok<Note?>(article);
		}
		catch (Exception ex)
		{
			return Result.Fail<Note?>($"Error getting article by id: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all non-archived articles.
	/// </summary>
	/// <returns>A <see cref="Result{IEnumerable{Note}}"/> containing the articles or an error message.</returns>
	public async Task<Result<IEnumerable<Note>?>> GetNotes()
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			List<Note>? articles = await context.Notes
					.Find(_ => true)
					.ToListAsync();

			return Result.Ok<IEnumerable<Note>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Note>?>($"Error getting articles: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets articles matching a specified predicate.
	/// </summary>
	/// <param name="where">The predicate to filter articles.</param>
	/// <returns>A <see cref="Result{IEnumerable{Note}}"/> containing the filtered articles or an error message.</returns>
	public async Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			List<Note>? articles = await context.Notes
					.Find(where)
					.ToListAsync();

			return Result.Ok<IEnumerable<Note>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Note>?>($"Error getting articles: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets articles matching a specified predicate with pagination.
	/// </summary>
	/// <param name="where"></param>
	/// <param name="skip"></param>
	/// <param name="take"></param>
	/// <returns></returns>
	public async Task<Result<IEnumerable<Note>?>> GetNotes(Expression<Func<Note, bool>> where, int skip, int take)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			List<Note>? articles = await context.Notes
					.Find(where)
					.Skip(skip)
					.Limit(take)
					.ToListAsync();

			return Result.Ok<IEnumerable<Note>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Note>?>($"Error getting articles: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets the count of articles matching a specified predicate.
	/// </summary>
	/// <param name="where"></param>
	/// <returns></returns>
	public async Task<Result<int>> GetNoteCount(Expression<Func<Note, bool>> where)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			var count = await context.Notes
					.CountDocumentsAsync(where);

			return Result.Ok((int)count);
		}
		catch (Exception ex)
		{
			return Result.Fail<int>($"Error getting article count: {ex.Message}");
		}
	}

	/// <summary>
	/// Adds a new article to the database.
	/// </summary>
	/// <param name="post">The article to add.</param>
	/// <returns>A <see cref="Result"/> containing the added article or an error message.</returns>
	public async Task<Result> AddNote(Note post)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Notes.InsertOneAsync(post);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail($"Error adding article: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates an existing article in the database.
	/// </summary>
	/// <param name="post">The article to update.</param>
	/// <returns>A <see cref="Result"/> containing the updated article or an error message.</returns>
	public async Task<Result> UpdateNote(Note post)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Notes.ReplaceOneAsync(a => a.Id == post.Id, post);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail($"Error updating article: {ex.Message}");
		}
	}

	/// <summary>
	/// Deletes an article from the database.
	/// </summary>
	/// <param name="post"></param>
	/// <returns></returns>
	public async Task<Result> DeleteNote(Note post)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Notes.DeleteOneAsync(a => a.Id == post.Id);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail($"Error deleting article: {ex.Message}");
		}
	}

	/// <summary>
	/// Archives an article by its slug.
	/// </summary>
	/// <param name="post"></param>
	public async Task<Result> ArchiveNote(Note post)
	{

		try
		{

			IMongoDbContext context = contextFactory.CreateDbContext();
			
			post.IsArchived = true;
			
			await context.Notes.ReplaceOneAsync(a => a.Id == post.Id, post);

			return Result.Ok();

		}
		catch (Exception ex)
		{
			return Result.Fail($"Error updating article: {ex.Message}");
		}

	}

}