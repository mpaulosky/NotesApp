// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Auth0AuthenticationStateProviderTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesSite
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Notes.Web.Services;

namespace Notes.Web.Tests.Unit.Services;

[ExcludeFromCodeCoverage]
public class Auth0AuthenticationStateProviderTests
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<Auth0AuthenticationStateProvider> _logger;
	private readonly Auth0AuthenticationStateProvider _provider;

	public Auth0AuthenticationStateProviderTests()
	{
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_logger = Substitute.For<ILogger<Auth0AuthenticationStateProvider>>();
		_provider = new Auth0AuthenticationStateProvider(_httpContextAccessor, _logger);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserNotAuthenticated_ShouldReturnAnonymousUser()
	{
		// Arrange
		var httpContext = new DefaultHttpContext
		{
			User = new ClaimsPrincipal(new ClaimsIdentity())
		};
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenHttpContextIsNull_ShouldReturnAnonymousUser()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserAuthenticated_ShouldReturnAuthenticatedUser()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new(ClaimTypes.Email, "test@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		result.User.Identity.Name.Should().Be("Test User");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserAuthenticated_ShouldLogUserClaims()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		await _provider.GetAuthenticationStateAsync();

		// Assert
		_logger.Received(1).Log(
			LogLevel.Information,
			Arg.Any<EventId>(),
			Arg.Is<object>(o => o.ToString()!.Contains("User authenticated with claims")),
			Arg.Any<Exception>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithCustomRolesClaim_ShouldAddRolesToIdentity()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new("https://articlesite.com/roles", "Admin,User,Editor")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("User").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithAuth0RolesClaims_ShouldAddRolesToIdentity()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new("roles", "Administrator"),
			new("roles", "Moderator")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Administrator").Should().BeTrue();
		result.User.IsInRole("Moderator").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithBothRoleTypes_ShouldNotDuplicateRoles()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new("https://articlesite.com/roles", "Admin"),
			new("roles", "Admin") // Same role from different source
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		var roleClaims = result.User.FindAll(ClaimTypes.Role).ToList();
		roleClaims.Should().HaveCount(1); // Should not duplicate
		roleClaims.First().Value.Should().Be("Admin");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithEmptyCustomRolesClaim_ShouldNotThrow()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new("https://articlesite.com/roles", string.Empty)
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var act = async () => await _provider.GetAuthenticationStateAsync();

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithRolesContainingWhitespace_ShouldTrimRoles()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new("https://articlesite.com/roles", " Admin , User , Editor ")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("User").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		// Should not have roles with whitespace
		result.User.IsInRole(" Admin ").Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithNoRolesClaims_ShouldStillReturnAuthenticatedUser()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new(ClaimTypes.Email, "test@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		result.User.FindAll(ClaimTypes.Role).Should().BeEmpty();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_ShouldPreserveAllOriginalClaims()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new(ClaimTypes.Name, "Test User"),
			new(ClaimTypes.Email, "test@example.com"),
			new("custom_claim", "custom_value")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("auth0|123456");
		result.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("Test User");
		result.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
		result.User.FindFirst("custom_claim")?.Value.Should().Be("custom_value");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithMultipleRolesInCommaSeparatedString_ShouldSplitCorrectly()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new("https://articlesite.com/roles", "Role1,Role2,Role3,Role4,Role5")
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		var roleClaims = result.User.FindAll(ClaimTypes.Role).ToList();
		roleClaims.Should().HaveCount(5);
		result.User.IsInRole("Role1").Should().BeTrue();
		result.User.IsInRole("Role2").Should().BeTrue();
		result.User.IsInRole("Role3").Should().BeTrue();
		result.User.IsInRole("Role4").Should().BeTrue();
		result.User.IsInRole("Role5").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithEmptyStringInRolesList_ShouldSkipEmptyEntries()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "auth0|123456"),
			new("https://articlesite.com/roles", "Admin,,User,,") // Empty entries between commas
		};
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var principal = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		var roleClaims = result.User.FindAll(ClaimTypes.Role).ToList();
		roleClaims.Should().HaveCount(2); // Only Admin and User, empty entries skipped
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("User").Should().BeTrue();
	}
}
