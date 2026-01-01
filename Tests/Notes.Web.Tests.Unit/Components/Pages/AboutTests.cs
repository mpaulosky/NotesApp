using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class AboutTests : BunitContext
{
	[Fact]
	public void About_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void About_ShouldDisplayPageHeading()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var heading = cut.FindComponent<PageHeadingComponent>();
		heading.Should().NotBeNull();
	}

	[Fact]
	public void About_ShouldDisplayPageTitle()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("About - Article Site");
	}

	[Fact]
	public void About_ShouldDisplayProjectDescription()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("Article Site");
		cut.Markup.Should().Contain("open-source blogging platform");
		cut.Markup.Should().Contain(".NET Aspire");
		cut.Markup.Should().Contain("Blazor");
	}

	[Fact]
	public void About_ShouldDisplayFeatureList()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var list = cut.Find("ul.list-disc");
		list.Should().NotBeNull();
		cut.Markup.Should().Contain("Built with .NET 9");
		cut.Markup.Should().Contain("TailwindCSS");
		cut.Markup.Should().Contain("Auth0");
		cut.Markup.Should().Contain("Clean Architecture");
	}

	[Fact]
	public void About_ShouldHaveGitHubLink()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var links = cut.FindAll("a");
		bool hasGitHubLink = links.Any(a => a.GetAttribute("href") != null && a.GetAttribute("href").Contains("github.com"));
		hasGitHubLink.Should().BeTrue();
		cut.Markup.Should().Contain("Source code:");
	}

	[Fact]
	public void About_ShouldHaveContainerCardStyling()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var container = cut.Find("div.container-card");
		container.Should().NotBeNull();
	}
}
