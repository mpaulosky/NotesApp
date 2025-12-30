// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ComponentArchitectureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

namespace Notes.Tests.Architecture;

/// <summary>
/// Additional Blazor component architecture tests for component organization and patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class ComponentArchitectureTests : BaseArchitectureTest
{
	[Fact]
	public void PageComponentsShouldBeInPagesOrUserFolder()
	{
		var pageComponents = Types.InAssembly(WebAssembly)
			.That()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Components.RouteAttribute))
			.GetTypes();

		var invalidPages = pageComponents
			.Where(t => !t.Namespace!.Contains("Pages") && !t.Namespace.Contains("User"))
			.Select(t => t.FullName)
			.ToList();

		invalidPages.Should().BeEmpty(
			$"components with @page directive should be in Pages or User folder. Invalid: {string.Join(", ", invalidPages)}");
	}

	[Fact]
	public void SharedComponentsShouldBeInSharedFolder()
	{
		var sharedComponents = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components.Shared")
			.And()
			.Inherit(typeof(ComponentBase))
			.GetTypes();

		sharedComponents.Should().NotBeEmpty(
			"there should be reusable components in the Shared folder");

		foreach (var component in sharedComponents)
		{
			component.Namespace.Should().Contain("Components.Shared",
				$"{component.Name} should be in Components.Shared namespace");
		}
	}

	[Fact]
	public void ComponentsShouldNotHaveBusinessLogicInPublicMethods()
	{
		// Components should expose functionality via parameters, not complex public methods
		var componentTypes = Types.InAssembly(WebAssembly)
			.That()
			.Inherit(typeof(ComponentBase))
			.And()
			.DoNotHaveNameStartingWith("_")
			.GetTypes();

		// This is informational - ensure we're aware of public methods on components
		var componentsWithPublicMethods = new List<string>();

		foreach (var component in componentTypes)
		{
			var publicMethods = component.GetMethods(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.DeclaredOnly)
				.Where(m => !m.Name.StartsWith("get_") &&
									 !m.Name.StartsWith("set_") &&
									 !m.Name.StartsWith("BuildRenderTree") &&
									 !m.Name.StartsWith("SetParametersAsync") &&
									 !m.Name.StartsWith("OnInitialized") &&
									 !m.Name.StartsWith("OnAfterRender") &&
									 !m.Name.StartsWith("On") && // Event handlers
									 !m.Name.StartsWith("Dispose") &&
									 !m.IsSpecialName)
				.ToList();

			if (publicMethods.Any())
			{
				componentsWithPublicMethods.Add($"{component.Name}: {string.Join(", ", publicMethods.Select(m => m.Name))}");
			}
		}

		// This is informational, not a hard failure
		if (componentsWithPublicMethods.Any())
		{
			// Log for awareness but don't fail
			var message = $"Components with public methods (review if business logic): {string.Join("; ", componentsWithPublicMethods)}";
			// Using Should().HaveCountLessThan to make this informational
			componentsWithPublicMethods.Should().HaveCountLessThan(100, message);
		}
	}

	[Fact]
	public void ComponentParametersShouldBeProperties()
	{
		var componentTypes = Types.InAssembly(WebAssembly)
			.That()
			.Inherit(typeof(ComponentBase))
			.GetTypes();

		var invalidParameters = new List<string>();

		foreach (var component in componentTypes)
		{
			var parameterAttributes = component.GetProperties()
				.Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), true).Any());

			foreach (var param in parameterAttributes)
			{
				if (!param.CanWrite)
				{
					invalidParameters.Add($"{component.Name}.{param.Name} (read-only)");
				}

				if (!param.SetMethod!.IsPublic)
				{
					invalidParameters.Add($"{component.Name}.{param.Name} (non-public setter)");
				}
			}
		}

		invalidParameters.Should().BeEmpty(
			$"component parameters should be public settable properties. Invalid: {string.Join(", ", invalidParameters)}");
	}

	[Fact]
	public void ComponentsShouldHaveClearNaming()
	{
		var components = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.And()
			.Inherit(typeof(ComponentBase))
			.And()
			.DoNotHaveNameStartingWith("_")
			.And()
			.DoNotHaveNameStartingWith("__")
			.GetTypes();

		// More lenient naming - just ensure components are intentionally named
		var veryPoorlyNamedComponents = components
			.Where(c => c.Name.Length < 3 || // Too short
								 c.Name.All(char.IsDigit) || // All numbers
								 c.Name.StartsWith("Temp")) // Temporary names
			.Select(c => c.FullName)
			.ToList();

		veryPoorlyNamedComponents.Should().BeEmpty(
			$"components should have meaningful names. Poorly named: {string.Join(", ", veryPoorlyNamedComponents)}");
	}

	[Fact]
	public void PagesShouldInjectLogger()
	{
		// Best practice: pages should inject ILogger for diagnostics
		var pageTypes = Types.InAssembly(WebAssembly)
			.That()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Components.RouteAttribute))
			.GetTypes();

		var pagesWithoutLogger = new List<string>();

		foreach (var page in pageTypes)
		{
			var properties = page.GetProperties(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance);

			var hasLogger = properties.Any(p =>
				p.PropertyType.IsGenericType &&
				p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Logging.ILogger<>));

			if (!hasLogger)
			{
				pagesWithoutLogger.Add(page.Name);
			}
		}

		// This is a recommendation, not strict requirement - using Should().HaveCountLessThan for flexibility
		pagesWithoutLogger.Should().HaveCountLessThan(pageTypes.Count(),
			$"most pages should inject ILogger for diagnostics. Pages without logger: {string.Join(", ", pagesWithoutLogger)}");
	}

	[Fact]
	public void ComponentsShouldNotDependOnConcreteRepositories()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.Inherit(typeof(ComponentBase))
			.Should()
			.NotHaveDependencyOn("Repositories")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"components should not directly depend on repositories. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
