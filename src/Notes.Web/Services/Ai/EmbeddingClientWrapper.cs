// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EmbeddingClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Web
// =======================================================

using OpenAI.Embeddings;

using System.ClientModel;

namespace Notes.Web.Services.Ai;

/// <summary>
///   Wrapper implementation for EmbeddingClient to enable dependency injection.
/// </summary>
/// <param name="embeddingClient">The underlying OpenAI embedding client.</param>
public class EmbeddingClientWrapper(EmbeddingClient embeddingClient) : IEmbeddingClientWrapper
{

	private readonly EmbeddingClient _embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));

	/// <inheritdoc />
	public async Task<ClientResult<OpenAIEmbedding>> GenerateEmbeddingAsync(
		string text,
		CancellationToken cancellationToken = default)
	{
		return await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
	}

}
