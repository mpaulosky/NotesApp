using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Bunit.TestDoubles;

using Notes.Web.Components.User;

namespace Notes.Web.Tests.Unit.Components.User;

[ExcludeFromCodeCoverage]
public class ProfileTests : BunitContext
{
	public ProfileTests()
	{
		// Setup authorization for Profile page
		this.AddAuthorization();
	}

	[Fact]
	public void Profile_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<Profile>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Profile_ShouldHavePageHeading()
	{
		// Arrange & Act
		var cut = Render<Profile>();

		// Assert
		var heading = cut.FindComponent<PageHeadingComponent>();
		heading.Should().NotBeNull();
		heading.Instance.Level.Should().Be("1");
		heading.Instance.HeaderText.Should().Be("User Profile");
	}

	[Fact]
	public void Profile_ShouldDisplayBasicInformationSection()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetClaims(
			new Claim(ClaimTypes.NameIdentifier, "user123"),
			new Claim(ClaimTypes.Email, "test@example.com")
		);

		// Act
		var cut = Render<Profile>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("Basic Information");
		markup.Should().Contain("Name:");
		markup.Should().Contain("Email:");
		markup.Should().Contain("User ID:");
	}

	[Fact]
	public void Profile_ShouldDisplayUserName()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("JohnDoe");

		// Act
		var cut = Render<Profile>();

		// Assert
		cut.Markup.Should().Contain("JohnDoe");
	}

	[Fact]
	public void Profile_ShouldDisplayEmail()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetClaims(new Claim(ClaimTypes.Email, "user@domain.com"));

		// Act
		var cut = Render<Profile>();

		// Assert
		cut.Markup.Should().Contain("user@domain.com");
	}

	[Fact]
	public void Profile_ShouldDisplayUserId()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, "abc-123-xyz"));

		// Act
		var cut = Render<Profile>();

		// Assert
		cut.Markup.Should().Contain("abc-123-xyz");
	}

	[Fact]
	public void Profile_ShouldDisplayRolesSection()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetRoles("User", "Admin");

		// Act
		var cut = Render<Profile>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("Roles & Permissions");
		markup.Should().Contain("Roles:");
	}

	[Fact]
	public void Profile_ShouldDisplayUserRole()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetRoles("User");

		// Act
		var cut = Render<Profile>();

		// Assert
		var roleList = cut.Find("ul.list-disc");
		roleList.Should().NotBeNull();
		roleList.TextContent.Should().Contain("User");
	}

	[Fact]
	public void Profile_ShouldDisplayMultipleRoles()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("AdminUser");
		authContext.SetRoles("User", "Admin", "Editor");

		// Act
		var cut = Render<Profile>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("User");
		markup.Should().Contain("Admin");
		markup.Should().Contain("Editor");
	}

	[Fact]
	public void Profile_ShouldDisplayNoRolesMessage()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		// No roles set

		// Act
		var cut = Render<Profile>();

		// Assert
		cut.Markup.Should().Contain("No roles assigned");
	}

	[Fact]
	public void Profile_ShouldDisplayProfilePictureWhenAvailable()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetClaims(new Claim("picture", "https://example.com/avatar.jpg"));

		// Act
		var cut = Render<Profile>();

		// Assert
		var img = cut.Find("img[alt='Profile Picture']");
		img.Should().NotBeNull();
		img.GetAttribute("src").Should().Be("https://example.com/avatar.jpg");
		img.ClassList.Should().Contain("rounded-full");
		img.ClassList.Should().Contain("w-24");
		img.ClassList.Should().Contain("h-24");
	}

	[Fact]
	public void Profile_ShouldDisplayPlaceholderWhenNoPicture()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		// No picture claim

		// Act
		var cut = Render<Profile>();

		// Assert
		var placeholder = cut.Find("div.rounded-full.w-24.h-24.bg-gray-700");
		placeholder.Should().NotBeNull();
		placeholder.TextContent.Should().Contain("?");
	}

	[Fact]
	public void Profile_ShouldDisplayAllClaimsTable()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		authContext.SetClaims(
			new Claim(ClaimTypes.NameIdentifier, "123"),
			new Claim(ClaimTypes.Email, "test@test.com"),
			new Claim("custom_claim", "custom_value")
		);

		// Act
		var cut = Render<Profile>();

		// Assert
		var table = cut.Find("table");
		table.Should().NotBeNull();

		var markup = cut.Markup;
		markup.Should().Contain("All Claims");
		markup.Should().Contain("Claim Type");
		markup.Should().Contain("Value");
	}

	[Fact]
	public void Profile_ShouldHaveContainerLayout()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");

		// Act
		var cut = Render<Profile>();

		// Assert
		var container = cut.Find("div.container.mx-auto");
		container.Should().NotBeNull();
	}

	[Fact]
	public void Profile_ShouldHaveGridLayout()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");

		// Act
		var cut = Render<Profile>();

		// Assert
		var grid = cut.Find("div.grid.grid-cols-1.md\\:grid-cols-3");
		grid.Should().NotBeNull();
		grid.ClassList.Should().Contain("gap-6");
	}

	[Fact]
	public void Profile_ShouldUseAuthorizeView()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");

		// Act
		var cut = Render<Profile>();

		// Assert
		// Content should be visible when authorized
		cut.Markup.Should().Contain("Profile Information");
	}

	[Fact]
	public void Profile_ShouldHandleEmptyEmail()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		// No email claim

		// Act
		var cut = Render<Profile>();

		// Assert
		// Should render without errors
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Email:");
	}

	[Fact]
	public void Profile_ShouldHaveDebugInformation()
	{
		// Arrange
		var authContext = this.AddAuthorization();
		authContext.SetAuthorized("TestUser");

		// Act
		var cut = Render<Profile>();

		// Assert
		cut.Markup.Should().Contain("Debug information showing all claims");
	}
}
