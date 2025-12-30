// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IArticleRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Interfaces;

public interface IArticleRepository
{

	Task<Result<Article?>> GetArticleByIdAsync(ObjectId id);

	Task<Result<Article?>> GetArticle(string dateString, string slug);

	Task<Result<IEnumerable<Article>?>> GetArticles();

	Task<Result<IEnumerable<Article>?>> GetArticles(Expression<Func<Article, bool>> where);

	Task<Result<Article>> AddArticle(Article post);

	Task<Result<Article>> UpdateArticle(Article post);

	Task ArchiveArticle(string slug);

}