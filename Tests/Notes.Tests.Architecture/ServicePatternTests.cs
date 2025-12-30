// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ServicePatternTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests for service layer patterns including wrappers and extensions.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServicePatternTests : BaseArchitectureTest
{
	[Fact]
	public void WrappersShouldImplementInterface()
	{
		var wrappers = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Wrapper")
			.GetTypes();

		var wrappersWithoutInterface = wrappers
			.Where(w => !w.IsInterface && w.GetInterfaces().Length == 0)
			.Select(w => w.FullName)
			.ToList();

		wrappersWithoutInterface.Should().BeEmpty(
			$"wrapper classes should implement an interface. Missing interfaces: {string.Join(", ", wrappersWithoutInterface)}");
	}

	[Fact]
	public void WrapperInterfacesShouldStartWithI()
	{
		var wrappers = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Wrapper")
			.And()
			.AreInterfaces()
			.GetTypes();

		var incorrectlyNamedInterfaces = wrappers
			.Where(w => !w.Name.StartsWith("I"))
			.Select(w => w.FullName)
			.ToList();

		incorrectlyNamedInterfaces.Should().BeEmpty(
			$"wrapper interfaces should start with 'I'. Incorrect: {string.Join(", ", incorrectlyNamedInterfaces)}");
	}

	[Fact]
	public void ServiceExtensionsShouldBeInServicesNamespace()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("ServiceExtensions")
			.Should()
			.ResideInNamespaceContaining("Services")
			.Or()
			.ResideInNamespaceContaining("Data")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"service extension classes should be in Services or Data namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ExtensionMethodsShouldExtendCorrectTypes()
	{
		var extensionClasses = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Extensions")
			.Or()
			.HaveNameEndingWith("ServiceExtensions")
			.GetTypes();

		var invalidExtensions = new List<string>();

		foreach (var extensionClass in extensionClasses)
		{
			if (!extensionClass.IsSealed || !extensionClass.IsAbstract) // static classes are sealed and abstract
			{
				invalidExtensions.Add($"{extensionClass.Name} (not static)");
				continue;
			}

			var methods = extensionClass.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static);

			var extensionMethods = methods.Where(m =>
				m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false));

			if (!extensionMethods.Any())
			{
				invalidExtensions.Add($"{extensionClass.Name} (no extension methods)");
			}
		}

		invalidExtensions.Should().BeEmpty(
			$"extension classes should be static with extension methods. Invalid: {string.Join(", ", invalidExtensions)}");
	}

	[Fact]
	public void ServicesShouldBeInServicesNamespace()
	{
		var services = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Service")
			.And()
			.AreClasses()
			.GetTypes();

		var servicesInWrongNamespace = services
			.Where(s => !s.Namespace!.Contains("Services"))
			.Select(s => s.FullName)
			.ToList();

		servicesInWrongNamespace.Should().BeEmpty(
			$"service classes should be in Services namespace. Wrong namespace: {string.Join(", ", servicesInWrongNamespace)}");
	}

	[Fact]
	public void OptionClassesShouldBeUsedByCorrespondingService()
	{
		var optionClasses = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.GetTypes();

		var orphanedOptions = new List<string>();

		foreach (var optionClass in optionClasses)
		{
			// Extract base name from options class (e.g., "Ai" from "AiServiceOptions")
			var baseName = optionClass.Name.Replace("ServiceOptions", "").Replace("Options", "");

			// Check if there's a corresponding service class
			var serviceName = baseName + "Service";
			var serviceExists = Types.InAssembly(WebAssembly)
				.That()
				.HaveName(serviceName)
				.GetTypes()
				.Any();

			if (!serviceExists)
			{
				// Check for interface pattern (e.g., IAiService for AiServiceOptions)
				var interfaceName = "I" + baseName + "Service";
				var interfaceExists = Types.InAssembly(WebAssembly)
					.That()
					.HaveName(interfaceName)
					.GetTypes()
					.Any();

				if (!interfaceExists)
				{
					// Also check for wrapper pattern
					var wrapperName = baseName + "Wrapper";
					var wrapperExists = Types.InAssembly(WebAssembly)
						.That()
						.HaveName(wrapperName)
						.GetTypes()
						.Any();

					if (!wrapperExists)
					{
						orphanedOptions.Add(optionClass.Name);
					}
				}
			}
		}

		orphanedOptions.Should().BeEmpty(
			$"option classes should have corresponding service, interface, or wrapper. Orphaned: {string.Join(", ", orphanedOptions)}");
	}

	[Fact]
	public void ServicesShouldNotHaveStaticMethods()
	{
		// Services should be instantiated, not static
		var serviceTypes = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Service")
			.And()
			.AreClasses()
			.And()
			.DoNotHaveNameEndingWith("Extensions")
			.GetTypes();

		var servicesWithStaticMethods = new List<string>();

		foreach (var service in serviceTypes)
		{
			var staticMethods = service.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static |
				System.Reflection.BindingFlags.DeclaredOnly)
				.Where(m => !m.IsSpecialName) // Exclude operators, etc.
				.ToList();

			if (staticMethods.Any())
			{
				servicesWithStaticMethods.Add($"{service.Name}: {string.Join(", ", staticMethods.Select(m => m.Name))}");
			}
		}

		servicesWithStaticMethods.Should().BeEmpty(
			$"services should be instance-based, not static. Violations: {string.Join("; ", servicesWithStaticMethods)}");
	}

	[Fact]
	public void AiServiceseShouldBeInAiNamespace()
	{
		var aiServices = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Services")
			.And()
			.HaveDependencyOn("OpenAI")
			.GetTypes();

		var aiServicesInWrongNamespace = aiServices
			.Where(s => !s.Namespace!.Contains(".Ai"))
			.Select(s => s.FullName)
			.ToList();

		aiServicesInWrongNamespace.Should().BeEmpty(
			$"AI-related services should be in Services.Ai namespace. Wrong namespace: {string.Join(", ", aiServicesInWrongNamespace)}");
	}
}
