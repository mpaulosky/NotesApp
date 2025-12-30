using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class HomeTests : BunitContext
{
	public HomeTests()
	{
		var logger = Substitute.For<ILogger<Home>>();
		Services.AddSingleton(logger);
	}

	[Fact]
	public void Home_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Home_ShouldDisplayPageHeading()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.FindComponent<PageHeadingComponent>().Should().NotBeNull();
	}

	[Fact]
	public void Home_ShouldDisplayWelcomeMessage()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Markup.Should().Contain("Welcome to the Notes App");
	}

	[Fact]
	public void Home_ShouldDisplayHeroSection()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		var heroSection = cut.Find("section.relative");
		heroSection.Should().NotBeNull();
	}

	[Fact]
	public void Home_ShouldHaveBrowseNotesLink()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		var links = cut.FindAll("a");
		links.Should().Contain(a => a.GetAttribute("href") == "/notes");
		cut.Markup.Should().Contain("Browse All Notes");
	}

	[Fact]
	public void Home_ShouldHaveLearnMoreLink()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		var links = cut.FindAll("a");
		links.Should().Contain(a => a.GetAttribute("href") == "/about");
		cut.Markup.Should().Contain("Learn More");
	}

	[Fact]
	public void Home_ShouldHaveWelcomeSection()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Markup.Should().Contain("Welcome");
		cut.Markup.Should().Contain("Start creating your notes");
	}
}