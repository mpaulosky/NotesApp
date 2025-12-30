using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components.Web;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class AdminTests : BunitContext
{
	public AdminTests()
	{
		// Setup authorization for Admin role requirement
		this.AddAuthorization();
	}

	[Fact]
	public void Admin_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Admin_ShouldHavePageTitle()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var pageTitle = cut.FindComponent<PageTitle>();
		pageTitle.Should().NotBeNull();
		// PageTitle sets browser title internally, cannot be directly asserted from markup
	}

	[Fact]
	public void Admin_ShouldDisplayMainHeading()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var heading = cut.Find("h1");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Admin Panel");
		heading.ClassList.Should().Contain("text-4xl");
		heading.ClassList.Should().Contain("font-bold");
	}

	[Fact]
	public void Admin_ShouldHaveAdminDashboardSection()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var dashboardHeading = cut.Find("h2");
		dashboardHeading.Should().NotBeNull();
		dashboardHeading.TextContent.Should().Be("Admin Dashboard");
	}

	[Fact]
	public void Admin_ShouldDisplayWelcomeMessage()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var welcomeText = cut.Markup;
		welcomeText.Should().Contain("Welcome to the admin panel");
		welcomeText.Should().Contain("Only users with the Admin role can access this page");
	}

	[Fact]
	public void Admin_ShouldHaveThreeManagementCards()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var cards = cut.FindAll("div.bg-gray-600.rounded");
		cards.Count.Should().Be(3);
	}

	[Fact]
	public void Admin_ShouldHaveUserManagementCard()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("User Management");
		markup.Should().Contain("Manage user accounts and permissions");
	}

	[Fact]
	public void Admin_ShouldHaveContentManagementCard()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("Content Management");
		markup.Should().Contain("Manage articles and categories");
	}

	[Fact]
	public void Admin_ShouldHaveSystemSettingsCard()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("System Settings");
		markup.Should().Contain("Configure application settings");
	}

	[Fact]
	public void Admin_ShouldHaveWarningSection()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var warning = cut.Find("div.bg-yellow-600");
		warning.Should().NotBeNull();
		warning.ClassList.Should().Contain("border-yellow-600");

		var markup = cut.Markup;
		markup.Should().Contain("Admin Access Required");
		markup.Should().Contain("role-based authorization");
	}

	[Fact]
	public void Admin_ShouldHaveCorrectGridLayout()
	{
		// Arrange & Act
		var cut = Render<Admin>();

		// Assert
		var grid = cut.Find("div.grid");
		grid.Should().NotBeNull();
		grid.ClassList.Should().Contain("grid-cols-1");
		grid.ClassList.Should().Contain("md:grid-cols-3");
		grid.ClassList.Should().Contain("gap-6");
	}
}
