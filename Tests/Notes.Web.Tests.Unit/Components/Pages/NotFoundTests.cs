using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class NotFoundTests : BunitContext
{
	[Fact]
	public void NotFound_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void NotFound_ShouldDisplayHeading()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var heading = cut.Find("h3");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Not Found");
	}

	[Fact]
	public void NotFound_ShouldDisplayApologyMessage()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var message = cut.Find("p");
		message.Should().NotBeNull();
		message.TextContent.Should().Contain("Sorry");
		message.TextContent.Should().Contain("does not exist");
	}
}
