using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Microsoft.Extensions.Options;

using MongoDB.Bson;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Services.Ai;

[ExcludeFromCodeCoverage]
public class OpenAiServiceTests
{
	private readonly IOptions<AiServiceOptions> _options;
	private readonly INoteRepository _repository;
	private readonly IChatClientWrapper _chatClient;
	private readonly IEmbeddingClientWrapper _embeddingClient;
	private readonly OpenAiService _service;

	public OpenAiServiceTests()
	{
		_options = Options.Create(new AiServiceOptions
		{
			MaxSummaryTokens = 100,
			SimilarityThreshold = 0.7f,
			RelatedNotesCount = 5,
			EmbeddingModel = "text-embedding-3-small",
			ChatModel = "gpt-4"
		});
		_repository = Substitute.For<INoteRepository>();
		_chatClient = Substitute.For<IChatClientWrapper>();
		_embeddingClient = Substitute.For<IEmbeddingClientWrapper>();
		_service = new OpenAiService(_options, _repository, _chatClient, _embeddingClient);
	}

	#region Constructor Tests

	[Fact]
	public void Constructor_WithNullChatClient_ShouldThrowArgumentNullException()
	{
		// Act
		Action act = () => new OpenAiService(_options, _repository, null!, _embeddingClient);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("chatClient");
	}

	[Fact]
	public void Constructor_WithNullEmbeddingClient_ShouldThrowArgumentNullException()
	{
		// Act
		Action act = () => new OpenAiService(_options, _repository, _chatClient, null!);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("embeddingClient");
	}

	#endregion

	#region GenerateSummaryAsync Tests - Edge Cases Only

	[Fact]
	public async Task GenerateSummaryAsync_WithNullContent_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateSummaryAsync(null!);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithEmptyContent_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateSummaryAsync(string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithWhitespace_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateSummaryAsync("   ");

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GenerateEmbeddingAsync Tests - Edge Cases Only

	[Fact]
	public async Task GenerateEmbeddingAsync_WithNullText_ShouldReturnEmptyArray()
	{
		// Act
		var result = await _service.GenerateEmbeddingAsync(null!);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithEmptyText_ShouldReturnEmptyArray()
	{
		// Act
		var result = await _service.GenerateEmbeddingAsync(string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithWhitespace_ShouldReturnEmptyArray()
	{
		// Act
		var result = await _service.GenerateEmbeddingAsync("   ");

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GenerateTagsAsync Tests - Edge Cases Only

	[Fact]
	public async Task GenerateTagsAsync_WithBothNullInputs_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateTagsAsync(null!, null!);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateTagsAsync_WithEmptyTitleAndContent_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateTagsAsync(string.Empty, string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateTagsAsync_WithWhitespaceTitleAndContent_ShouldReturnEmptyString()
	{
		// Act
		var result = await _service.GenerateTagsAsync("   ", "   ");

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region FindRelatedNotesAsync Tests - Full Coverage

	[Fact]
	public async Task FindRelatedNotesAsync_WithNullEmbedding_ShouldReturnEmptyList()
	{
		// Act
		var result = await _service.FindRelatedNotesAsync(null!, "user@example.com");

		// Assert
		result.Should().BeEmpty();
		await _repository.DidNotReceive().GetNotes(
			Arg.Any<Expression<Func<Note, bool>>>());
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithEmptyEmbedding_ShouldReturnEmptyList()
	{
		// Act
		var result = await _service.FindRelatedNotesAsync(Array.Empty<float>(), "user@example.com");

		// Assert
		result.Should().BeEmpty();
		await _repository.DidNotReceive().GetNotes(
			Arg.Any<Expression<Func<Note, bool>>>());
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WhenRepositoryFails_ShouldReturnEmptyList()
	{
		// Arrange
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Fail<IEnumerable<Note>?>("Database error")));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WhenRepositoryReturnsNull_ShouldReturnEmptyList()
	{
		// Arrange
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(null)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNoNotes_ShouldReturnEmptyList()
	{
		// Arrange
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(new List<Note>())));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ShouldExcludeCurrentNote()
	{
		// Arrange
		var currentNoteId = ObjectId.GenerateNewId();
		var embedding = new[] { 1.0f, 0.0f, 0.0f };

		var notes = new List<Note>
		{
			new() { Id = currentNoteId, OwnerSubject = "user@example.com", Embedding = new[] { 1.0f, 0.0f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.9f, 0.1f, 0.0f } }
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com", currentNoteId);

		// Assert
		result.Should().HaveCount(1);
		result.Should().NotContain(currentNoteId);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ShouldFilterBySimilarityThreshold()
	{
		// Arrange
		var embedding = new[] { 1.0f, 0.0f, 0.0f };

		var notes = new List<Note>
		{
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.9f, 0.1f, 0.0f } }, // High similarity
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.5f, 0.5f, 0.0f } }, // Medium similarity
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.0f, 1.0f, 0.0f } }  // Low similarity (orthogonal)
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().HaveCount(2); // Only notes above 0.7 threshold (first two notes)
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ShouldLimitToTopN()
	{
		// Arrange
		var embedding = new[] { 1.0f, 0.0f, 0.0f };

		var notes = new List<Note>
		{
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.99f, 0.01f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.98f, 0.02f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.97f, 0.03f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.96f, 0.04f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.95f, 0.05f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.94f, 0.06f, 0.0f } }
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com", topN: 3);

		// Assert
		result.Should().HaveCount(3);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNegativeTopN_ShouldUseConfiguredValue()
	{
		// Arrange
		var embedding = new[] { 1.0f, 0.0f, 0.0f };

		var notes = new List<Note>
		{
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.99f, 0.01f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.98f, 0.02f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.97f, 0.03f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.96f, 0.04f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.95f, 0.05f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.94f, 0.06f, 0.0f } }
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act (configured RelatedNotesCount is 5)
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com", topN: -1);

		// Assert
		result.Should().HaveCount(5);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ShouldOrderByDescendingSimilarity()
	{
		// Arrange
		var embedding = new[] { 1.0f, 0.0f, 0.0f };
		var note1Id = ObjectId.GenerateNewId();
		var note2Id = ObjectId.GenerateNewId();
		var note3Id = ObjectId.GenerateNewId();

		var notes = new List<Note>
		{
			new() { Id = note2Id, OwnerSubject = "user@example.com", Embedding = new[] { 0.99f, 0.01f, 0.0f } }, // Highest
			new() { Id = note1Id, OwnerSubject = "user@example.com", Embedding = new[] { 0.95f, 0.05f, 0.0f } }, // Lowest
			new() { Id = note3Id, OwnerSubject = "user@example.com", Embedding = new[] { 0.97f, 0.03f, 0.0f } }  // Middle
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().HaveCount(3);
		result[0].Should().Be(note2Id); // Most similar
		result[1].Should().Be(note3Id); // Second most similar
		result[2].Should().Be(note1Id); // Least similar
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithAllNotesBelowThreshold_ShouldReturnEmptyList()
	{
		// Arrange
		var embedding = new[] { 1.0f, 0.0f, 0.0f };

		// These vectors have very low similarity with [1, 0, 0]
		// [0.3, 0.9, 0] has similarity ≈ 0.316 < 0.7
		// [0, 1, 0] has similarity = 0 < 0.7
		var notes = new List<Note>
		{
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.3f, 0.9f, 0.0f } },
			new() { Id = ObjectId.GenerateNewId(), OwnerSubject = "user@example.com", Embedding = new[] { 0.0f, 1.0f, 0.0f } }
		};

		_repository.GetNotes(Arg.Any<Expression<Func<Note, bool>>>())
			.Returns(Task.FromResult(Result.Ok<IEnumerable<Note>?>(notes)));

		// Act
		var result = await _service.FindRelatedNotesAsync(embedding, "user@example.com");

		// Assert
		result.Should().BeEmpty();
	}

	#endregion
}
