using System.Diagnostics.CodeAnalysis;

using Bunit.TestDoubles;

namespace Notes.Web.Tests.Unit.Helpers;

/// <summary>
/// Helper methods for configuring authentication in bUnit component tests.
/// Provides convenient methods for setting up authenticated and unauthenticated test scenarios with role support.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestAuthHelper
{
	/// <summary>
	/// Registers a test authentication context for bUnit tests with an authenticated user.
	/// </summary>
	/// <param name="context">The BunitContext to register with.</param>
	/// <param name="userName">The test user name.</param>
	/// <param name="roles">The roles for the test user (optional).</param>
	/// <example>
	/// TestAuthHelper.RegisterTestAuthentication(this, "TestUser", new[] { "Admin", "User" });
	/// </example>
	public static void RegisterTestAuthentication(BunitContext context, string userName, string[]? roles = null)
	{
		var authContext = context.AddAuthorization();
		authContext.SetAuthorized(userName);

		if (roles != null && roles.Length > 0)
		{
			authContext.SetRoles(roles);
		}
	}

	/// <summary>
	/// Registers a test authentication context for unauthenticated scenarios.
	/// </summary>
	/// <param name="context">The BunitContext to register with.</param>
	/// <example>
	/// TestAuthHelper.RegisterUnauthenticated(this);
	/// </example>
	public static void RegisterUnauthenticated(BunitContext context)
	{
		// AddAuthorization() default state is already unauthenticated
		context.AddAuthorization();
	}

	/// <summary>
	/// Registers a test authentication context with an admin user that has admin roles.
	/// Uses "notes.admin" role which matches the application's AdminPolicy constant.
	/// </summary>
	/// <param name="context">The BunitContext to register with.</param>
	/// <param name="userName">The admin user name (defaults to "AdminUser").</param>
	/// <example>
	/// TestAuthHelper.RegisterAdminAuthentication(this);
	/// </example>
	public static void RegisterAdminAuthentication(BunitContext context, string userName = "AdminUser")
	{
		var authContext = context.AddAuthorization();
		authContext.SetAuthorized(userName);
		authContext.SetRoles("Admin", "notes.admin");
	}

	/// <summary>
	/// Registers a test authentication context with a regular user (no admin roles).
	/// </summary>
	/// <param name="context">The BunitContext to register with.</param>
	/// <param name="userName">The user name (defaults to "TestUser").</param>
	/// <example>
	/// TestAuthHelper.RegisterUserAuthentication(this);
	/// </example>
	public static void RegisterUserAuthentication(BunitContext context, string userName = "TestUser")
	{
		var authContext = context.AddAuthorization();
		authContext.SetAuthorized(userName);
		authContext.SetRoles("User");
	}
}
