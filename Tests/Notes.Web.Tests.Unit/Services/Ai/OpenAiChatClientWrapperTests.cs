using System.ClientModel;
using System.Diagnostics.CodeAnalysis;

using OpenAI.Chat;

namespace Notes.Web.Tests.Unit.Services.Ai;

/// <summary>
/// Tests for OpenAiChatClientWrapper.
/// Note: This is a thin wrapper around ChatClient for dependency injection.
/// Tests focus on constructor validation and method delegation.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenAiChatClientWrapperTests
{
	#region Constructor Tests

	[Fact]
	public void Constructor_WithValidChatClient_ShouldCreateInstance()
	{
		// Arrange
		var chatClient = Substitute.For<ChatClient>();

		// Act
		var wrapper = new OpenAiChatClientWrapper(chatClient);

		// Assert
		wrapper.Should().NotBeNull();
	}

	[Fact]
	public void Constructor_WithNullChatClient_ShouldThrowArgumentNullException()
	{
		// Act
		Action act = () => new OpenAiChatClientWrapper(null!);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("chatClient");
	}

	#endregion

	#region CompleteChatAsync Tests

	[Fact]
	public async Task CompleteChatAsync_WithValidMessages_ShouldDelegateToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new UserChatMessage("Hello, AI!")
		};

		// Act
		await wrapper.CompleteChatAsync(messages);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => m.SequenceEqual(messages)),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithOptions_ShouldPassOptionsToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new UserChatMessage("Test message")
		};
		var options = new ChatCompletionOptions
		{
			MaxOutputTokenCount = 100,
			Temperature = 0.7f
		};

		// Act
		await wrapper.CompleteChatAsync(messages, options);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Any<IEnumerable<ChatMessage>>(),
			Arg.Is<ChatCompletionOptions?>(o => o == options),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithNullOptions_ShouldPassNullToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new UserChatMessage("Test message")
		};

		// Act
		await wrapper.CompleteChatAsync(messages, null);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Any<IEnumerable<ChatMessage>>(),
			Arg.Is<ChatCompletionOptions?>(o => o == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithCancellationToken_ShouldPassTokenToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new UserChatMessage("Test message")
		};
		var cancellationToken = new CancellationToken();

		// Act
		await wrapper.CompleteChatAsync(messages, null, cancellationToken);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Any<IEnumerable<ChatMessage>>(),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Is<CancellationToken>(ct => ct == cancellationToken));
	}

	[Fact]
	public async Task CompleteChatAsync_WithEmptyMessagesList_ShouldDelegateToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>();

		// Act
		await wrapper.CompleteChatAsync(messages);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => !m.Any()),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithMultipleMessages_ShouldDelegateToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new SystemChatMessage("You are a helpful assistant."),
			new UserChatMessage("What is AI?"),
			new AssistantChatMessage("AI stands for Artificial Intelligence."),
			new UserChatMessage("Tell me more.")
		};

		// Act
		await wrapper.CompleteChatAsync(messages);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => m.Count() == 4),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithAllParameters_ShouldPassAllParametersCorrectly()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new SystemChatMessage("System prompt"),
			new UserChatMessage("User query")
		};
		var options = new ChatCompletionOptions
		{
			MaxOutputTokenCount = 200,
			Temperature = 0.5f,
			TopP = 0.9f
		};
		var cancellationToken = new CancellationToken();

		// Act
		await wrapper.CompleteChatAsync(messages, options, cancellationToken);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => m.SequenceEqual(messages)),
			Arg.Is<ChatCompletionOptions?>(o => o == options),
			Arg.Is<CancellationToken>(ct => ct == cancellationToken));
	}

	[Fact]
	public async Task CompleteChatAsync_WithSingleSystemMessage_ShouldDelegateToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new SystemChatMessage("You are a helpful assistant.")
		};

		// Act
		await wrapper.CompleteChatAsync(messages);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => m.Count() == 1),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithSingleAssistantMessage_ShouldDelegateToUnderlyingClient()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new AssistantChatMessage("This is a response.")
		};

		// Act
		await wrapper.CompleteChatAsync(messages);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Is<IEnumerable<ChatMessage>>(m => m.Count() == 1),
			Arg.Any<ChatCompletionOptions?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteChatAsync_WithComplexOptions_ShouldPassOptionsCorrectly()
	{
		// Arrange
		var mockChatClient = Substitute.For<ChatClient>();
		var wrapper = new OpenAiChatClientWrapper(mockChatClient);
		var messages = new List<ChatMessage>
		{
			new UserChatMessage("Test")
		};
		var options = new ChatCompletionOptions
		{
			MaxOutputTokenCount = 500,
			Temperature = 0.3f,
			TopP = 0.95f,
			FrequencyPenalty = 0.5f,
			PresencePenalty = 0.5f
		};

		// Act
		await wrapper.CompleteChatAsync(messages, options);

		// Assert
		await mockChatClient.Received(1).CompleteChatAsync(
			Arg.Any<IEnumerable<ChatMessage>>(),
			Arg.Is<ChatCompletionOptions?>(o =>
				o != null &&
				o.MaxOutputTokenCount == 500 &&
				o.Temperature == 0.3f &&
				o.TopP == 0.95f &&
				o.FrequencyPenalty == 0.5f &&
				o.PresencePenalty == 0.5f),
			Arg.Any<CancellationToken>());
	}

	#endregion
}

