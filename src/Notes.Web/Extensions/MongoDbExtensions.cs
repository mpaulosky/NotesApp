// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Web
// =======================================================
namespace Microsoft.Extensions.Hosting;

/// <summary>
///   Extension methods for configuring MongoDB services.
/// </summary>
public static class MongoDbExtensions
{

	/// <summary>
	///   Adds MongoDB services including database context and repositories.
	/// </summary>
	/// <param name="builder">The web application builder.</param>
	/// <returns>The web application builder for chaining.</returns>
	public static IHostApplicationBuilder AddMongoDb(this IHostApplicationBuilder builder)
	{
		// Register MongoDB database context
		builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();

		// Register repositories
		builder.Services.AddScoped<INoteRepository, NoteRepository>();

		// Add MongoDB health check
		//builder.Services.AddHealthChecks()
		//	.AddMongoDb(
		//		connectionString: "mongodb://localhost:27017",
		//		name: "mongodb",
		//		timeout: TimeSpan.FromSeconds(5),
		//		tags: new[] { "db", "mongo", "mongodb" });

		return builder;
	}

}
