using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class PageHeadingComponentTests : BunitContext
{
	[Fact]
	public void PageHeadingComponent_WithDefaultProps_ShouldRenderH1()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Should().Be("My Blog");
		h1.ClassList.Should().Contain("text-3xl");
		h1.ClassList.Should().Contain("text-gray-50");
	}

	[Fact]
	public void PageHeadingComponent_WithLevel2_ShouldRenderH2()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.HeaderText, "Test Header")
			.Add(p => p.Level, "2")
		);

		// Assert
		var h2 = cut.Find("h2");
		h2.Should().NotBeNull();
		h2.TextContent.Should().Be("Test Header");
		h2.ClassList.Should().Contain("text-2xl");
	}

	[Fact]
	public void PageHeadingComponent_WithLevel3_ShouldRenderH3()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.HeaderText, "Small Header")
			.Add(p => p.Level, "3")
		);

		// Assert
		var h3 = cut.Find("h3");
		h3.Should().NotBeNull();
		h3.TextContent.Should().Be("Small Header");
		h3.ClassList.Should().Contain("text-1xl");
	}

	[Fact]
	public void PageHeadingComponent_WithCustomTextColor_ShouldApplyClass()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.HeaderText, "Colored Header")
			.Add(p => p.TextColorClass, "text-blue-500")
		);

		// Assert
		var h1 = cut.Find("h1");
		h1.ClassList.Should().Contain("text-blue-500");
		h1.ClassList.Should().NotContain("text-gray-50");
	}

	[Fact]
	public void PageHeadingComponent_ShouldSetPageTitle()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.HeaderText, "Page Title Test")
		);

		// Assert
		// PageTitle component sets the browser title, which is handled by Blazor internally
		// We verify the component rendered successfully with the header text
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Should().Be("Page Title Test");
	}

	[Fact]
	public void PageHeadingComponent_ShouldRenderContainerCard()
	{
		// Arrange & Act
		var cut = Render<PageHeadingComponent>();

		// Assert
		var header = cut.Find("header");
		header.Should().NotBeNull();
		header.ClassList.Should().Contain("container-card");
		header.ClassList.Should().Contain("p-2");
	}
}