// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ServiceDefaultsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for the ServiceDefaults project.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServiceDefaultsTests : BaseArchitectureTest
{
	[Fact]
	public void ExtensionMethodsShouldBePublicAndStatic()
	{
		var result = Types.InAssembly(ServiceDefaultsAssembly)
			.That()
			.HaveNameEndingWith("Extensions")
			.Should()
			.BeStatic()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Extension classes should be public and static. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ExtensionMethodsShouldBeInCorrectNamespace()
	{
		var result = Types.InAssembly(ServiceDefaultsAssembly)
			.That()
			.HaveNameEndingWith("Extensions")
			.Should()
			.ResideInNamespace("Microsoft.Extensions.Hosting")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Extension classes should be in Microsoft.Extensions.Hosting namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServiceDefaultsShouldNotDependOnWebProject()
	{
		var result = Types.InAssembly(ServiceDefaultsAssembly)
			.Should()
			.NotHaveDependencyOn("Notes.Web")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"ServiceDefaults should not depend on Notes.Web. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServiceDefaultsShouldNotDependOnAppHost()
	{
		var result = Types.InAssembly(ServiceDefaultsAssembly)
			.Should()
			.NotHaveDependencyOn("Notes.AppHost")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"ServiceDefaults should not depend on AppHost. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServiceDefaultsShouldNotDependOnShared()
	{
		var result = Types.InAssembly(ServiceDefaultsAssembly)
			.Should()
			.NotHaveDependencyOn("Shared")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"ServiceDefaults should not depend on Shared. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ServiceDefaultsShouldOnlyContainExtensionClasses()
	{
		// Get all non-compiler-generated classes
		var types = ServiceDefaultsAssembly.GetTypes()
			.Where(t => t.IsClass && 
				!t.Name.Contains("<>") && 
				!t.Name.Contains("+") &&
				!t.IsNested &&
				!t.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated")))
			.ToList();

		var nonExtensionClasses = types
			.Where(t => !t.Name.EndsWith("Extensions"))
			.Select(t => t.FullName)
			.ToList();

		nonExtensionClasses.Should().BeEmpty(
			$"All classes in ServiceDefaults should be extension classes. Non-extension classes: {string.Join(", ", nonExtensionClasses)}");
	}
}
