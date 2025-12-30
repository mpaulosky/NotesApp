using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class RecentRelatedComponentTests : BunitContext
{
	[Fact]
	public void RecentRelatedComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void RecentRelatedComponent_ShouldHaveHeading()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var heading = cut.FindComponent<ComponentHeadingComponent>();
		heading.Should().NotBeNull();
		heading.Instance.HeaderText.Should().Be("Recent Posts");
		heading.Instance.Level.Should().Be("1");
		heading.Instance.TextColorClass.Should().Be("text-gray-50");
	}

	[Fact]
	public void RecentRelatedComponent_ShouldHaveTwoPostLinks()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var links = cut.FindAll("a[target='_blank']");
		links.Count.Should().Be(2);
	}

	[Fact]
	public void RecentRelatedComponent_ShouldHaveSQLServerOnMacLink()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var link = cut.Find("a[href='/run-sql-server-on-m1-or-m2-macbook']");
		link.Should().NotBeNull();
		link.TextContent.Should().Contain("Run SQL Server on M1 or M2 Macbook");
		link.GetAttribute("target").Should().Be("_blank");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}

	[Fact]
	public void RecentRelatedComponent_ShouldHavePostgresOnGoogleCloudLink()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var links = cut.FindAll("a[href='/run-sql-server-on-m1-or-m2-macbook']");
		links.Count.Should().Be(2); // Both links have same href (appears to be placeholder)

		var secondLink = links[1];
		secondLink.TextContent.Should().Contain("Run a Postgres Database for Free on Google Cloud");
	}

	[Fact]
	public void RecentRelatedComponent_ShouldHaveFlexLayout()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var flexContainer = cut.Find("div.flex.flex-col.gap-4");
		flexContainer.Should().NotBeNull();
	}

	[Fact]
	public void RecentRelatedComponent_LinksShouldHaveHoverEffect()
	{
		// Arrange & Act
		var cut = Render<RecentRelatedComponent>();

		// Assert
		var links = cut.FindAll("a.hover\\:text-blue-700");
		links.Count.Should().Be(2);
	}
}
