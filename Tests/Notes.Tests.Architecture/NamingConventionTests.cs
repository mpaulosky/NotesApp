// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NamingConventionTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests for naming conventions.
/// </summary>
[ExcludeFromCodeCoverage]
public class NamingConventionTests : BaseArchitectureTest
{
	[Fact]
	public void InterfacesShouldStartWithI()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.AreInterfaces()
			.Should()
			.HaveNameStartingWith("I")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"interfaces should start with 'I'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void MediatRRequestsShouldHaveCorrectNaming()
	{
		// Commands modify state and should end with Command
		// Queries read state and should end with Query
		// Some utility requests like Seed/Backfill may not follow this convention
		var requests = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequest<>))
			.And()
			.ResideInNamespaceContaining("Features")
			.And()
			.DoNotHaveNameStartingWith("Seed")
			.And()
			.DoNotHaveNameStartingWith("Backfill")
			.GetTypes();

		var invalidNames = new List<string>();

		foreach (var request in requests)
		{
			if (!request.Name.EndsWith("Command") && !request.Name.EndsWith("Query"))
			{
				invalidNames.Add(request.FullName!);
			}
		}

		invalidNames.Should().BeEmpty(
			$"MediatR requests should end with 'Command' or 'Query'. Invalid: {string.Join(", ", invalidNames)}");
	}

	[Fact]
	public void QueriesShouldEndWithQuery()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequest<>))
			.And()
			.ResideInNamespaceContaining("Features")
			.And()
			.DoNotHaveNameEndingWith("Command")
			.Should()
			.HaveNameEndingWith("Query")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"MediatR queries should end with 'Query'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void HandlersShouldEndWithHandler()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ImplementInterface(typeof(MediatR.IRequestHandler<,>))
			.Should()
			.HaveNameEndingWith("Handler")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"MediatR handlers should end with 'Handler'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void RepositoriesShouldEndWithRepository()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Repositories")
			.Should()
			.HaveNameEndingWith("Repository")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"repository classes should end with 'Repository'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServicesShouldEndWithService()
	{
		var result = Types.InAssembly(WebAssembly)
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
			.Should()
			.HaveNameEndingWith("Service")
			.Or()
			.HaveNameEndingWith("Wrapper")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"service classes should end with 'Service' or 'Wrapper'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void EntitiesShouldNotHaveSuffix()
	{
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespaceContaining("Entities")
			.ShouldNot()
			.HaveNameEndingWith("Entity")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"entity classes should not end with 'Entity'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
