// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbContext.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Web
// =======================================================

using MongoDB.Driver;

using Shared.Entities;
using Shared.Interfaces;

namespace Notes.Web.Data;

/// <summary>
///   MongoDB Context using native MongoDB Driver
///   This option provides direct access to MongoDB collections without EF Core overhead
/// </summary>

// ReSharper disable once UnusedType.Global
public class MongoDbContext : IMongoDbContext
{

	private readonly IMongoDatabase _database;

	public MongoDbContext(IMongoClient mongoClient, string databaseName)
	{
		_database = mongoClient.GetDatabase(databaseName);
	}

	public IMongoCollection<Note> Notes => _database.GetCollection<Note>("Notes");
	
	public IMongoDatabase Database => _database;

}