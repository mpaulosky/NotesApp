using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class ComponentHeadingComponentTests : BunitContext
{
	[Fact]
	public void ComponentHeadingComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderDefaultHeading()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>();

		// Assert
		var heading = cut.Find("h3");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("My Component");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderH1WhenLevel1()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "1")
			.Add(p => p.HeaderText, "Test Heading"));

		// Assert
		var heading = cut.Find("h1");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Test Heading");
		heading.ClassList.Should().Contain("text-2xl");
		heading.ClassList.Should().Contain("font-semibold");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderH2WhenLevel2()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "2")
			.Add(p => p.HeaderText, "Level 2"));

		// Assert
		var heading = cut.Find("h2");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 2");
		heading.ClassList.Should().Contain("text-xl");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderH3WhenLevel3()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "3")
			.Add(p => p.HeaderText, "Level 3"));

		// Assert
		var heading = cut.Find("h3");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 3");
		heading.ClassList.Should().Contain("text-lg");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderH4WhenLevel4()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "4")
			.Add(p => p.HeaderText, "Level 4"));

		// Assert
		var heading = cut.Find("h4");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 4");
		heading.ClassList.Should().Contain("text-md");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldRenderH5WhenLevel5()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "5")
			.Add(p => p.HeaderText, "Level 5"));

		// Assert
		var heading = cut.Find("h5");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Level 5");
		heading.ClassList.Should().Contain("text-sm");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldApplyCustomTextColor()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "1")
			.Add(p => p.HeaderText, "Custom Color")
			.Add(p => p.TextColorClass, "text-blue-500"));

		// Assert
		var heading = cut.Find("h1");
		heading.ClassList.Should().Contain("text-blue-500");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldApplyDefaultTextColor()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "1")
			.Add(p => p.HeaderText, "Default Color"));

		// Assert
		var heading = cut.Find("h1");
		heading.ClassList.Should().Contain("text-gray-50");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldHaveHeaderElement()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>();

		// Assert
		var header = cut.Find("header");
		header.Should().NotBeNull();
		header.ClassList.Should().Contain("mb-6");
		header.ClassList.Should().Contain("py-4");
	}

	[Fact]
	public void ComponentHeadingComponent_ShouldHaveTrackingTightClass()
	{
		// Arrange & Act
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "2"));

		// Assert
		var heading = cut.Find("h2");
		heading.ClassList.Should().Contain("tracking-tight");
		heading.ClassList.Should().Contain("py-4");
	}
}
