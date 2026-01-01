// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ChatClientWrapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Notes.Web.Services.Ai;
using OpenAI.Chat;

namespace Notes.Web.Tests.Unit.Services.Ai;

/// <summary>
/// Unit tests for ChatClientWrapper validating OpenAI chat client integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChatClientWrapperTests
{
 [Fact]
 public void Constructor_WithNullChatClient_ThrowsArgumentNullException()
 {
  // Arrange & Act
  var act = () => new ChatClientWrapper(null!);

  // Assert
  act.Should().Throw<ArgumentNullException>()
   .WithParameterName("chatClient");
 }

 [Fact]
 public void Constructor_WithValidChatClient_CreatesInstance()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();

  // Act
  var wrapper = new ChatClientWrapper(chatClient);

  // Assert
  wrapper.Should().NotBeNull();
  wrapper.Should().BeAssignableTo<IChatClientWrapper>();
 }

 [Fact]
 public async Task CompleteChatAsync_WithValidMessages_CallsChatClient()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  var messages = new List<ChatMessage>
  {
   ChatMessage.CreateUserMessage("Test message")
  };

  // Act
  await wrapper.CompleteChatAsync(messages);

  // Assert
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Is<IEnumerable<ChatMessage>>(m => m.SequenceEqual(messages)),
   Arg.Any<ChatCompletionOptions>(),
   Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task CompleteChatAsync_PassesCancellationToken()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  using var cts = new CancellationTokenSource();
  var messages = new List<ChatMessage> { ChatMessage.CreateUserMessage("Test") };

  // Act
  try
  {
   await wrapper.CompleteChatAsync(messages, null, cts.Token);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Any<IEnumerable<ChatMessage>>(),
   Arg.Any<ChatCompletionOptions>(),
   cts.Token);
 }

 [Fact]
 public async Task CompleteChatAsync_WithOptions_PassesOptionsToClient()
 {
  // Arrange
  var chatClient = Substitute.For<ChatClient>();
  var wrapper = new ChatClientWrapper(chatClient);
  var messages = new List<ChatMessage> { ChatMessage.CreateUserMessage("Test") };
  var options = new ChatCompletionOptions { Temperature = 0.7f };

  // Act
  try
  {
   await wrapper.CompleteChatAsync(messages, options);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await chatClient.Received(1).CompleteChatAsync(
   Arg.Any<IEnumerable<ChatMessage>>(),
   Arg.Is<ChatCompletionOptions>(o => o != null && o.Temperature == 0.7f),
   Arg.Any<CancellationToken>());
 }
}
