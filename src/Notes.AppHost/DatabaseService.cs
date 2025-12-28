// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DatabaseService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.AppHost
// =======================================================

namespace Notes.AppHost;

/// <summary>
///   Extension methods for adding and configuring MongoDB resources with Aspire 9.4.0 features.
/// </summary>
public static class DatabaseService
{
	// /// <summary>
	// ///   Adds MongoDB services to the distributed application builder, including resource tagging, grouping, and improved
	// ///   seeding logic.
	// /// </summary>
	// /// <param name="builder">The distributed application builder.</param>
	// /// <returns>The MongoDB database resource builder.</returns>
	// public static IResourceBuilder<MongoDBDatabaseResource> AddMongoDbServices(
	// 	this IDistributedApplicationBuilder builder)
	// {
	//
	// 	var mongoDbConnection = builder.AddParameter("mongoDb-connection", secret: true);
	// 	var databaseName = builder.AddParameter("mongoDb-database", secret: true);
	//
	// 	// Use a valid resource name, not the connection string
	// 	var server = builder.AddMongoDB(Server)
	// 			.WithLifetime(ContainerLifetime.Persistent)
	// 			.WithDataVolume($"{Server}-data", isReadOnly: false)
	// 			.WithEnvironment("MONGODB-CONNECTION-STRING", mongoDbConnection)
	// 			.WithEnvironment("MONGODB-DATABASE-NAME", databaseName)
	// 			.WithMongoExpress();
	//
	// 	var database = server.AddDatabase(DatabaseName);
	//
	// 	return database;
	// }

	/// <summary>
	///   Adds MongoDB Atlas (cloud) connection to the distributed application builder.
	/// </summary>
	/// <param name="builder">The distributed application builder.</param>
	/// <returns>The MongoDB server resource builder.</returns>
	public static IResourceBuilder<MongoDBServerResource> AddMongoDbAtlas(
		this IDistributedApplicationBuilder builder)
	{
		// Connection string from user secrets or configuration
		var mongoServer = builder.AddMongoDB(ServerName)
			.PublishAsConnectionString();

		return mongoServer;
	}

	/// <summary>
	///   Adds MongoDB Atlas database reference.
	/// </summary>
	/// <param name="builder">The distributed application builder.</param>
	/// <param name="databaseName">The name of the database. Defaults to "NotesDb".</param>
	/// <returns>The MongoDB database resource builder.</returns>
	public static IResourceBuilder<MongoDBDatabaseResource> AddMongoDbAtlasDatabase(
		this IDistributedApplicationBuilder builder,
		string databaseName = "NotesDb")
	{
		var server = builder.AddMongoDbAtlas();
		return server.AddDatabase(databaseName);
	}
}