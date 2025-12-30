using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class PageHeaderComponentTests : BunitContext
{
	[Fact]
	public void PageHeaderComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void PageHeaderComponent_ShouldRenderDefaultH1()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>();

		// Assert
		var heading = cut.Find("h1");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("My Blog");
		heading.ClassList.Should().Contain("text-2xl");
		heading.ClassList.Should().Contain("font-bold");
		heading.ClassList.Should().Contain("tracking-tight");
		heading.ClassList.Should().Contain("text-gray-50");
	}

	[Fact]
	public void PageHeaderComponent_ShouldRenderH1WhenLevel1()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "1")
			.Add(p => p.HeaderText, "Custom Title"));

		// Assert
		var heading = cut.Find("h1");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Custom Title");
		heading.ClassList.Should().Contain("text-2xl");
	}

	[Fact]
	public void PageHeaderComponent_ShouldRenderH2WhenLevel2()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "2")
			.Add(p => p.HeaderText, "Level 2 Header"));

		// Assert
		var heading = cut.Find("h2");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 2 Header");
		heading.ClassList.Should().Contain("text-3xl");
	}

	[Fact]
	public void PageHeaderComponent_ShouldRenderH3WhenLevel3()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "3")
			.Add(p => p.HeaderText, "Level 3 Header"));

		// Assert
		var heading = cut.Find("h3");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 3 Header");
		heading.ClassList.Should().Contain("text-2xl");
	}

	[Fact]
	public void PageHeaderComponent_ShouldHaveHeaderElement()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>();

		// Assert
		var header = cut.Find("header");
		header.Should().NotBeNull();
		header.ClassList.Should().Contain("shadow-lg");
	}

	[Fact]
	public void PageHeaderComponent_ShouldHaveContainerCard()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>();

		// Assert
		var container = cut.Find("div.container-card");
		container.Should().NotBeNull();
		container.ClassList.Should().Contain("p-2");
	}

	[Fact]
	public void PageHeaderComponent_AllHeadingsShouldHaveTextGray50()
	{
		// Arrange & Act - Test all levels
		var cut1 = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "1"));
		var cut2 = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "2"));
		var cut3 = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "3"));

		// Assert
		cut1.Find("h1").ClassList.Should().Contain("text-gray-50");
		cut2.Find("h2").ClassList.Should().Contain("text-gray-50");
		cut3.Find("h3").ClassList.Should().Contain("text-gray-50");
	}

	[Fact]
	public void PageHeaderComponent_AllHeadingsShouldBeBoldAndTracked()
	{
		// Arrange & Act
		var cut = Render<PageHeaderComponent>(parameters => parameters
			.Add(p => p.Level, "2"));

		// Assert
		var heading = cut.Find("h2");
		heading.ClassList.Should().Contain("font-bold");
		heading.ClassList.Should().Contain("tracking-tight");
	}
}
