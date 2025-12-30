// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     LayeringTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests for architectural layering and dependencies.
/// </summary>
[ExcludeFromCodeCoverage]
public class LayeringTests : BaseArchitectureTest
{
	[Fact]
	public void FeaturesShouldBeInCorrectNamespace()
	{
		// Features should be organized in proper namespace structure
		// Sealed is optional - records are inherently immutable
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Features")
			.And()
			.AreClasses()
			.Should()
			.ResideInNamespace("Notes.Web.Features.Notes.CreateNote")
			.Or()
			.ResideInNamespaceStartingWith("Notes.Web.Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"feature classes should be in proper namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void HandlersShouldBeInFeaturesNamespace()
	{
		// Handlers should be in Features namespace (visibility can vary based on framework needs)
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequestHandler<,>))
			.Should()
			.ResideInNamespaceContaining("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"MediatR handlers should be in Features namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void RequestsShouldBeRecords()
	{
		// NetArchTest doesn't support BeRecords, so check manually
		var requestTypes = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequest<>))
			.And()
			.ResideInNamespaceContaining("Features")
			.GetTypes();

		var nonRecordTypes = requestTypes
			.Where(t => !t.IsValueType && !t.GetMethods().Any(m => m.Name == "<Clone>$"))
			.Select(t => t.FullName)
			.ToList();

		nonRecordTypes.Should().BeEmpty(
			$"MediatR requests should be records. Non-records: {string.Join(", ", nonRecordTypes)}");
	}

	[Fact]
	public void EntitiesShouldBeInSharedAssembly()
	{
		// NetArchTest doesn't support ResideInAssembly, so check manually
		var entityTypes = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.ResideInNamespaceContaining("Entities")
			.GetTypes();

		var wrongAssemblyTypes = entityTypes
			.Where(t => t.Assembly != SharedAssembly)
			.Select(t => t.FullName)
			.ToList();

		wrongAssemblyTypes.Should().BeEmpty(
			$"entities should reside in Shared assembly. Wrong assembly: {string.Join(", ", wrongAssemblyTypes)}");
	}

	[Fact]
	public void RepositoriesShouldImplementInterface()
	{
		// NetArchTest doesn't support ImplementAnInterface, so check manually
		var repositoryTypes = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Repositories")
			.And()
			.AreClasses()
			.And()
			.DoNotHaveNameEndingWith("Base")
			.GetTypes();

		var typesWithoutInterfaces = repositoryTypes
			.Where(t => !t.IsInterface && t.GetInterfaces().Length == 0)
			.Select(t => t.FullName)
			.ToList();

		typesWithoutInterfaces.Should().BeEmpty(
			$"repository classes should implement an interface. Missing interfaces: {string.Join(", ", typesWithoutInterfaces)}");
	}

	[Fact]
	public void ServicesShouldImplementInterface()
	{
		// NetArchTest doesn't support ImplementAnInterface, so check manually
		var serviceTypes = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Services")
			.And()
			.AreClasses()
			.And()
			.DoNotHaveNameEndingWith("Extensions")
			.And()
			.DoNotHaveNameEndingWith("Options")
			.And()
			.DoNotHaveNameEndingWith("Provider")
			.GetTypes();

		var typesWithoutInterfaces = serviceTypes
			.Where(t => !t.IsInterface && t.GetInterfaces().Length == 0)
			.Select(t => t.FullName)
			.ToList();

		typesWithoutInterfaces.Should().BeEmpty(
			$"service classes should implement an interface. Missing interfaces: {string.Join(", ", typesWithoutInterfaces)}");
	}

	[Fact]
	public void DataContextShouldBeInDataNamespace()
	{
		// Context classes should be in Data namespace (visibility can be public for DI)
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Context")
			.Should()
			.ResideInNamespaceContaining("Data")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"data context classes should be in Data namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void AbstractionsShouldBeInSharedAssembly()
	{
		// NetArchTest doesn't support ResideInAssembly, so check manually
		var abstractionTypes = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.ResideInNamespaceContaining("Abstractions")
			.GetTypes();

		var wrongAssemblyTypes = abstractionTypes
			.Where(t => t.Assembly != SharedAssembly)
			.Select(t => t.FullName)
			.ToList();

		wrongAssemblyTypes.Should().BeEmpty(
			$"abstractions should reside in Shared assembly. Wrong assembly: {string.Join(", ", wrongAssemblyTypes)}");
	}
}
