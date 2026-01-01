// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EmbeddingClientWrapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Notes.Web.Services.Ai;
using OpenAI.Embeddings;

namespace Notes.Web.Tests.Unit.Services.Ai;

/// <summary>
/// Unit tests for EmbeddingClientWrapper validating OpenAI embedding client integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class EmbeddingClientWrapperTests
{
 [Fact]
 public void Constructor_WithNullEmbeddingClient_ThrowsArgumentNullException()
 {
  // Arrange & Act
  var act = () => new EmbeddingClientWrapper(null!);

  // Assert
  act.Should().Throw<ArgumentNullException>()
   .WithParameterName("embeddingClient");
 }

 [Fact]
 public void Constructor_WithValidEmbeddingClient_CreatesInstance()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();

  // Act
  var wrapper = new EmbeddingClientWrapper(embeddingClient);

  // Assert
  wrapper.Should().NotBeNull();
  wrapper.Should().BeAssignableTo<IEmbeddingClientWrapper>();
 }

 [Fact]
 public async Task GenerateEmbeddingAsync_WithValidInput_CallsEmbeddingClient()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();
  var wrapper = new EmbeddingClientWrapper(embeddingClient);
  const string input = "Test input for embedding";

  // Act
  await wrapper.GenerateEmbeddingAsync(input);

  // Assert
  await embeddingClient.Received(1).GenerateEmbeddingAsync(
   input,
   cancellationToken: Arg.Any<CancellationToken>());
 }

 [Fact]
 public async Task GenerateEmbeddingAsync_PassesCancellationToken()
 {
  // Arrange
  var embeddingClient = Substitute.For<EmbeddingClient>();
  var wrapper = new EmbeddingClientWrapper(embeddingClient);
  using var cts = new CancellationTokenSource();

  // Act
  try
  {
   await wrapper.GenerateEmbeddingAsync("Test", cts.Token);
  }
  catch
  {
   // Ignore exceptions from mocked client
  }

  // Assert
  await embeddingClient.Received(1).GenerateEmbeddingAsync(
   Arg.Any<string>(),
   cancellationToken: cts.Token);
 }
}
