// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AiServiceOptions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using System.ComponentModel.DataAnnotations;

namespace Notes.Web.Services.Ai;

/// <summary>
///   Configuration options for OpenAI API integration.
/// </summary>
public class AiServiceOptions
{
	/// <summary>
	///   Configuration section name in appsettings.json.
	/// </summary>
	public const string SectionName = "OpenAI";

	/// <summary>
	///   Gets or sets the OpenAI API key.
	/// </summary>
	[Required(ErrorMessage = "OpenAI API key is required.")]
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the model to use for chat completions (summaries).
	/// </summary>
	[Required(ErrorMessage = "Chat model is required.")]
	public string ChatModel { get; set; } = "gpt-4o-mini";

	/// <summary>
	///   Gets or sets the model to use for embeddings.
	/// </summary>
	[Required(ErrorMessage = "Embedding model is required.")]
	public string EmbeddingModel { get; set; } = "text-embedding-3-small";

	/// <summary>
	///   Gets or sets the maximum number of tokens for summary generation.
	/// </summary>
	[Range(1, 1000, ErrorMessage = "MaxSummaryTokens must be between 1 and 1000.")]
	public int MaxSummaryTokens { get; set; } = 150;

	/// <summary>
	///   Gets or sets the number of related notes to return.
	/// </summary>
	[Range(1, 20, ErrorMessage = "RelatedNotesCount must be between 1 and 20.")]
	public int RelatedNotesCount { get; set; } = 5;

	/// <summary>
	///   Gets or sets the similarity threshold for related notes (0.0 to 1.0).
	/// </summary>
	[Range(0.0, 1.0, ErrorMessage = "SimilarityThreshold must be between 0.0 and 1.0.")]
	public double SimilarityThreshold { get; set; } = 0.7;
}