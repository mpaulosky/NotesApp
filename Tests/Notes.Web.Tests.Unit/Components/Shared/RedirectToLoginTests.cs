using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class RedirectToLoginTests : BunitContext
{
	[Fact]
	public void RedirectToLogin_ShouldRender()
	{
		// Arrange
		var navMan = Services.GetRequiredService<NavigationManager>();

		// Act
		var cut = Render<RedirectToLogin>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void RedirectToLogin_ShouldNavigateToLoginOnInitialized()
	{
		// Arrange
		var navMan = Services.GetRequiredService<NavigationManager>();

		// Act
		var cut = Render<RedirectToLogin>();

		// Assert
		// The component should trigger navigation during OnInitialized
		// Since we're using bUnit's FakeNavigationManager, we can verify the URI
		navMan.Uri.Should().Contain("/Account/Login");
		navMan.Uri.Should().Contain("returnUrl=");
	}

	[Fact]
	public void RedirectToLogin_ShouldIncludeReturnUrlInNavigation()
	{
		// Arrange
		var navMan = Services.GetRequiredService<NavigationManager>();
		var originalUri = navMan.Uri;

		// Act
		var cut = Render<RedirectToLogin>();

		// Assert
		navMan.Uri.Should().Contain("returnUrl=");
		navMan.Uri.Should().Contain(Uri.EscapeDataString(originalUri));
	}
}
