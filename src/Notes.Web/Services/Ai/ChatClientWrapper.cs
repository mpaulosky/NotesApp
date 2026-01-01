// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ChatClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Web
// =======================================================

using OpenAI.Chat;

using System.ClientModel;

namespace Notes.Web.Services.Ai;

/// <summary>
///   Wrapper implementation for ChatClient to enable dependency injection.
/// </summary>
/// <param name="chatClient">The underlying OpenAI chat client.</param>
public class ChatClientWrapper(ChatClient chatClient) : IChatClientWrapper
{

	private readonly ChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));

	/// <inheritdoc />
	public async Task<ClientResult<ChatCompletion>> CompleteChatAsync(
		IEnumerable<ChatMessage> messages,
		ChatCompletionOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		return await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
	}

}
