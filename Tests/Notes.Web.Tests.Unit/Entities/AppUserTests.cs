using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Entities;

[ExcludeFromCodeCoverage]
public class AppUserTests
{
	[Fact]
	public void CreateAppUser_ShouldHaveDefaultValues()
	{
		// Arrange & Act
		var user = new AppUser();

		// Assert
		user.Auth0Subject.Should().BeNull();
		user.Name.Should().BeNull();
		user.Email.Should().BeNull();
		user.Notes.Should().NotBeNull();
		user.Notes.Should().BeEmpty();
		user.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void CreateAppUser_WithTestDataBuilder_ShouldSetAllProperties()
	{
		// Arrange
		var expectedCreatedUtc = DateTime.UtcNow.AddDays(-10);

		// Act
		var user = TestDataBuilder.CreateAppUser(
			auth0Subject: "auth0|custom456",
			email: "custom@test.com",
			name: "Custom User",
			createdUtc: expectedCreatedUtc
		);

		// Assert
		user.Auth0Subject.Should().Be("auth0|custom456");
		user.Email.Should().Be("custom@test.com");
		user.Name.Should().Be("Custom User");
		user.CreatedUtc.Should().Be(expectedCreatedUtc);
		user.Notes.Should().NotBeNull();
	}

	[Fact]
	public void AppUser_AddNote_ShouldUpdateNotesCollection()
	{
		// Arrange
		var user = TestDataBuilder.CreateAppUser();
		var note = TestDataBuilder.CreateNote(ownerSubject: user.Auth0Subject);

		// Act
		user.Notes.Add(note);

		// Assert
		user.Notes.Should().HaveCount(1);
		user.Notes.Should().Contain(note);
	}

	[Fact]
	public void AppUser_Notes_ShouldBeModifiableCollection()
	{
		// Arrange
		var user = TestDataBuilder.CreateAppUser();
		var note1 = TestDataBuilder.CreateNote(title: "Note 1");
		var note2 = TestDataBuilder.CreateNote(title: "Note 2");

		// Act
		user.Notes.Add(note1);
		user.Notes.Add(note2);

		// Assert
		user.Notes.Should().HaveCount(2);
		user.Notes.Should().ContainInOrder(note1, note2);
	}
}
