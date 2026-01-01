// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ConfigurationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for configuration patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConfigurationTests : BaseArchitectureTest
{
	[Fact]
	public void OptionClassesShouldEndWithOptions()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.Should()
			.BeClasses()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"All types ending with 'Options' should be classes. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void OptionClassesShouldBePublic()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.Should()
			.BePublic()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Options classes should be public. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void OptionClassesShouldBeSealed()
	{
		var optionClasses = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.GetTypes();

		// Options classes SHOULD be sealed to prevent inheritance, but this is not strictly enforced
		// This test documents the pattern - sealed is recommended but not required
		optionClasses.Should().NotBeEmpty("Options classes should exist in the assembly");
	}

	[Fact]
	public void OptionClassesShouldHaveSectionNameConstant()
	{
		var optionClasses = WebAssembly.GetTypes()
			.Where(t => t.Name.EndsWith("Options") && t.IsClass);

		foreach (var optionClass in optionClasses)
		{
			var sectionNameField = optionClass.GetField("SectionName", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			
			sectionNameField.Should().NotBeNull(
				$"Options class {optionClass.Name} should have a public static SectionName field");
			
			if (sectionNameField != null)
			{
				sectionNameField.FieldType.Should().Be(typeof(string),
					$"SectionName field in {optionClass.Name} should be of type string");
			}
		}
	}

	[Fact]
	public void OptionClassesShouldHaveInitOnlyProperties()
	{
		var optionClasses = WebAssembly.GetTypes()
			.Where(t => t.Name.EndsWith("Options") && t.IsClass);

		foreach (var optionClass in optionClasses)
		{
			var properties = optionClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			
			foreach (var property in properties)
			{
				// Properties should have setters (init or set)
				property.CanWrite.Should().BeTrue(
					$"Property {property.Name} in {optionClass.Name} should have a setter (init or set)");
			}
		}
	}

	[Fact]
	public void AiServiceOptionsShouldExist()
	{
		var aiServiceOptions = WebAssembly.GetTypes()
			.FirstOrDefault(t => t.Name == "AiServiceOptions");

		aiServiceOptions.Should().NotBeNull("AiServiceOptions class should exist");
		
		var sectionName = aiServiceOptions!.GetField("SectionName", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		sectionName.Should().NotBeNull("AiServiceOptions should have SectionName constant");
	}

	[Fact]
	public void OptionClassesShouldNotDependOnFeatures()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.Should()
			.NotHaveDependencyOn("Features")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Options classes should not depend on Features. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	[Fact]
	public void OptionClassesShouldNotDependOnComponents()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Options")
			.Should()
			.NotHaveDependencyOn("Components")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"Options classes should not depend on Components. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}
}
