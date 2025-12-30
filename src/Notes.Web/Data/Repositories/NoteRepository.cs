// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using System.Linq.Expressions;

namespace Notes.Web.Data.Repositories;

/// <summary>
/// Article repository implementation using native MongoDB.Driver with factory pattern.
/// </summary>
public class ArticleRepository
(
		IMongoDbContextFactory contextFactory
) : IArticleRepository
{

	/// <summary>
	/// Gets an article by its unique identifier.
	/// </summary>
	/// <param name="id">The ObjectId of the article.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the article if found, or an error message.</returns>
	public async Task<Result<Article?>> GetArticleByIdAsync(ObjectId id)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			Article? article = await context.Notes
					.Find(a => a.Id == id)
					.FirstOrDefaultAsync();

			return Result.Ok<Article?>(article);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article?>($"Error getting article by id: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets an article by its date string and slug.
	/// </summary>
	/// <param name="dateString">The date string of the article.</param>
	/// <param name="slug">The slug of the article.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the article if found, or an error message.</returns>
	public async Task<Result<Article?>> GetArticle(string dateString, string slug)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			Article? article = await context.Notes
					.Find(a => a.Slug == slug)
					.FirstOrDefaultAsync();

			return Result.Ok<Article?>(article);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article?>($"Error getting article: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all non-archived articles.
	/// </summary>
	/// <returns>A <see cref="Result{IEnumerable{Article}}"/> containing the articles or an error message.</returns>
	public async Task<Result<IEnumerable<Article>?>> GetArticles()
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			List<Article>? articles = await context.Notes
					.Find(_ => true)
					.ToListAsync();

			return Result.Ok<IEnumerable<Article>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Article>?>($"Error getting articles: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets articles matching a specified predicate.
	/// </summary>
	/// <param name="where">The predicate to filter articles.</param>
	/// <returns>A <see cref="Result{IEnumerable{Article}}"/> containing the filtered articles or an error message.</returns>
	public async Task<Result<IEnumerable<Article>?>> GetArticles(Expression<Func<Article, bool>> where)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();

			List<Article>? articles = await context.Notes
					.Find(where)
					.ToListAsync();

			return Result.Ok<IEnumerable<Article>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Article>?>($"Error getting articles: {ex.Message}");
		}
	}

	/// <summary>
	/// Adds a new article to the database.
	/// </summary>
	/// <param name="post">The article to add.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the added article or an error message.</returns>
	public async Task<Result<Article>> AddArticle(Article post)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Notes.InsertOneAsync(post);

			return Result.Ok(post);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article>($"Error adding article: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates an existing article in the database.
	/// </summary>
	/// <param name="post">The article to update.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the updated article or an error message.</returns>
	public async Task<Result<Article>> UpdateArticle(Article post)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Notes.ReplaceOneAsync(a => a.Id == post.Id, post);

			return Result.Ok(post);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article>($"Error updating article: {ex.Message}");
		}
	}

	/// <summary>
	/// Archives an article by its slug.
	/// </summary>
	/// <param name="slug">The slug of the article to archive.</param>
	public async Task ArchiveArticle(string slug)
	{
		IMongoDbContext context = contextFactory.CreateDbContext();
		UpdateDefinition<Article>? update = Builders<Article>.Update.Set(a => a.IsArchived, true);
		await context.Notes.UpdateOneAsync(a => a.Slug == slug, update);
	}

}
