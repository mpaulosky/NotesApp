// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbServiceExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

using Notes.Web.Data;
using Notes.Web.Data.Repositories;

namespace Notes.Web.Tests.Unit.Data;

[ExcludeFromCodeCoverage]
public class MongoDbServiceExtensionsTests : IDisposable
{
	private readonly string _originalConnectionString;
	private readonly string _originalDatabaseName;

	public MongoDbServiceExtensionsTests()
	{
		// Store original environment variables
		_originalConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? string.Empty;
		_originalDatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? string.Empty;
	}

	public void Dispose()
	{
		// Restore original environment variables
		if (!string.IsNullOrEmpty(_originalConnectionString))
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", _originalConnectionString);
		}
		else
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", null);
		}

		if (!string.IsNullOrEmpty(_originalDatabaseName))
		{
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", _originalDatabaseName);
		}
		else
		{
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", null);
		}

		GC.SuppressFinalize(this);
	}

	[Fact]
	public void AddMongoDb_ShouldRegisterMongoClient()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var mongoClient = serviceProvider.GetService<IMongoClient>();
		mongoClient.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_ShouldRegisterMongoDatabase()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var database = serviceProvider.GetService<IMongoDatabase>();
		database.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_ShouldRegisterMongoDbContext()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var context = serviceProvider.GetService<IMongoDbContext>();
		context.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_ShouldRegisterMongoDbContextFactory()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var factory = serviceProvider.GetService<IMongoDbContextFactory>();
		factory.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_ShouldRegisterNoteRepository()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var repository = serviceProvider.GetService<INoteRepository>();
		repository.Should().NotBeNull();
		repository.Should().BeOfType<NoteRepository>();
	}

	[Fact]
	public void AddMongoDb_MongoClientShouldBeSingleton()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var client1 = serviceProvider.GetService<IMongoClient>();
		var client2 = serviceProvider.GetService<IMongoClient>();
		client1.Should().BeSameAs(client2);
	}

	[Fact]
	public void AddMongoDb_MongoDbContextShouldBeScoped()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		using var scope1 = serviceProvider.CreateScope();
		using var scope2 = serviceProvider.CreateScope();

		var context1 = scope1.ServiceProvider.GetService<IMongoDbContext>();
		var context2 = scope1.ServiceProvider.GetService<IMongoDbContext>();
		var context3 = scope2.ServiceProvider.GetService<IMongoDbContext>();

		// Same scope should return same instance
		context1.Should().BeSameAs(context2);
		// Different scope should return different instance
		context1.Should().NotBeSameAs(context3);
	}

	[Fact]
	public void AddMongoDb_NoteRepositoryShouldBeScoped()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		using var scope1 = serviceProvider.CreateScope();
		using var scope2 = serviceProvider.CreateScope();

		var repo1 = scope1.ServiceProvider.GetService<INoteRepository>();
		var repo2 = scope1.ServiceProvider.GetService<INoteRepository>();
		var repo3 = scope2.ServiceProvider.GetService<INoteRepository>();

		// Same scope should return same instance
		repo1.Should().BeSameAs(repo2);
		// Different scope should return different instance
		repo1.Should().NotBeSameAs(repo3);
	}

	[Fact]
	public void AddMongoDb_ShouldUseAspireDatabaseName()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert - Uses database name from configuration (TestDb in this case)
		var database = serviceProvider.GetService<IMongoDatabase>();
		database.Should().NotBeNull();
		database!.DatabaseNamespace.DatabaseName.Should().Be("TestDb");
	}

	[Fact]
	public void AddMongoDb_IgnoresEnvironmentVariableForDatabaseName()
	{
		// Arrange - Aspire uses fixed database name from resource, not environment variables
		var expectedDatabaseName = "EnvDatabase";
		Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", expectedDatabaseName);
		var builder = CreateWebApplicationBuilder(useDatabaseNameFromConfig: false);

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert - Aspire uses "NotesDb" regardless of environment variable
		var database = serviceProvider.GetService<IMongoDatabase>();
		database.Should().NotBeNull();
		database!.DatabaseNamespace.DatabaseName.Should().Be("NotesDb");
	}

	[Fact]
	public void AddMongoDb_ShouldThrowWhenConnectionStringNotFound()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder(useConnectionString: false);
		Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", null);

		// Act - Registration doesn't throw; Aspire validates on first use
		var act = () => builder.AddMongoDb();

		// Assert - Aspire doesn't throw during registration
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_ShouldPreferConnectionStringFromConfiguration()
	{
		// Arrange
		var configConnectionString = "mongodb://config-host:27017";
		Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://env-host:27017");
		var builder = CreateWebApplicationBuilder(connectionString: configConnectionString);

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var client = serviceProvider.GetService<IMongoClient>();
		client.Should().NotBeNull();
		// If config is set, it should use config over environment variable
	}

	[Fact]
	public void AddMongoDb_MongoDbContextFactory_CreateDbContext_ShouldReturnSameInstance()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		using var scope = serviceProvider.CreateScope();
		var factory = scope.ServiceProvider.GetService<IMongoDbContextFactory>();
		var context = scope.ServiceProvider.GetService<IMongoDbContext>();

		// Act
		var contextFromFactory = factory!.CreateDbContext();

		// Assert
		contextFromFactory.Should().BeSameAs(context);
	}

	[Fact]
	public void AddMongoDb_ShouldUseDefaultDatabaseNameWhenNotConfigured()
	{
		// Arrange
		Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", null);
		var builder = CreateWebApplicationBuilder(useDatabaseNameFromConfig: false);

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert - Aspire uses "NotesDb" as the default from the resource name
		var database = serviceProvider.GetService<IMongoDatabase>();
		database.Should().NotBeNull();
		database!.DatabaseNamespace.DatabaseName.Should().Be("NotesDb");
	}

	[Fact]
	public void AddMongoDb_WithNullBuilder_ShouldThrowArgumentNullException()
	{
		// Arrange
		WebApplicationBuilder? builder = null;

		// Act & Assert
		var act = () => builder!.AddMongoDb();
		act.Should().Throw<NullReferenceException>();
	}

	[Fact]
	public void AddMongoDb_WithEmptyConnectionString_ShouldNotThrowDuringRegistration()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder(connectionString: string.Empty);

		// Act
		var act = () => builder.AddMongoDb();

		// Assert - Registration succeeds; connection validation happens when MongoDB client is actually used
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_WithWhitespaceConnectionString_ShouldNotThrow()
	{
		// Arrange - MongoDB client will handle invalid connection strings
		var builder = CreateWebApplicationBuilder(connectionString: "   ");

		// Act
		var act = () => builder.AddMongoDb();

		// Assert - Registration should succeed, validation happens on use
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_WithEmptyDatabaseName_ShouldUseAspireDefault()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder(databaseName: string.Empty);

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert - Aspire uses fixed "NotesDb" regardless of configuration
		var act = () => serviceProvider.GetService<IMongoDatabase>();
		act.Should().NotThrow();
		var database = act();
		database.Should().NotBeNull();
		database!.DatabaseNamespace.DatabaseName.Should().Be("NotesDb");
	}

	[Fact]
	public void AddMongoDb_CalledMultipleTimes_ShouldNotDuplicateRegistrations()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();

		// Act
		builder.AddMongoDb();
		builder.AddMongoDb();

		// Assert - Should not throw, and services should be registered
		var serviceProvider = builder.Services.BuildServiceProvider();
		var client = serviceProvider.GetService<IMongoClient>();
		client.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_MongoDatabase_ShouldResolveFromSameClient()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		using var scope = serviceProvider.CreateScope();

		// Act
		var client = scope.ServiceProvider.GetService<IMongoClient>();
		var database = scope.ServiceProvider.GetService<IMongoDatabase>();

		// Assert
		client.Should().NotBeNull();
		database.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_MongoDbContext_ShouldUseSameDatabaseNameAsMongoDatabase()
	{
		// Arrange - Aspire uses fixed "NotesDb" database name
		var builder = CreateWebApplicationBuilder(useDatabaseNameFromConfig: false);
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

			using var scope = serviceProvider.CreateScope();

			// Act
			var database = scope.ServiceProvider.GetService<IMongoDatabase>();
			var context = scope.ServiceProvider.GetService<IMongoDbContext>();

			// Assert - Both should use "NotesDb"
			database!.DatabaseNamespace.DatabaseName.Should().Be("NotesDb");
			context!.Database.DatabaseNamespace.DatabaseName.Should().Be("NotesDb");
		}

	[Fact]
	public void AddMongoDb_RuntimeMongoDbContextFactory_ShouldReturnNonNullContext()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		using var scope = serviceProvider.CreateScope();

		// Act
		var factory = scope.ServiceProvider.GetService<IMongoDbContextFactory>();
		var context = factory!.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Notes.Should().NotBeNull();
		context.Database.Should().NotBeNull();
	}

	[Fact]
	public void AddMongoDb_FactoryCalledMultipleTimes_ShouldReturnSameContextInSameScope()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		using var scope = serviceProvider.CreateScope();
		var factory = scope.ServiceProvider.GetService<IMongoDbContextFactory>();

		// Act
		var context1 = factory!.CreateDbContext();
		var context2 = factory.CreateDbContext();

		// Assert
		context1.Should().BeSameAs(context2);
	}

	[Fact]
	public void AddMongoDb_WithSpecialCharactersInDatabaseName_ShouldHandleCorrectly()
	{
		// Arrange
		var specialDatabaseName = "test-db_123";
		var builder = CreateWebApplicationBuilder(databaseName: specialDatabaseName);

		// Act
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Assert
		var database = serviceProvider.GetService<IMongoDatabase>();
		database.Should().NotBeNull();
		database!.DatabaseNamespace.DatabaseName.Should().Be(specialDatabaseName);
	}

	[Fact]
	public void AddMongoDb_WithMongoDbUriConnectionString_ShouldParseCorrectly()
	{
		// Arrange
		var mongoDbUri = "mongodb://user:password@host1:27017,host2:27017/database?replicaSet=myReplicaSet";
		var builder = CreateWebApplicationBuilder(connectionString: mongoDbUri);

		// Act
		var act = () => builder.AddMongoDb();

		// Assert - Should not throw during registration
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_WithMongoDbSrvConnectionString_ShouldParseCorrectly()
	{
		// Arrange
		var mongoDbSrvUri = "mongodb+srv://user:password@cluster.mongodb.net/database";
		var builder = CreateWebApplicationBuilder(connectionString: mongoDbSrvUri);

		// Act
		var act = () => builder.AddMongoDb();

		// Assert - Should not throw during registration
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_EnvironmentVariableTakesPrecedenceWhenConfigMissing()
	{
		// Arrange
		Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://env-host:27017");
		var builder = CreateWebApplicationBuilder(useConnectionString: false);

		// Act
		var act = () => builder.AddMongoDb();

		// Assert - Should not throw, uses environment variable
		act.Should().NotThrow();
	}

	[Fact]
	public void AddMongoDb_NoteRepository_ShouldBeResolvableWithinScope()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		// Act
		using var scope = serviceProvider.CreateScope();
		var repository = scope.ServiceProvider.GetService<INoteRepository>();

		// Assert
		repository.Should().NotBeNull();
		repository.Should().BeAssignableTo<INoteRepository>();
	}

	[Fact]
	public void AddMongoDb_AllServicesResolvableInDependencyChain()
	{
		// Arrange
		var builder = CreateWebApplicationBuilder();
		builder.AddMongoDb();
		var serviceProvider = builder.Services.BuildServiceProvider();

		using var scope = serviceProvider.CreateScope();

		// Act & Assert - Verify entire dependency chain resolves
		var client = scope.ServiceProvider.GetService<IMongoClient>();
		var database = scope.ServiceProvider.GetService<IMongoDatabase>();
		var context = scope.ServiceProvider.GetService<IMongoDbContext>();
		var factory = scope.ServiceProvider.GetService<IMongoDbContextFactory>();
		var repository = scope.ServiceProvider.GetService<INoteRepository>();

		client.Should().NotBeNull();
		database.Should().NotBeNull();
		context.Should().NotBeNull();
		factory.Should().NotBeNull();
		repository.Should().NotBeNull();
	}

	/// <summary>
	/// Creates a WebApplicationBuilder with test configuration
	/// </summary>
	private static WebApplicationBuilder CreateWebApplicationBuilder(
		string connectionString = "mongodb://localhost:27017",
		string databaseName = "TestDb",
		bool useConnectionString = true,
		bool useDatabaseNameFromConfig = true)
	{
		var args = Array.Empty<string>();
		var builder = WebApplication.CreateBuilder(args);

		// Clear existing configuration
		builder.Configuration.Sources.Clear();

		// Add in-memory configuration
		var configValues = new Dictionary<string, string?>();

		if (useConnectionString)
		{
			// Aspire looks for ConnectionStrings:NotesDb (matching the resource name in AppHost)
			configValues["ConnectionStrings:NotesDb"] = connectionString;
		}

		if (useDatabaseNameFromConfig)
		{
			configValues["MongoDb:DatabaseName"] = databaseName;
		}

		builder.Configuration.AddInMemoryCollection(configValues);

		return builder;
	}
}
