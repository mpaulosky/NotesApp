using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class LoadingComponentTests : BunitContext
{
	[Fact]
	public void LoadingComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<LoadingComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void LoadingComponent_ShouldDisplayLoadingText()
	{
		// Arrange & Act
		var cut = Render<LoadingComponent>();

		// Assert
		var heading = cut.Find("h3");
		heading.TextContent.Should().Contain("Loading...");
	}

	[Fact]
	public void LoadingComponent_ShouldHaveSpinner()
	{
		// Arrange & Act
		var cut = Render<LoadingComponent>();

		// Assert
		var svg = cut.Find("svg");
		svg.Should().NotBeNull();
		svg.ClassList.Should().Contain("animate-spin");
	}

	[Fact]
	public void LoadingComponent_ShouldHaveCorrectStyling()
	{
		// Arrange & Act
		var cut = Render<LoadingComponent>();

		// Assert
		var container = cut.Find("div");
		container.ClassList.Should().Contain("flex");
		container.ClassList.Should().Contain("justify-center");
		container.ClassList.Should().Contain("items-center");
	}
}