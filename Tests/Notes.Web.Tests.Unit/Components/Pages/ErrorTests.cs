using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components.Web;

namespace Notes.Web.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class ErrorTests : BunitContext
{
	[Fact]
	public void Error_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Error_ShouldHavePageTitle()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var pageTitle = cut.FindComponent<PageTitle>();
		pageTitle.Should().NotBeNull();
		// PageTitle sets browser title internally, cannot be directly asserted from markup
	}

	[Fact]
	public void Error_ShouldDisplayErrorHeading()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var heading = cut.Find("h1.text-danger");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Error.");
	}

	[Fact]
	public void Error_ShouldDisplayErrorMessage()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var message = cut.Find("h2.text-danger");
		message.Should().NotBeNull();
		message.TextContent.Should().Contain("An error occurred while processing your request");
	}

	[Fact]
	public void Error_ShouldNotShowRequestIdByDefault()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var markup = cut.Markup;
		markup.Should().NotContain("Request ID:");
	}

	[Fact]
	public void Error_ShouldHaveDevelopmentModeSection()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var heading = cut.Find("h3");
		heading.Should().NotBeNull();
		heading.TextContent.Should().Be("Development Mode");
	}

	[Fact]
	public void Error_ShouldWarnAboutDevelopmentEnvironment()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("Development environment shouldn't be enabled for deployed applications");
		markup.Should().Contain("sensitive information");
	}

	[Fact]
	public void Error_ShouldMentionASPNETCOREENVIRONMENT()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var markup = cut.Markup;
		markup.Should().Contain("ASPNETCORE_ENVIRONMENT");
		markup.Should().Contain("Development");
	}

	[Fact]
	public void Error_ShouldHaveMultipleParagraphs()
	{
		// Arrange & Act
		var cut = Render<Error>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs.Count.Should().BeGreaterOrEqualTo(2);
	}
}
