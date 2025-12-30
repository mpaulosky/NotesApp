using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Layout;

[ExcludeFromCodeCoverage]
public class ReconnectModalTests : BunitContext
{
	[Fact]
	public void ReconnectModal_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldHaveDialogElement()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var dialog = cut.Find("dialog#components-reconnect-modal");
		dialog.Should().NotBeNull();
		dialog.GetAttribute("data-nosnippet").Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldHaveReconnectContainer()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var container = cut.Find("div.components-reconnect-container");
		container.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldHaveRejoiningAnimation()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var animation = cut.Find("div.components-rejoining-animation");
		animation.Should().NotBeNull();
		animation.GetAttribute("aria-hidden").Should().Be("true");
	}

	[Fact]
	public void ReconnectModal_ShouldHaveFirstAttemptMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find("p.components-reconnect-first-attempt-visible");
		message.Should().NotBeNull();
		message.TextContent.Trim().Should().Be("Rejoining the server...");
	}

	[Fact]
	public void ReconnectModal_ShouldHaveRepeatedAttemptMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find("p.components-reconnect-repeated-attempt-visible");
		message.Should().NotBeNull();
		message.TextContent.Should().Contain("Rejoin failed");

		var secondsSpan = cut.Find("span#components-seconds-to-next-attempt");
		secondsSpan.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldHaveFailedMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find("p.components-reconnect-failed-visible");
		message.Should().NotBeNull();
		message.InnerHtml.Should().Contain("Failed to rejoin");
	}

	[Fact]
	public void ReconnectModal_ShouldHaveRetryButton()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var button = cut.Find("button#components-reconnect-button");
		button.Should().NotBeNull();
		button.ClassList.Should().Contain("components-reconnect-failed-visible");
		button.TextContent.Trim().Should().Be("Retry");
	}

	[Fact]
	public void ReconnectModal_ShouldHavePauseMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find("p.components-pause-visible");
		message.Should().NotBeNull();
		message.TextContent.Should().Contain("session has been paused");
	}

	[Fact]
	public void ReconnectModal_ShouldHaveResumeButton()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var button = cut.Find("button#components-resume-button");
		button.Should().NotBeNull();
		button.ClassList.Should().Contain("components-pause-visible");
		button.TextContent.Trim().Should().Be("Resume");
	}

	[Fact]
	public void ReconnectModal_ShouldHaveResumeFailedMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find("p.components-resume-failed-visible");
		message.Should().NotBeNull();
		message.InnerHtml.Should().Contain("Failed to resume");
	}
}
