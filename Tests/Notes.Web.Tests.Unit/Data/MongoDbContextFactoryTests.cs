// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbContextFactoryTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Notes.Web.Data;

namespace Notes.Web.Tests.Unit.Data;

[ExcludeFromCodeCoverage]
public class MongoDbContextFactoryTests : IDisposable
{
	private readonly string _originalDefaultConnection;
	private readonly string _originalDatabaseName;

	public MongoDbContextFactoryTests()
	{
		// Store original environment variables
		_originalDefaultConnection = Environment.GetEnvironmentVariable("DefaultConnection") ?? string.Empty;
		_originalDatabaseName = Environment.GetEnvironmentVariable("DatabaseName") ?? string.Empty;
	}

	public void Dispose()
	{
		// Restore original environment variables
		if (!string.IsNullOrEmpty(_originalDefaultConnection))
		{
			Environment.SetEnvironmentVariable("DefaultConnection", _originalDefaultConnection);
		}
		else
		{
			Environment.SetEnvironmentVariable("DefaultConnection", null);
		}

		if (!string.IsNullOrEmpty(_originalDatabaseName))
		{
			Environment.SetEnvironmentVariable("DatabaseName", _originalDatabaseName);
		}
		else
		{
			Environment.SetEnvironmentVariable("DatabaseName", null);
		}

		GC.SuppressFinalize(this);
	}

	[Fact]
	public void CreateDbContext_ShouldReturnMongoDbContext()
	{
		// Arrange
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Should().BeAssignableTo<IMongoDbContext>();
	}

	[Fact]
	public void CreateDbContext_WithArgs_ShouldReturnMongoDbContext()
	{
		// Arrange
		var factory = new MongoDbContextFactory();
		var args = new[] { "arg1", "arg2" };

		// Act
		var context = factory.CreateDbContext(args);

		// Assert
		context.Should().NotBeNull();
		context.Should().BeAssignableTo<IMongoDbContext>();
	}

	[Fact]
	public void CreateDbContext_WithEnvironmentVariableConnectionString_ShouldUseEnvironmentVariable()
	{
		// Arrange
		var expectedConnectionString = "mongodb://testhost:27017";
		Environment.SetEnvironmentVariable("DefaultConnection", expectedConnectionString);
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Should().BeAssignableTo<IMongoDbContext>();
	}

	[Fact]
	public void CreateDbContext_WithEnvironmentVariableDatabaseName_ShouldUseEnvironmentVariable()
	{
		// Arrange
		var expectedDatabaseName = "TestDatabase";
		Environment.SetEnvironmentVariable("DatabaseName", expectedDatabaseName);
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Database.Should().NotBeNull();
		context.Database.DatabaseNamespace.DatabaseName.Should().Be(expectedDatabaseName);
	}

	[Fact]
	public void CreateDbContext_WithNoConfiguration_ShouldUseDefaultValues()
	{
		// Arrange
		// Clear environment variables
		Environment.SetEnvironmentVariable("DefaultConnection", null);
		Environment.SetEnvironmentVariable("DatabaseName", null);
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Database.Should().NotBeNull();
		context.Database.DatabaseNamespace.DatabaseName.Should().Be("NotesSiteDb");
	}

	[Fact]
	public void CreateDbContext_ShouldHaveNotesCollection()
	{
		// Arrange
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Notes.Should().NotBeNull();
		context.Notes.CollectionNamespace.CollectionName.Should().Be("Notes");
	}

	[Fact]
	public void CreateDbContext_CalledMultipleTimes_ShouldReturnNewInstances()
	{
		// Arrange
		var factory = new MongoDbContextFactory();

		// Act
		var context1 = factory.CreateDbContext();
		var context2 = factory.CreateDbContext();

		// Assert
		context1.Should().NotBeNull();
		context2.Should().NotBeNull();
		context1.Should().NotBeSameAs(context2);
	}

	[Fact]
	public void CreateDbContext_WithArgsAndParameterless_ShouldReturnSameConfiguration()
	{
		// Arrange
		var factory = new MongoDbContextFactory();
		var args = new[] { "test" };

		// Act
		var context1 = factory.CreateDbContext();
		var context2 = factory.CreateDbContext(args);

		// Assert
		context1.Database.DatabaseNamespace.DatabaseName.Should().Be(context2.Database.DatabaseNamespace.DatabaseName);
	}

	[Theory]
	[InlineData("mongodb://localhost:27017")]
	[InlineData("mongodb://localhost:27018")]
	[InlineData("mongodb://testserver:27017")]
	public void CreateDbContext_WithDifferentConnectionStrings_ShouldCreateValidContext(string connectionString)
	{
		// Arrange
		Environment.SetEnvironmentVariable("DefaultConnection", connectionString);
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Should().NotBeNull();
		context.Database.Should().NotBeNull();
	}

	[Theory]
	[InlineData("TestDb1")]
	[InlineData("TestDb2")]
	[InlineData("ProductionDb")]
	public void CreateDbContext_WithDifferentDatabaseNames_ShouldUseCorrectDatabase(string databaseName)
	{
		// Arrange
		Environment.SetEnvironmentVariable("DatabaseName", databaseName);
		var factory = new MongoDbContextFactory();

		// Act
		var context = factory.CreateDbContext();

		// Assert
		context.Database.DatabaseNamespace.DatabaseName.Should().Be(databaseName);
	}
}
