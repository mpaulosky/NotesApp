using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class ContactTests : BunitContext
{
	[Fact]
	public void Contact_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Contact_ShouldDisplayPageHeading()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var heading = cut.FindComponent<PageHeadingComponent>();
		heading.Should().NotBeNull();
	}

	[Fact]
	public void Contact_ShouldDisplayPageTitle()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		cut.Markup.Should().Contain("Contacts - Article Site");
	}

	[Fact]
	public void Contact_ShouldDisplayContactIntroduction()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		cut.Markup.Should().Contain("For questions, support, or feedback");
		cut.Markup.Should().Contain("ArticlesSite");
	}

	[Fact]
	public void Contact_ShouldHaveEmailLink()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var emailLinks = cut.FindAll("a[href^='mailto:']");
		emailLinks.Should().HaveCountGreaterThan(0);
		cut.Markup.Should().Contain("mpaulosky@gmail.com");
	}

	[Fact]
	public void Contact_ShouldHaveGitHubIssuesLink()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var links = cut.FindAll("a");
		var hasGitHubLink = links.Any(a => a.GetAttribute("href") != null && a.GetAttribute("href")!.Contains("github.com"));
		hasGitHubLink.Should().BeTrue();
		cut.Markup.Should().Contain("GitHub Issues");
	}

	[Fact]
	public void Contact_ShouldHaveContributingGuideLink()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var links = cut.FindAll("a");
		var hasContributingLink = links.Any(a => a.GetAttribute("href") != null && a.GetAttribute("href")!.Contains("CONTRIBUTING"));
		hasContributingLink.Should().BeTrue();
		cut.Markup.Should().Contain("contributing guide");
	}

	[Fact]
	public void Contact_ShouldHaveContainerCardStyling()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var container = cut.Find("div.container-card");
		container.Should().NotBeNull();
	}

	[Fact]
	public void Contact_ShouldHaveContactMethodsList()
	{
		// Arrange & Act
		var cut = Render<Contact>();

		// Assert
		var list = cut.Find("ul.list-disc");
		list.Should().NotBeNull();
		var listItems = cut.FindAll("li");
		listItems.Should().HaveCountGreaterThanOrEqualTo(2);
	}
}