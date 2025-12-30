// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DependencyTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests for dependency injection patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class DependencyTests : BaseArchitectureTest
{
	[Fact]
	public void SharedShouldNotDependOnNotesWeb()
	{
		var result = Types.InAssembly(SharedAssembly)
			.Should()
			.NotHaveDependencyOn("Notes.Web")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Shared should not depend on Notes.Web. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void SharedShouldNotDependOnServiceDefaults()
	{
		var result = Types.InAssembly(SharedAssembly)
			.Should()
			.NotHaveDependencyOn("Notes.ServiceDefaults")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Shared should not depend on Notes.ServiceDefaults. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void SharedShouldNotDependOnAppHost()
	{
		var result = Types.InAssembly(SharedAssembly)
			.Should()
			.NotHaveDependencyOn("Notes.AppHost")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Shared should not depend on Notes.AppHost. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void EntitiesShouldNotDependOnFeatures()
	{
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespaceContaining("Entities")
			.Should()
			.NotHaveDependencyOn("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"entities should not depend on features. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void FeaturesShouldNotDependOnComponents()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Features")
			.Should()
			.NotHaveDependencyOn("Components")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"features should not depend on UI components. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ComponentsShouldNotDependOnData()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.Should()
			.NotHaveDependencyOn("Data")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"UI components should not directly depend on data layer. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void MediatRHandlersShouldHaveCorrectDependencies()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequestHandler<,>))
			.Should()
			.ResideInNamespaceContaining("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"MediatR handlers should reside in Features namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void RepositoriesShouldBeInCorrectNamespace()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Repositories")
			.Should()
			.ResideInNamespace("Notes.Web.Data.Repositories")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"repositories should be in Data.Repositories namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServicesShouldNotDependOnFeatures()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Services")
			.And()
			.AreClasses()
			.Should()
			.NotHaveDependencyOn("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"services should not depend on features. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
