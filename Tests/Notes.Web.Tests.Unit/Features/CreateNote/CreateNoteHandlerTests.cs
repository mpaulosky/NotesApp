using System.Diagnostics.CodeAnalysis;

using Notes.Web.Tests.Unit.Infrastructure;

namespace Notes.Web.Tests.Unit.Features.CreateNote;

[ExcludeFromCodeCoverage]
public class CreateNoteHandlerTests
{
	private readonly INoteRepository _repository;
	private readonly IAiService _aiService;
	private readonly CreateNoteHandler _handler;

	public CreateNoteHandlerTests()
	{
		_repository = Substitute.For<INoteRepository>();
		_aiService = Substitute.For<IAiService>();
		_handler = new CreateNoteHandler(_repository, _aiService);
	}

	[Fact]
	public async Task Handle_ShouldCreateNoteWithAiGeneratedData()
	{
		// Arrange
		var command = TestDataBuilder.CreateNoteCommand(
			title: "Test Title",
			content: "Test Content",
			userSubject: "auth0|test123"
		);

		var expectedSummary = "AI Generated Summary";
		var expectedTags = "ai,generated,tags";
		var expectedEmbedding = new[] { 0.1f, 0.2f, 0.3f };

		_aiService.GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedSummary));
		_aiService.GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedTags));
		_aiService.GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(expectedEmbedding));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
		response.Title.Should().Be(command.Title);
		response.Content.Should().Be(command.Content);
		response.AiSummary.Should().Be(expectedSummary);
		response.Tags.Should().Be(expectedTags);
		response.Embedding.Should().BeEquivalentTo(expectedEmbedding);
		response.OwnerSubject.Should().Be(command.UserSubject);
		response.IsArchived.Should().BeFalse();
		response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
		response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Handle_ShouldCallAiServiceInParallel()
	{
		// Arrange
		var command = TestDataBuilder.CreateNoteCommand();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldSaveNoteToRepository()
	{
		// Arrange
		var command = TestDataBuilder.CreateNoteCommand();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).AddNote(Arg.Is<Note>(n =>
			n.Title == command.Title &&
			n.Content == command.Content &&
			n.OwnerSubject == command.UserSubject &&
			!n.IsArchived
		));
	}

	[Fact]
	public async Task Handle_WithCancellationToken_ShouldPassToAiService()
	{
		// Arrange
		var command = TestDataBuilder.CreateNoteCommand();
		var cts = new CancellationTokenSource();
		var token = cts.Token;

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), token)
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), token)
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), token)
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		await _handler.Handle(command, token);

		// Assert
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, token);
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, token);
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, token);
	}

	[Fact]
	public async Task Handle_ShouldGenerateNewObjectId()
	{
		// Arrange
		var command = TestDataBuilder.CreateNoteCommand();

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("Summary"));
		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult("tags"));
		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new[] { 0.1f }));

		// Act
		var response = await _handler.Handle(command, CancellationToken.None);

		// Assert
		response.Id.Should().NotBe(ObjectId.Empty);
	}
}
