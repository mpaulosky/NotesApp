// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DataLayerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests for data layer patterns specific to MongoDB implementation.
/// </summary>
[ExcludeFromCodeCoverage]
public class DataLayerTests : BaseArchitectureTest
{
	[Fact]
	public void RepositoriesShouldBeInRepositoriesNamespace()
	{
		var repositories = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Repository")
			.And()
			.AreClasses()
			.GetTypes();

		var repositoriesInWrongNamespace = repositories
			.Where(r => !r.Namespace!.Contains("Repositories"))
			.Select(r => r.FullName)
			.ToList();

		repositoriesInWrongNamespace.Should().BeEmpty(
			$"repository classes should be in Repositories namespace. Wrong namespace: {string.Join(", ", repositoriesInWrongNamespace)}");
	}

	[Fact]
	public void RepositoryInterfacesShouldBeInSharedProject()
	{
		var repositoryInterfaces = Types.InAssembly(SharedAssembly)
			.That()
			.AreInterfaces()
			.And()
			.HaveNameEndingWith("Repository")
			.GetTypes();

		repositoryInterfaces.Should().NotBeEmpty(
			"repository interfaces should be defined in Shared project for abstraction");

		foreach (var repoInterface in repositoryInterfaces)
		{
			repoInterface.Assembly.GetName().Name.Should().Be(SharedAssembly.GetName().Name,
				$"{repoInterface.Name} should be in Shared assembly");
		}
	}

	[Fact]
	public void MongoDbContextShouldNotBeUsedDirectlyInFeatures()
	{
		var featureTypes = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Features")
			.GetTypes();

		var featuresUsingContext = new List<string>();

		foreach (var feature in featureTypes)
		{
			var constructors = feature.GetConstructors();
			foreach (var ctor in constructors)
			{
				var parameters = ctor.GetParameters();
				if (parameters.Any(p => p.ParameterType.Name.Contains("MongoDbContext")))
				{
					featuresUsingContext.Add(feature.FullName!);
					break;
				}
			}
		}

		featuresUsingContext.Should().BeEmpty(
			$"features should use repositories, not MongoDbContext directly. Violations: {string.Join(", ", featuresUsingContext)}");
	}

	[Fact]
	public void DataClassesShouldBeInDataNamespace()
	{
		var dataClasses = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Context")
			.Or()
			.HaveNameEndingWith("ContextFactory")
			.GetTypes();

		var dataClassesInWrongNamespace = dataClasses
			.Where(d => !d.Namespace!.Contains("Data"))
			.Select(d => d.FullName)
			.ToList();

		dataClassesInWrongNamespace.Should().BeEmpty(
			$"data infrastructure classes should be in Data namespace. Wrong namespace: {string.Join(", ", dataClassesInWrongNamespace)}");
	}

	[Fact]
	public void RepositoriesShouldNotReturnDbTypes()
	{
		// Repositories should return domain entities, not MongoDB-specific types
		var repositoryTypes = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Repository")
			.And()
			.AreClasses()
			.GetTypes();

		var repositoriesReturningDbTypes = new List<string>();

		foreach (var repository in repositoryTypes)
		{
			var methods = repository.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Instance);

			foreach (var method in methods)
			{
				if (method.ReturnType.Namespace?.Contains("MongoDB") == true)
				{
					repositoriesReturningDbTypes.Add($"{repository.Name}.{method.Name} returns {method.ReturnType.Name}");
				}
			}
		}

		repositoriesReturningDbTypes.Should().BeEmpty(
			$"repositories should return domain entities, not MongoDB types. Violations: {string.Join("; ", repositoriesReturningDbTypes)}");
	}

	[Fact]
	public void EntitiesShouldHaveBsonAttributes()
	{
		// MongoDB entities should have proper BSON serialization attributes
		var entities = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespaceContaining("Entities")
			.And()
			.AreClasses()
			.GetTypes();

		var entitiesWithoutBsonAttributes = new List<string>();

		foreach (var entity in entities)
		{
			// Skip DTOs that aren't actually MongoDB entities (like AppUser)
			if (entity.Name == "AppUser")
				continue;

			// Check for any BSON attribute on the entity or its properties
			var hasAnyBsonAttribute = entity.GetProperties()
				.Any(p => p.GetCustomAttributes(false)
					.Any(a => a.GetType().Namespace?.Contains("MongoDB.Bson") == true));

			if (!hasAnyBsonAttribute)
			{
				entitiesWithoutBsonAttributes.Add(entity.Name);
			}
		}

		entitiesWithoutBsonAttributes.Should().BeEmpty(
			$"MongoDB entities should have BSON attributes. Missing: {string.Join(", ", entitiesWithoutBsonAttributes)}");
	}

	[Fact]
	public void DataNamespaceShouldNotDependOnComponents()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Data")
			.Should()
			.NotHaveDependencyOn("Components")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"data layer should not depend on UI components. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
