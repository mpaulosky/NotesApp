// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Web
// =======================================================

using Shared.Interfaces;

using MongoDB.Driver;

using Notes.Web.Data.Repositories;

namespace Notes.Web.Data;

/// <summary>
///   Provides extension methods for registering MongoDB-related services, context factories, and repositories
///   in a Blazor/ASP.NET Core application using Aspire service discovery and Aspire.MongoDB.Driver.
/// </summary>
public static class MongoDbServiceExtensions
{

	/// <summary>
	///   Adds MongoDB services to the application's dependency injection container using Aspire service discovery and
	///   Aspire.MongoDB.Driver.
	///   Registers IMongoClient, IMongoDatabase, MongoDbContext, and related repositories and handlers.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder" /> to configure.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder" /> for chaining.</returns>
	public static void AddMongoDb(this WebApplicationBuilder builder)
	{
		IServiceCollection services = builder.Services;
		var configuration = builder.Configuration;

		// Get MongoDB connection string from an environment variable or configuration
		string connectionString = configuration["ConnectionStrings:articlesdb"] ??
															Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ??
															throw new InvalidOperationException("MongoDB connection string not found.");

		string databaseName = configuration["MongoDb:DatabaseName"] ??
													Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? "articlesdb";

		// Register IMongoClient and IMongoDatabase manually
		services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

		services.AddScoped(sp =>
		{
			IMongoClient client = sp.GetRequiredService<IMongoClient>();

			return client.GetDatabase(databaseName);
		});

		services.AddScoped<IMongoDbContext>(sp =>
		{
			IMongoClient client = sp.GetRequiredService<IMongoClient>();
			IMongoDatabase database = sp.GetRequiredService<IMongoDatabase>();

			return new MongoDbContext(client, database.DatabaseNamespace.DatabaseName);
		});

		services.AddScoped<IMongoDbContextFactory>(sp =>
		{
			IMongoDbContext context = sp.GetRequiredService<IMongoDbContext>();

			return new RuntimeMongoDbContextFactory(context);
		});

		RegisterRepositoriesAndHandlers(services);

	}

	/// <summary>
	///   Registers MongoDB repositories and CQRS handlers for articles and categories.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection" /> to register services with.</param>
	private static void RegisterRepositoriesAndHandlers(IServiceCollection services)
	{

		// Register repositories
		services.AddScoped<INoteRepository, NoteRepository>();

		// Register services
		services.AddScoped<INoteService, NoteService>();

	}

	/// <summary>
	///   Runtime adapter that wraps the DI-resolved <see cref="IMongoDbContext" /> for use with
	///   <see cref="IMongoDbContextFactory" />.
	/// </summary>
	private sealed class RuntimeMongoDbContextFactory : IMongoDbContextFactory
	{

		private readonly IMongoDbContext _context;

		/// <summary>
		///   Initializes a new instance of the <see cref="RuntimeMongoDbContextFactory" /> class.
		/// </summary>
		/// <param name="context">The DI-resolved <see cref="IMongoDbContext" /> instance.</param>
		public RuntimeMongoDbContextFactory(IMongoDbContext context)
		{
			_context = context;
		}

		/// <summary>
		///   Returns the DI-resolved <see cref="IMongoDbContext" /> instance.
		/// </summary>
		/// <returns>The <see cref="IMongoDbContext" /> instance.</returns>
		public IMongoDbContext CreateDbContext()
		{
			// Return the context directly
			return _context;
		}

	}

}