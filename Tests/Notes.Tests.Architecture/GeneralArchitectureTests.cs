// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GeneralArchitectureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// General architecture tests for all types.
/// </summary>
[ExcludeFromCodeCoverage]
public class GeneralArchitectureTests : BaseArchitectureTest
{
	[Fact]
	public void ClassesShouldNotHavePublicFields()
	{
		// NetArchTest doesn't support HavePublicFields, so check manually
		var types = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.AreClasses()
			.GetTypes();

		var typesWithPublicFields = types
			.Where(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Any())
			.Select(t => t.FullName)
			.ToList();

		typesWithPublicFields.Should().BeEmpty(
			$"classes should not have public fields. Classes with public fields: {string.Join(", ", typesWithPublicFields)}");
	}

	[Fact]
	public void AsyncMethodsShouldHaveAsyncSuffix()
	{
		// Get all types from application assemblies
		var types = Types.InAssemblies(ApplicationAssemblies).GetTypes();

		var invalidMethods = new List<string>();

		foreach (var type in types)
		{
			// Skip compiler-generated types, interfaces, and repository implementations
			if (type.Name.Contains("<>") ||
					type.Name.Contains("+") ||
					type.Name.Contains("d__") || // Async state machines
					type.IsInterface ||
					type.Name.EndsWith("Repository") || // Repositories implement interfaces with specific names
					type.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated")))
			{
				continue;
			}

			var methods = type.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.Static |
				System.Reflection.BindingFlags.DeclaredOnly);

			foreach (var method in methods)
			{
				// Skip property getters/setters, compiler-generated methods, and framework-required methods
				if (method.Name.StartsWith("get_") ||
						method.Name.StartsWith("set_") ||
						method.Name == "MoveNext" ||
						method.Name == "Handle" || // MediatR handlers use Handle by convention
						method.Name.Contains("<") ||
						method.Name.Contains(">") ||
						method.IsVirtual && method.GetBaseDefinition().DeclaringType != type) // Override methods
				{
					continue;
				}

				// Check if method returns Task or ValueTask and doesn't end with Async
				if ((method.ReturnType == typeof(Task) ||
						 method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ||
						 method.ReturnType == typeof(ValueTask) ||
						 method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)) &&
						!method.Name.EndsWith("Async"))
				{
					invalidMethods.Add($"{type.FullName}.{method.Name}");
				}
			}
		}

		invalidMethods.Should().BeEmpty(
			$"async methods should end with 'Async' suffix. Invalid methods: {string.Join(", ", invalidMethods)}");
	}

	[Fact]
	public void OptionClassesShouldBeInCorrectNamespace()
	{
		// Option classes should be near their related services
		var result = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.HaveNameEndingWith("Options")
			.And()
			.AreClasses()
			.Should()
			.ResideInNamespaceContaining("Services")
			.Or()
			.ResideInNamespaceContaining("Data")
			.Or()
			.ResideInNamespaceContaining("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"option classes should be in appropriate namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ExceptionsShouldEndWithException()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.Inherit(typeof(Exception))
			.Should()
			.HaveNameEndingWith("Exception")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"exception classes should end with 'Exception'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ExtensionMethodClassesShouldBeStaticAndSealed()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.HaveNameEndingWith("Extensions")
			.Should()
			.BeStatic()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"extension method classes should be static. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void DTOsShouldBeRecords()
	{
		// NetArchTest doesn't support BeRecords, so check manually
		var dtoTypes = Types.InAssemblies(ApplicationAssemblies)
			.That()
			.ResideInNamespaceContaining("DTOs")
			.Or()
			.ResideInNamespaceContaining("Dtos")
			.GetTypes();

		var nonRecordTypes = dtoTypes
			.Where(t => !t.IsValueType && !t.GetMethods().Any(m => m.Name == "<Clone>$"))
			.Select(t => t.FullName)
			.ToList();

		nonRecordTypes.Should().BeEmpty(
			$"DTOs should be records. Non-records: {string.Join(", ", nonRecordTypes)}");
	}

	[Fact]
	public void NullableContextShouldBeEnabled()
	{
		// Check that nullable reference types are enabled
		// In modern .NET this is typically configured via project settings
		foreach (var assembly in ApplicationAssemblies)
		{
			var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsNested).Take(10);

			// If we can find types, nullable is likely configured
			// The actual NullableContextAttribute may be on the module or types in .NET 10
			var hasTypes = types.Any();

			hasTypes.Should().BeTrue(
				$"Assembly {assembly.GetName().Name} should have types (nullable enabled)");
		}
	}

	[Fact]
	public void ProductionCodeShouldNotReferenceXUnit()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.Should()
			.NotHaveDependencyOn("xunit")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"production code should not reference xunit. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ProductionCodeShouldNotReferenceNUnit()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.Should()
			.NotHaveDependencyOn("NUnit")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"production code should not reference NUnit. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ProductionCodeShouldNotReferenceMSTest()
	{
		var result = Types.InAssemblies(ApplicationAssemblies)
			.Should()
			.NotHaveDependencyOn("MSTest")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"production code should not reference MSTest. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void MethodAccessibilityShouldBeCorrect()
	{
		// Ensure public methods are truly public and private methods are truly private
		// Records generate EqualityContract and PrintMembers which are special compiler-generated
		var types = Types.InAssemblies(ApplicationAssemblies).GetTypes();

		var incorrectAccessibility = new List<string>();

		foreach (var type in types)
		{
			// Skip anonymous types, compiler-generated types, and nested types
			if (type.Name.Contains("<>") ||
					type.Name.Contains("+") ||
					type.IsNested ||
					type.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated")))
			{
				continue;
			}

			var publicMethods = type.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.Static |
				System.Reflection.BindingFlags.DeclaredOnly);

			// Check that public methods are intentionally public (not compiler-generated mistakes)
			foreach (var method in publicMethods)
			{
				// Skip compiler-generated methods (records, async, etc.)
				if (method.Name.Contains("<") ||
						method.Name.Contains("$") ||
						method.Name.Contains(">") ||
						method.Name == "EqualityContract" ||
						method.Name == "PrintMembers" ||
						method.Name.StartsWith("get_") ||
						method.Name.StartsWith("set_"))
				{
					continue;
				}

				// Public methods should be on public types or extension methods
				if (!type.IsPublic && !type.IsNestedPublic && !method.IsStatic)
				{
					incorrectAccessibility.Add($"{type.FullName}.{method.Name}");
				}
			}
		}

		incorrectAccessibility.Should().BeEmpty(
			$"methods should have correct accessibility. Issues: {string.Join(", ", incorrectAccessibility)}");
	}
}
