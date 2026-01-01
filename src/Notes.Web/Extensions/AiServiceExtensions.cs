// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AiServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Web
// =======================================================

using Notes.Web.Services.Ai;

using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace Microsoft.Extensions.Hosting;

/// <summary>
///   Extension methods for configuring AI services.
/// </summary>
public static class AiServiceExtensions
{

	/// <summary>
	///   Adds AI services including OpenAI integration to the service collection.
	/// </summary>
	/// <param name="builder">The web application builder.</param>
	/// <returns>The web application builder for chaining.</returns>
	public static IHostApplicationBuilder AddAiServices(this IHostApplicationBuilder builder)
	{
		// Configure AI service options from configuration
		builder.Services.Configure<AiServiceOptions>(
			builder.Configuration.GetSection(AiServiceOptions.SectionName));

		// Register OpenAI client wrappers as singletons (they wrap stateless clients)
		builder.Services.AddSingleton<IChatClientWrapper>(sp =>
		{
			var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
			var chatClient = new ChatClient(options.ChatModel, options.ApiKey);

			return new ChatClientWrapper(chatClient);
		});

		builder.Services.AddSingleton<IEmbeddingClientWrapper>(sp =>
		{
			var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
			var embeddingClient = new EmbeddingClient(options.EmbeddingModel, options.ApiKey);

			return new EmbeddingClientWrapper(embeddingClient);
		});

		// Register the AI service as scoped (it depends on repository which is typically scoped)
		builder.Services.AddScoped<IAiService, OpenAiService>();

		return builder;
	}

}
