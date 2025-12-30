using System.Diagnostics.CodeAnalysis;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Web;

namespace Notes.Web.Tests.Unit.Components.Layout;

[ExcludeFromCodeCoverage]
public class MainLayoutTests : BunitContext
{
	public MainLayoutTests()
	{
		var localStorage = Substitute.For<ILocalStorageService>();
		Services.AddSingleton(localStorage);

		// Setup authorization for NavMenu component (which contains AuthorizeView)
		this.AddAuthorization();

		// Setup JSInterop for ThemeToggle component
		var themeModule = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		themeModule.SetupVoid("setTheme", _ => true);
	}

	[Fact]
	public void MainLayout_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ShouldContainNavMenu()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		cut.FindComponent<NavMenuComponent>().Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ShouldContainFooter()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		cut.FindComponent<FooterComponent>().Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ShouldHaveMainElement()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		var main = cut.Find("main");
		main.Should().NotBeNull();
		main.ClassList.Should().Contain("container");
		main.ClassList.Should().Contain("flex-1");
	}

	[Fact]
	public void MainLayout_ShouldHaveErrorBoundary()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		// ErrorBoundary component should exist - error UI only renders when an error occurs
		cut.FindComponent<ErrorBoundary>().Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ShouldHaveCorrectContainerStyling()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		var rootDiv = cut.Find("div.antialiased");
		rootDiv.Should().NotBeNull();
		rootDiv.ClassList.Should().Contain("bg-gray-400");
		rootDiv.ClassList.Should().Contain("dark:bg-gray-800");
		rootDiv.ClassList.Should().Contain("transition-colors");
	}

	[Fact]
	public void MainLayout_ShouldHaveFlexLayout()
	{
		// Arrange & Act
		var cut = Render<MainLayout>();

		// Assert
		var flexContainer = cut.Find("div.flex.flex-col");
		flexContainer.Should().NotBeNull();
		flexContainer.ClassList.Should().Contain("min-h-screen");
	}
}