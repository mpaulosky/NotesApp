using System.Diagnostics.CodeAnalysis;

using Blazored.LocalStorage;

using Notes.Web.Tests.Unit.Helpers;

namespace Notes.Web.Tests.Unit.Components.Layout;

[ExcludeFromCodeCoverage]
public class NavMenuComponentTests : BunitContext
{
	public NavMenuComponentTests()
	{
		var localStorage = Substitute.For<ILocalStorageService>();
		Services.AddSingleton(localStorage);

		// Setup JSInterop for ThemeToggle component
		var themeModule = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		themeModule.SetupVoid("setTheme", _ => true);
	}

	[Fact]
	public void NavMenuComponent_ShouldRender()
	{
		// Arrange
		this.AddAuthorization();

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void NavMenuComponent_ShouldDisplaySiteName()
	{
		// Arrange
		this.AddAuthorization();

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Article Site");
	}

	[Fact]
	public void NavMenuComponent_ShouldContainThemeToggle()
	{
		// Arrange
		this.AddAuthorization();

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.FindComponent<ThemeToggle>().Should().NotBeNull();
	}

	[Fact]
	public void NavMenuComponent_ShouldDisplayNavigationLinks()
	{
		// Arrange
		this.AddAuthorization();

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Articles");
		cut.Markup.Should().Contain("About");
		cut.Markup.Should().Contain("Contacts");
	}

	[Fact]
	public void NavMenuComponent_ShouldHaveCorrectStyling()
	{
		// Arrange
		this.AddAuthorization();

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		var header = cut.Find("header");
		header.ClassList.Should().Contain("container");
		header.ClassList.Should().Contain("flex");
		header.ClassList.Should().Contain("items-center");
		header.ClassList.Should().Contain("justify-between");
	}

	[Fact]
	public void NavMenuComponent_WhenAuthenticated_ShouldShowUserName()
	{
		// Arrange
		TestAuthHelper.RegisterTestAuthentication(this, "Test User");

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Test User");
		cut.Markup.Should().Contain("Hey");
	}

	[Fact]
	public void NavMenuComponent_WhenNotAuthenticated_ShouldNotShowUserName()
	{
		// Arrange
		TestAuthHelper.RegisterUnauthenticated(this);

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().NotContain("Hey");
	}

	[Fact]
	public void NavMenuComponent_WithAdminRole_ShouldShowCategoriesLink()
	{
		// Arrange
		TestAuthHelper.RegisterAdminAuthentication(this, "Admin User");

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Categories");
	}

	[Fact]
	public void NavMenuComponent_WithoutAdminRole_ShouldNotShowCategoriesLink()
	{
		// Arrange
		TestAuthHelper.RegisterUserAuthentication(this, "Regular User");

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().NotContain("Categories");
	}

	[Fact]
	public void NavMenuComponent_WhenAuthenticated_ShouldShowLogoutLink()
	{
		// Arrange
		TestAuthHelper.RegisterTestAuthentication(this, "Test User");

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Log out");
	}

	[Fact]
	public void NavMenuComponent_WhenNotAuthenticated_ShouldShowLoginLink()
	{
		// Arrange
		TestAuthHelper.RegisterUnauthenticated(this);

		// Act
		var cut = Render<NavMenuComponent>();

		// Assert
		cut.Markup.Should().Contain("Log in");
	}
}