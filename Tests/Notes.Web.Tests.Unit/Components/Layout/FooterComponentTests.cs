using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Layout;

[ExcludeFromCodeCoverage]
public class FooterComponentTests : BunitContext
{
	[Fact]
	public void FooterComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<FooterComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void FooterComponent_ShouldDisplayCompanyName()
	{
		// Arrange & Act
		var cut = Render<FooterComponent>();

		// Assert
		cut.Markup.Should().Contain("MPaulosky Co.");
		cut.Markup.Should().Contain("All rights reserved.");
	}

	[Fact]
	public void FooterComponent_ShouldDisplayCurrentYear()
	{
		// Arrange
		var currentYear = DateTime.Now.Year.ToString();

		// Act
		var cut = Render<FooterComponent>();

		// Assert
		cut.Markup.Should().Contain($"©{currentYear}");
	}

	[Fact]
	public void FooterComponent_ShouldHaveCorrectStyling()
	{
		// Arrange & Act
		var cut = Render<FooterComponent>();

		// Assert
		var container = cut.Find("div");
		container.ClassList.Should().Contain("container-card");
		container.ClassList.Should().Contain("p-2");
		container.ClassList.Should().Contain("text-center");
	}

	[Fact]
	public void FooterComponent_ShouldHaveLinkToHomePage()
	{
		// Arrange & Act
		var cut = Render<FooterComponent>();

		// Assert
		var link = cut.Find("a");
		link.GetAttribute("href").Should().Be("/");
	}

	[Fact]
	public void FooterComponent_LinkShouldHaveTransitionStyling()
	{
		// Arrange & Act
		var cut = Render<FooterComponent>();

		// Assert
		var link = cut.Find("a");
		link.ClassList.Should().Contain("transition-colors");
		link.ClassList.Should().Contain("duration-200");
	}
}
