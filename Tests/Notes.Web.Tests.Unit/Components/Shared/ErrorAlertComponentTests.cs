using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class ErrorAlertComponentTests : BunitContext
{
	[Fact]
	public void ErrorAlertComponent_WithDefaultProps_ShouldRenderDefaultValues()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		var title = cut.Find("h3");
		title.TextContent.Should().Be("Error");

		var message = cut.Find("div.text-red-700");
		message.TextContent.Trim().Should().Be("An error occurred.");
	}

	[Fact]
	public void ErrorAlertComponent_WithCustomTitle_ShouldDisplayCustomTitle()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Title, "Custom Error Title")
		);

		// Assert
		var title = cut.Find("h3");
		title.TextContent.Should().Be("Custom Error Title");
	}

	[Fact]
	public void ErrorAlertComponent_WithCustomMessage_ShouldDisplayCustomMessage()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Message, "Custom error message")
		);

		// Assert
		var message = cut.Find("div.text-red-700");
		message.TextContent.Trim().Should().Be("Custom error message");
	}

	[Fact]
	public void ErrorAlertComponent_WithChildContent_ShouldRenderChildContent()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.ChildContent, builder =>
			{
				builder.AddContent(0, "Custom child content");
			})
		);

		// Assert
		var message = cut.Find("div.text-red-700");
		message.TextContent.Trim().Should().Be("Custom child content");
	}

	[Fact]
	public void ErrorAlertComponent_ShouldHaveErrorStyling()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		var container = cut.Find("div.bg-red-50");
		container.Should().NotBeNull();
		container.ClassList.Should().Contain("border-red-200");
		container.ClassList.Should().Contain("rounded-md");
	}

	[Fact]
	public void ErrorAlertComponent_ShouldDisplayErrorIcon()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		var svg = cut.Find("svg");
		svg.Should().NotBeNull();
		svg.ClassList.Should().Contain("text-red-500");
	}

	[Fact]
	public void ErrorAlertComponent_WhenChildContentProvided_ShouldNotDisplayMessage()
	{
		// Arrange & Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Message, "This should not appear")
			.Add(p => p.ChildContent, builder =>
			{
				builder.AddContent(0, "Child content");
			})
		);

		// Assert
		var message = cut.Find("div.text-red-700");
		message.TextContent.Trim().Should().Be("Child content");
		message.TextContent.Should().NotContain("This should not appear");
	}
}