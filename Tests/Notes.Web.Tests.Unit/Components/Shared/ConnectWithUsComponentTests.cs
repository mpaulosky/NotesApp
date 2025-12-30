using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class ConnectWithUsComponentTests : BunitContext
{
	[Fact]
	public void ConnectWithUsComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveHeading()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var heading = cut.FindComponent<ComponentHeadingComponent>();
		heading.Should().NotBeNull();
		heading.Instance.HeaderText.Should().Be("Connect With Us");
		heading.Instance.Level.Should().Be("1");
		heading.Instance.TextColorClass.Should().Be("text-gray-50");
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveThreeSocialLinks()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var navLinks = cut.FindAll("a[target='_blank']");
		navLinks.Count.Should().Be(3);
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveThreadsLink()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var threadsLink = cut.Find("a[href='https://www.threads/']");
		threadsLink.Should().NotBeNull();
		threadsLink.GetAttribute("target").Should().Be("_blank");

		var icon = threadsLink.QuerySelector("i.ri-threads-line");
		icon.Should().NotBeNull();
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveInstagramLink()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var instagramLink = cut.Find("a[href='https://www.instagram.com/']");
		instagramLink.Should().NotBeNull();
		instagramLink.GetAttribute("target").Should().Be("_blank");

		var icon = instagramLink.QuerySelector("i.ri-instagram-line");
		icon.Should().NotBeNull();
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveYouTubeLink()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var youtubeLink = cut.Find("a[href='https://www.youtube.com/']");
		youtubeLink.Should().NotBeNull();
		youtubeLink.GetAttribute("target").Should().Be("_blank");

		var icon = youtubeLink.QuerySelector("i.ri-youtube-line");
		icon.Should().NotBeNull();
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveIconStyling()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var icons = cut.FindAll("i.text-3xl");
		icons.Count.Should().Be(3);
	}

	[Fact]
	public void ConnectWithUsComponent_ShouldHaveFlexLayout()
	{
		// Arrange & Act
		var cut = Render<ConnectWithUsComponent>();

		// Assert
		var flexContainer = cut.Find("div.flex.flex-col.gap-4");
		flexContainer.Should().NotBeNull();
	}
}
