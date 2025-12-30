using System.Diagnostics.CodeAnalysis;

using Blazored.LocalStorage;

namespace Notes.Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class ThemeToggleTests : BunitContext
{
	public ThemeToggleTests()
	{
		var localStorage = Substitute.For<ILocalStorageService>();
		Services.AddSingleton(localStorage);

		// Setup JSInterop for ThemeToggle component
		var themeModule = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		themeModule.SetupVoid("setTheme", _ => true);
	}

	[Fact]
	public void ThemeToggle_ShouldRenderToggleSwitch()
	{
		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var toggle = cut.Find("input#toggle");
		toggle.Should().NotBeNull();
		toggle.GetAttribute("type").Should().Be("checkbox");
	}

	[Fact]
	public void ThemeToggle_ShouldDisplayLightAndDarkLabels()
	{
		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var spans = cut.FindAll("span");
		spans.Should().HaveCountGreaterThanOrEqualTo(2);
		cut.Markup.Should().Contain("Light");
		cut.Markup.Should().Contain("Dark");
	}

	[Fact]
	public void ThemeToggle_ShouldHaveHiddenCheckbox()
	{
		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var checkbox = cut.Find("input#toggle");
		checkbox.ClassList.Should().Contain("hidden");
	}

	[Fact]
	public void ThemeToggle_ShouldHaveCorrectContainerStyling()
	{
		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var container = cut.Find("div");
		container.ClassList.Should().Contain("flex");
		container.ClassList.Should().Contain("justify-end");
		container.ClassList.Should().Contain("items-center");
	}

	[Fact]
	public void ThemeToggle_ShouldHaveToggleLabel()
	{
		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var label = cut.Find("label[for='toggle']");
		label.Should().NotBeNull();
		label.ClassList.Should().Contain("cursor-pointer");
		label.ClassList.Should().Contain("rounded-full");
	}
}