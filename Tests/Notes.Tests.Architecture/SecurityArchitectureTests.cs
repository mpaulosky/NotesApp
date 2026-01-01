// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SecurityArchitectureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using NetArchTest.Rules;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for security patterns including authentication and authorization.
/// </summary>
[ExcludeFromCodeCoverage]
public class SecurityArchitectureTests : BaseArchitectureTest
{

	/// <summary>
	/// Verifies that page components requiring authentication have the [Authorize] attribute.
	/// </summary>
	[Fact]
	public void AuthenticatedPagesShouldHaveAuthorizeAttribute()
	{
		// Pages that should require authentication
		string[] authenticatedPages =
		[
			"CreateNote",
			"EditNote",
			"ListNotes"
		];

		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Notes.Web.Components.Pages")
			.And()
			.HaveNameMatching(string.Join("|", authenticatedPages))
			.Should()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"authenticated pages should have [Authorize] attribute. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that admin pages have the appropriate role-based authorization.
	/// </summary>
	[Fact]
	public void AdminPagesShouldHaveRoleBasedAuthorization()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Notes.Web.Components.Pages")
			.And()
			.HaveNameMatching(".*Admin.*")
			.Should()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"admin pages should have [Authorize] attribute. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that authentication-related services are in the correct namespace.
	/// </summary>
	[Fact]
	public void AuthenticationServicesShouldBeInServicesNamespace()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameMatching(".*Authentication.*")
			.And()
			.AreClasses()
			.Should()
			.ResideInNamespace("Notes.Web.Services")
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"authentication services should be in Services namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that authentication state providers inherit from AuthenticationStateProvider.
	/// </summary>
	[Fact]
	public void AuthenticationStateProvidersShouldInheritFromBase()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("AuthenticationStateProvider")
			.And()
			.AreClasses()
			.Should()
			.Inherit(typeof(Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"authentication state providers should inherit from AuthenticationStateProvider. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that user-specific components requiring authentication are properly secured.
	/// </summary>
	[Fact]
	public void UserComponentsShouldHaveAuthorizeAttribute()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Notes.Web.Components.User")
			.And()
			.AreClasses()
			.Should()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"user components should have [Authorize] attribute. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that features handling user data validate user authorization through UserSubject.
	/// </summary>
	[Fact]
	public void FeatureCommandsShouldIncludeUserSubjectForAuthorization()
	{
		var commands = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Notes.Web.Features")
			.And()
			.HaveNameEndingWith("Command")
			.And()
			.AreNotAbstract()
			.GetTypes();

		// Commands that modify data should have UserSubject for authorization
		var commandsRequiringAuth = commands.Where(c =>
			c.Name.Contains("Create") ||
			c.Name.Contains("Update") ||
			c.Name.Contains("Delete")).ToList();

		foreach (var command in commandsRequiringAuth)
		{
			var properties = command.GetProperties();
			var hasUserSubject = properties.Any(p =>
				p.Name.Equals("UserSubject", StringComparison.OrdinalIgnoreCase) ||
				p.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase));

			hasUserSubject.Should().BeTrue(
				$"command {command.Name} should have UserSubject or UserId property for authorization");
		}
	}

	/// <summary>
	/// Verifies that authentication extension methods are properly structured.
	/// </summary>
	[Fact]
	public void AuthenticationExtensionsShouldBeStaticClasses()
	{
		var result = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameMatching(".*Authentication.*Extensions")
			.Should()
			.BeStatic()
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"authentication extension classes should be static. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

	/// <summary>
	/// Verifies that public pages do not require authorization.
	/// </summary>
	[Fact]
	public void PublicPagesShouldNotRequireAuthorization()
	{
		// Public pages that should not require authentication
		string[] publicPages =
		[
			"Home",
			"About",
			"Privacy",
			"Error"
		];

		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Notes.Web.Components.Pages")
			.And()
			.HaveNameMatching(string.Join("|", publicPages))
			.ShouldNot()
			.HaveCustomAttribute(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))
			.GetResult();

		result.IsSuccessful.Should().BeTrue(
			$"public pages should not have [Authorize] attribute. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
	}

}
