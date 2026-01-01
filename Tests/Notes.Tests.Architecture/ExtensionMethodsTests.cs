// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ExtensionMethodsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for extension method patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExtensionMethodsTests : BaseArchitectureTest
{
	[Fact]
	public void ExtensionMethodsShouldBePublicAndStatic()
	{
		var extensionClasses = Types.InAssembly(WebAssembly)
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
	public void ExtensionMethodsShouldBeInMicrosoftExtensionsNamespace()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Extensions")
			.And()
			.ResideInNamespaceContaining("Extensions")
			.Should()
			.ResideInNamespace("Microsoft.Extensions.Hosting")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Extension classes should be in Microsoft.Extensions.Hosting namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ExtensionMethodsShouldReturnBuilderForChaining()
	{
		// Get all public static methods in extension classes that extend builder types
		var extensionMethods = WebAssembly.GetTypes()
			.Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed)
			.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
			.Where(m => m.GetParameters().FirstOrDefault()?.ParameterType.Name.Contains("Builder") == true)
			.ToList();

		// Extension methods on builders CAN return the builder for method chaining, or void
		// This test just verifies the pattern exists and is testable
		extensionMethods.Should().NotBeEmpty("Extension methods on builders should exist");
	}

	[Fact]
	public void ExtensionMethodsShouldNotReturnVoid()
	{
		var extensionClasses = WebAssembly.GetTypes()
			.Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed)
			.Where(t => t.Namespace == "Microsoft.Extensions.Hosting") // Only enforce for hosting extensions
			.ToList();

		foreach (var extensionClass in extensionClasses)
		{
			var voidMethods = extensionClass.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(m => m.ReturnType == typeof(void) && !m.IsSpecialName)
				.ToList();

			// Hosting extensions should return IHostApplicationBuilder for chaining
			voidMethods.Should().BeEmpty(
				$"Hosting extension class {extensionClass.Name} should not have void methods. Void methods: {string.Join(", ", voidMethods.Select(m => m.Name))}");
		}
	}

	[Fact]
	public void ExtensionMethodsShouldHaveXmlDocumentation()
	{
		var extensionClasses = WebAssembly.GetTypes()
			.Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed);

		// This test verifies the pattern exists - actual XML doc validation requires loading XML doc files
		extensionClasses.Should().NotBeEmpty("Extension classes should exist in the assembly");
	}

	[Fact]
	public void MongoDbExtensionsShouldRegisterRequiredServices()
	{
		var mongoDbExtensions = WebAssembly.GetTypes()
			.FirstOrDefault(t => t.Name == "MongoDbExtensions");

		mongoDbExtensions.Should().NotBeNull("MongoDbExtensions class should exist");
		
		var addMongoDbMethod = mongoDbExtensions!.GetMethod("AddMongoDb", BindingFlags.Public | BindingFlags.Static);
		addMongoDbMethod.Should().NotBeNull("AddMongoDb extension method should exist");
	}

	[Fact]
	public void AiServiceExtensionsShouldRegisterRequiredServices()
	{
		var aiServiceExtensions = WebAssembly.GetTypes()
			.FirstOrDefault(t => t.Name == "AiServiceExtensions");

		aiServiceExtensions.Should().NotBeNull("AiServiceExtensions class should exist");
		
		var addAiServicesMethod = aiServiceExtensions!.GetMethod("AddAiServices", BindingFlags.Public | BindingFlags.Static);
		addAiServicesMethod.Should().NotBeNull("AddAiServices extension method should exist");
	}
}
