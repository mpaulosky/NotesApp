// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : UI
// Project Name :  UI
// =======================================================

using Shared.Interfaces;

using Microsoft.Extensions.Options;

using OpenAI.Chat;

namespace Notes.Web.Services.Ai;

/// <summary>
///   Implementation of an AI service using OpenAI API.
/// </summary>
public class OpenAiService : IAiService
{

	private readonly IChatClientWrapper _chatClient;

	private readonly INoteRepository _repository;

	private readonly IEmbeddingClientWrapper _embeddingClient;

	private readonly AiServiceOptions _options;

	/// <summary>
	///   Initializes a new instance of the <see cref="OpenAiService" /> class.
	/// </summary>
	/// <param name="options">AI service configuration options.</param>
	/// <param name="repository">Repository for querying notes.</param>
	/// <param name="chatClient">Chat client for AI completions.</param>
	/// <param name="embeddingClient">Embedding client for vector generation.</param>
	public OpenAiService(
	IOptions<AiServiceOptions> options,
	INoteRepository repository,
	IChatClientWrapper chatClient,
	IEmbeddingClientWrapper embeddingClient)
	{
		_options = options.Value;
		_repository = repository;
		_chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
		_embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
	}

	/// <summary>
	///   Generates a concise summary for the given note content.
	/// </summary>
	public async Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(content))
		{
			return string.Empty;
		}

		try
		{
			var messages = new List<ChatMessage>
			{
					new SystemChatMessage("You are a helpful assistant that creates concise summaries of notes. " +
																"Keep summaries brief (1-2 sentences) and capture the main point."),
					new UserChatMessage($"Summarize this note:\n\n{content}")
			};

			var completionOptions = new ChatCompletionOptions
			{
				MaxOutputTokenCount = _options.MaxSummaryTokens,
				Temperature = 0.5f
			};

			var completion = await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);

			return completion.Value.Content[0].Text.Trim();
		}
		catch (Exception ex)
		{
			// Log the error (in production, use ILogger)
			Console.WriteLine($"Error generating summary: {ex.Message}");

			return string.Empty;
		}
	}

	/// <summary>
	///   Generates embeddings for semantic search and similarity matching.
	/// </summary>
	public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return Array.Empty<float>();
		}

		try
		{
			var embedding = await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);

			return embedding.Value.ToFloats().ToArray();
		}
		catch (Exception ex)
		{
			// Log the error (in production, use ILogger)
			Console.WriteLine($"Error generating embedding: {ex.Message}");

			return Array.Empty<float>();
		}
	}

	/// <summary>
	///   Generates relevant tags for the given note title and content.
	/// </summary>
	public async Task<string> GenerateTagsAsync(
		string title,
		string content,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
		{
			return string.Empty;
		}

		try
		{
			var messages = new List<ChatMessage>
			{
					new SystemChatMessage("You are a helpful assistant that generates relevant tags for notes. " +
																"Generate 3-5 relevant, specific tags that categorize the content. " +
																"Return ONLY the tags as a comma-separated list with no extra text. " +
																"Use lowercase, keep tags concise (1-3 words each)."),
					new UserChatMessage($"Generate tags for this note:\n\nTitle: {title}\n\nContent: {content}")
			};

			var completionOptions = new ChatCompletionOptions
			{
				MaxOutputTokenCount = 50,
				Temperature = 0.3f
			};

			var completion = await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);
			var tags = completion.Value.Content[0].Text.Trim();

			// Clean up the response - remove quotes, extra spaces, ensure comma separation
			tags = tags.Replace("\"", "").Replace("'", "").Trim();

			return tags;
		}
		catch (Exception ex)
		{
			// Log the error (in production, use ILogger)
			Console.WriteLine($"Error generating tags: {ex.Message}");

			return string.Empty;
		}
	}

	/// <summary>
	///   Finds related notes based on semantic similarity using embeddings.
	/// </summary>
	public async Task<List<ObjectId>> FindRelatedNotesAsync(
		float[] embedding,
		string userSubject,
		ObjectId? currentNoteId = null,
		int? topN = null,
		CancellationToken cancellationToken = default)
	{
		if (embedding == null || embedding.Length == 0)
		{
			return new List<ObjectId>();
		}

		try
		{
			var notesResult = await _repository.GetNotes(n => n.OwnerSubject == userSubject && n.Embedding != null);

			if (!notesResult.Success || notesResult.Value == null)
			{
				return new List<ObjectId>();
			}

			var notes = notesResult.Value.ToList();

			// Exclude current note if specified
			if (currentNoteId.HasValue)
			{
				notes = notes.Where(n => n.Id != currentNoteId.Value).ToList();
			}

			// Calculate cosine similarity for each note
			var similarities = notes
					.Select(note => new
					{
						NoteId = note.Id,
						Similarity = CalculateCosineSimilarity(embedding, note.Embedding!)
					})
					.Where(x => x.Similarity >= _options.SimilarityThreshold)
					.OrderByDescending(x => x.Similarity)
					.Take(topN is < 0 ? _options.RelatedNotesCount : topN ?? _options.RelatedNotesCount)
					.Select(x => x.NoteId)
					.ToList();

			return similarities;
		}
		catch (Exception ex)
		{
			// Log the error (in production, use ILogger)
			Console.WriteLine($"Error finding related notes: {ex.Message}");

			return new List<ObjectId>();
		}
	}

	/// <summary>
	///   Calculates the cosine similarity between two vectors.
	/// </summary>
	/// <param name="vectorA">First vector.</param>
	/// <param name="vectorB">Second vector.</param>
	/// <returns>Similarity score between 0 and 1.</returns>
	private static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
	{
		if (vectorA.Length != vectorB.Length)
		{
			return 0;
		}

		double dotProduct = 0;
		double magnitudeA = 0;
		double magnitudeB = 0;

		for (var i = 0; i < vectorA.Length; i++)
		{
			dotProduct += vectorA[i] * vectorB[i];
			magnitudeA += vectorA[i] * vectorA[i];
			magnitudeB += vectorB[i] * vectorB[i];
		}

		magnitudeA = Math.Sqrt(magnitudeA);
		magnitudeB = Math.Sqrt(magnitudeB);

		if (magnitudeA == 0 || magnitudeB == 0)
		{
			return 0;
		}

		return dotProduct / (magnitudeA * magnitudeB);
	}

}