using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class TextEditorTests : BunitContext
{
	[Fact]
	public void TextEditor_ShouldRender()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, ""));

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void TextEditor_ShouldShowPlaceholderMessage()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, ""));

		// Assert
		var placeholder = cut.Find("div.text-editor-placeholder");
		placeholder.Should().NotBeNull();

		var message = placeholder.QuerySelector("p");
		message.Should().NotBeNull();
		message!.TextContent.Should().Contain("TextEditor component placeholder");
		message.TextContent.Should().Contain("requires PSC Markdown Editor package");
	}

	[Fact]
	public void TextEditor_ShouldHaveTextarea()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, "Test content"));

		// Assert
		var textarea = cut.Find("textarea.form-control");
		textarea.Should().NotBeNull();
		textarea.GetAttribute("rows").Should().Be("10");
	}

	[Fact]
	public void TextEditor_ShouldBindContent()
	{
		// Arrange
		var initialContent = "Initial content";

		// Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, initialContent));

		// Assert
		var textarea = cut.Find("textarea");
		textarea.GetAttribute("value").Should().Be(initialContent);
	}

	[Fact]
	public void TextEditor_ShouldAcceptEmptyContent()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, false)
			.Add(p => p.Content, ""));

		// Assert
		var textarea = cut.Find("textarea");
		textarea.GetAttribute("value").Should().BeEmpty();
	}

	[Fact]
	public void TextEditor_ShouldAcceptAlignmentOptionsEnabledTrue()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, ""));

		// Assert
		cut.Should().NotBeNull();
		cut.Instance.AlignmentOptionsEnabled.Should().BeTrue();
	}

	[Fact]
	public void TextEditor_ShouldAcceptAlignmentOptionsEnabledFalse()
	{
		// Arrange & Act
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, false)
			.Add(p => p.Content, ""));

		// Assert
		cut.Should().NotBeNull();
		cut.Instance.AlignmentOptionsEnabled.Should().BeFalse();
	}
}
