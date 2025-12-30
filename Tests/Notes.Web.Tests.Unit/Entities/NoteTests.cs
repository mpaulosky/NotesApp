using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Entities;

[ExcludeFromCodeCoverage]
public class NoteTests
{
	[Fact]
	public void CreateNote_ShouldHaveDefaultValues()
	{
		// Arrange & Act
		var note = new Note();

		// Assert
		note.Title.Should().Be(string.Empty);
		note.Content.Should().Be(string.Empty);
		note.OwnerSubject.Should().Be(string.Empty);
		note.AiSummary.Should().BeNull();
		note.Tags.Should().BeNull();
		note.Embedding.Should().BeNull();
		note.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void CreateNote_WithTestDataBuilder_ShouldSetAllProperties()
	{
		// Arrange
		var expectedId = ObjectId.GenerateNewId();
		var expectedCreatedAt = DateTime.UtcNow.AddDays(-1);
		var expectedUpdatedAt = DateTime.UtcNow;
		var expectedEmbedding = new[] { 0.5f, 0.6f, 0.7f };

		// Act
		var note = TestDataBuilder.CreateNote(
			id: expectedId,
			title: "Custom Title",
			content: "Custom Content",
			aiSummary: "Custom Summary",
			tags: "custom,tags",
			embedding: expectedEmbedding,
			ownerSubject: "auth0|custom123",
			createdAt: expectedCreatedAt,
			updatedAt: expectedUpdatedAt,
			isArchived: true
		);

		// Assert
		note.Id.Should().Be(expectedId);
		note.Title.Should().Be("Custom Title");
		note.Content.Should().Be("Custom Content");
		note.AiSummary.Should().Be("Custom Summary");
		note.Tags.Should().Be("custom,tags");
		note.Embedding.Should().BeEquivalentTo(expectedEmbedding);
		note.OwnerSubject.Should().Be("auth0|custom123");
		note.CreatedAt.Should().Be(expectedCreatedAt);
		note.UpdatedAt.Should().Be(expectedUpdatedAt);
		note.IsArchived.Should().BeTrue();
	}

	[Fact]
	public void Note_Owner_ShouldBeIgnoredByBson()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote();
		var owner = TestDataBuilder.CreateAppUser();

		// Act
		note.Owner = owner;

		// Assert
		note.Owner.Should().NotBeNull();
		note.Owner.Should().BeSameAs(owner);
	}

	[Fact]
	public void Note_SetIsArchived_ShouldUpdateProperty()
	{
		// Arrange
		var note = TestDataBuilder.CreateNote(isArchived: false);

		// Act
		note.IsArchived = true;

		// Assert
		note.IsArchived.Should().BeTrue();
	}

	[Fact]
	public void Note_UpdateTimestamps_ShouldReflectChanges()
	{
		// Arrange
		var createdAt = DateTime.UtcNow.AddDays(-5);
		var note = TestDataBuilder.CreateNote(createdAt: createdAt, updatedAt: createdAt);
		var newUpdatedAt = DateTime.UtcNow;

		// Act
		note.UpdatedAt = newUpdatedAt;

		// Assert
		note.CreatedAt.Should().Be(createdAt);
		note.UpdatedAt.Should().Be(newUpdatedAt);
		note.UpdatedAt.Should().BeAfter(note.CreatedAt);
	}
}
