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
		var extensionClasses = Types.InAssembly(ServiceDefaultsAssembly)
			.That()
			.HaveNameEndingWith("Extensions")
			.GetTypes();

		var invalidExtensions = new List<string>();

		foreach (var extensionClass in extensionClasses)
		{
			if (!extensionClass.IsSealed || !extensionClass.IsAbstract) // static classes are sealed and abstract
			{
				invalidExtensions.Add($"{extensionClass.Name} (not static)");
			}
		}

		invalidExtensions.Should().BeEmpty(
			$"Extension classes should be public and static. Invalid: {string.Join(", ", invalidExtensions)}");
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
		var types = Types.InAssembly(ServiceDefaultsAssembly)
			.That()
			.AreClasses()
			.GetTypes();

		var nonExtensionClasses = types
			.Where(t => !t.Name.EndsWith("Extensions"))
			.Where(t => !t.Name.StartsWith("<")) // Exclude compiler-generated classes
			.Where(t => t.DeclaringType == null || !t.DeclaringType.Name.StartsWith("<")) // Exclude nested types of compiler-generated classes
			.Select(t => t.FullName)
			.ToList();

		nonExtensionClasses.Should().BeEmpty(
			$"All classes in ServiceDefaults should be extension classes. Non-extension classes: {string.Join(", ", nonExtensionClasses)}");
	}
}
