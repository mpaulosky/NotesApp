// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BlazorComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

namespace Notes.Tests.Architecture;

/// <summary>
/// Tests specific to Blazor component architecture.
/// </summary>
[ExcludeFromCodeCoverage]
public class BlazorComponentTests : BaseArchitectureTest
{
	[Fact]
	public void ComponentsShouldInheritFromComponentBase()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.And()
			.AreClasses()
			.And()
			.AreNotNested() // Exclude nested types like form models
			.And()
			.DoNotHaveNameEndingWith("Base")
			.And()
			.DoNotHaveNameStartingWith("_") // Exclude compiler-generated like _Imports
			.And()
			.DoNotHaveNameStartingWith("__") // Exclude compiler-generated attributes
			.Should()
			.Inherit(typeof(ComponentBase))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Blazor components should inherit from ComponentBase. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ComponentsShouldNotBeSealed()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.And()
			.Inherit(typeof(ComponentBase))
			.Should()
			.NotBeSealed()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Blazor components should not be sealed to allow inheritance. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ComponentsShouldNotDependOnMediatR()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.Should()
			.NotHaveDependencyOn("MediatR")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Blazor components should not directly use MediatR. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void PageComponentsShouldHaveRouteAttribute()
	{
		// Check that routable components are properly configured
		var componentTypes = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Components")
			.And()
			.Inherit(typeof(ComponentBase))
			.GetTypes();

		var invalidComponents = new List<string>();

		foreach (var componentType in componentTypes)
		{
			var hasRouteAttribute = componentType.GetCustomAttributes(
				typeof(Microsoft.AspNetCore.Components.RouteAttribute), true).Length != 0;

			// If component has Route attribute, it should be in Pages namespace or similar
			if (hasRouteAttribute &&
					!componentType.Namespace!.Contains("Pages") &&
					!componentType.Namespace.Contains("Components"))
			{
				invalidComponents.Add(componentType.FullName!);
			}
		}

		invalidComponents.Should().BeEmpty(
			$"routable components should be in appropriate namespace. Invalid: {string.Join(", ", invalidComponents)}");
	}

	[Fact]
	public void ComponentsShouldHaveParameterPropertiesWithAttribute()
	{
		// Get all Blazor component types
		var componentTypes = Types.InAssembly(WebAssembly)
			.That()
			.Inherit(typeof(ComponentBase))
			.GetTypes();

		var invalidComponents = new List<string>();

		foreach (var componentType in componentTypes)
		{
			var publicProperties = componentType.GetProperties(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.DeclaredOnly);

			foreach (var property in publicProperties)
			{
				// Check if public property is settable and doesn't have Parameter or CascadingParameter attribute
				if (property.CanWrite &&
						property.GetCustomAttributes(typeof(ParameterAttribute), true).Length == 0 &&
						property.GetCustomAttributes(typeof(CascadingParameterAttribute), true).Length == 0)
				{
					invalidComponents.Add($"{componentType.FullName}.{property.Name}");
				}
			}
		}

		invalidComponents.Should().BeEmpty(
			$"public settable properties on components should have [Parameter] or [CascadingParameter] attribute. Invalid properties: {string.Join(", ", invalidComponents)}");
	}

	[Fact]
	public void LayoutComponentsShouldBeInLayoutNamespace()
	{
		// Layout components should be in Layout namespace
		// They may inherit from ComponentBase or LayoutComponentBase depending on use case
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespaceContaining("Layout")
			.And()
			.AreClasses()
			.And()
			.DoNotHaveNameEndingWith("Base")
			.Should()
			.Inherit(typeof(ComponentBase))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"layout components should inherit from ComponentBase. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void ComponentsShouldBeInCorrectNamespace()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.Inherit(typeof(ComponentBase))
			.Should()
			.ResideInNamespace("Notes.Web.Components")
			.Or()
			.ResideInNamespaceStartingWith("Notes.Web.Components.")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Blazor components should reside in Notes.Web.Components namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
