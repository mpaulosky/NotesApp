using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class LoginComponentTests : BunitContext
{
	[Fact]
	public void LoginComponent_ShouldRender()
	{
		// Arrange
		var authContext = this.AddAuthorization();

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void LoginComponent_WhenAuthenticated_ShouldShowLogoutLink()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("Test User");

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Markup.Should().Contain("Log out");
		var link = cut.Find("a");
		link.GetAttribute("href").Should().Be("Account/Logout");
	}

	[Fact]
	public void LoginComponent_WhenNotAuthenticated_ShouldShowLoginLink()
	{
		// Arrange
		var authContext = this.AddAuthorization();

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Markup.Should().Contain("Log in");
		var link = cut.Find("a");
		link.GetAttribute("href").Should().Be("Account/Login");
	}

	[Fact]
	public void LoginComponent_WhenAuthenticated_ShouldNotShowLoginLink()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("Test User");

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Markup.Should().NotContain("Log in");
	}

	[Fact]
	public void LoginComponent_WhenNotAuthenticated_ShouldNotShowLogoutLink()
	{
		// Arrange
		var authContext = this.AddAuthorization();

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Markup.Should().NotContain("Log out");
	}

	[Fact]
	public void LoginComponent_WithAdminUser_ShouldShowLogoutLink()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("Admin User");
		authContext.SetRoles("Admin");

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		cut.Markup.Should().Contain("Log out");
	}

	[Fact]
	public void LoginComponent_LogoutLink_ShouldHaveHoverStyling()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("Test User");

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		var link = cut.Find("a");
		link.ClassList.Should().Contain("p-1");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}

	[Fact]
	public void LoginComponent_LoginLink_ShouldHaveHoverStyling()
	{
		// Arrange
		var authContext = this.AddAuthorization();

		// Act
		var cut = Render<LoginComponent>();

		// Assert
		var link = cut.Find("a");
		link.ClassList.Should().Contain("p-1");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}
}
