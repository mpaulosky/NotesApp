using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components.Web;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class ErrorPageComponentTests : BunitContext
{
	[Fact]
	public void ErrorPageComponent_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void ErrorPageComponent_ShouldDisplay401Unauthorized()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 401));

		// Assert
		var heading = cut.Find("h1");
		heading.TextContent.Should().Be("401 Unauthorized");

		var message = cut.Find("p");
		message.TextContent.Should().Contain("not authorized to view this page");
	}

	[Fact]
	public void ErrorPageComponent_ShouldDisplay403Forbidden()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 403));

		// Assert
		var heading = cut.Find("h1");
		heading.TextContent.Should().Be("403 Forbidden");

		var message = cut.Find("p");
		message.TextContent.Should().Contain("Access to this resource is forbidden");
	}

	[Fact]
	public void ErrorPageComponent_ShouldDisplay404NotFound()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		var heading = cut.Find("h1");
		heading.TextContent.Should().Be("404 Not Found");

		var message = cut.Find("p");
		message.TextContent.Should().Contain("page you are looking for does not exist");
	}

	[Fact]
	public void ErrorPageComponent_ShouldDisplay500InternalServerError()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 500));

		// Assert
		var heading = cut.Find("h1");
		heading.TextContent.Should().Be("500 Internal Server Error");

		var message = cut.Find("p");
		message.TextContent.Should().Contain("unexpected error occured on the server");
	}

	[Fact]
	public void ErrorPageComponent_ShouldDisplayUnknownErrorForInvalidCode()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 999));

		// Assert
		var heading = cut.Find("h1");
		heading.TextContent.Should().Be("Unknown Error");

		var message = cut.Find("p");
		message.TextContent.Should().Contain("error occurred. Please try again later");
	}

	[Fact]
	public void ErrorPageComponent_ShouldHaveGoHomeLink()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		var link = cut.Find("a[href='/']");
		link.Should().NotBeNull();
		link.TextContent.Should().Be("Go Home");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}

	[Fact]
	public void ErrorPageComponent_ShouldApplyDefaultShadowStyle()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		var header = cut.Find("header");
		header.ClassList.Should().Contain("shadow-red-500");
	}

	[Fact]
	public void ErrorPageComponent_ShouldApplyCustomShadowStyle()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404)
			.Add(p => p.ShadowStyle, "shadow-blue-500"));

		// Assert
		var header = cut.Find("header");
		header.ClassList.Should().Contain("shadow-blue-500");
	}

	[Fact]
	public void ErrorPageComponent_ShouldApplyDefaultTextColor()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		var heading = cut.Find("h1");
		heading.GetAttribute("class").Should().Contain("red-600");
	}

	[Fact]
	public void ErrorPageComponent_ShouldApplyCustomTextColor()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404)
			.Add(p => p.TextColor, "blue-600"));

		// Assert
		var heading = cut.Find("h1");
		heading.GetAttribute("class").Should().Contain("blue-600");
	}

	[Fact]
	public void ErrorPageComponent_ShouldHaveHeaderStyling()
	{
		// Arrange & Act
		var cut = Render<ErrorPageComponent>(parameters => parameters
			.Add(p => p.ErrorCode, 404));

		// Assert
		var header = cut.Find("header");
		header.ClassList.Should().Contain("container");
		header.ClassList.Should().Contain("flex");
		header.ClassList.Should().Contain("border-b");
		header.ClassList.Should().Contain("border-blue-700");
		header.ClassList.Should().Contain("shadow-md");
	}
}
