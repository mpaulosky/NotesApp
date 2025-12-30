// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IMongoDbContext.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : Note
sSite
// Project Name :  Web
// =======================================================

using MongoDB.Driver;

using Shared.Entities;

namespace Notes.Web.Data;

/// <summary>
///   Interface for MongoDB context using a native driver
/// </summary>
public interface IMongoDbContext
{

	IMongoCollection<Note> Notes { get; }

	IMongoDatabase Database { get; }

}