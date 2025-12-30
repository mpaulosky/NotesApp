// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Constants.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Constants;

public static class Constants
{

	public const string AdminPolicy = "AdminOnly";

	public const string Cache = "Cache";

	// Backwards-compatible service/resource names moved from the legacy Services class
	public const string Server = "Server";

	// Tests historically expect a `Database` constant with the value "NotesDb".
	public const string Database = "NotesDb";

	public const string DatabaseName = "NotesDb";

	public const string DefaultCorsPolicy = "DefaultPolicy";

	public const string OutputCache = "output-cache";

	public const string ServerName = "noteesite-server";

	public const string RedisCache = "RedisCache";

	public const string Website = "WebApp";

}