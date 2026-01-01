// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notes.Web.Tests.Unit.Extensions;

/// <summary>
/// Unit tests for MongoDbExtensions validating service registration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbExtensionsTests
{
 [Fact]
 public void AddMongoDb_RegistersMongoDbContext()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var context = serviceProvider.GetService<IMongoDbContext>();
  context.Should().NotBeNull("MongoDbContext should be registered");
 }

 [Fact]
 public void AddMongoDb_RegistersNoteRepository()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var serviceProvider = builder.Services.BuildServiceProvider();
  var repository = serviceProvider.GetService<INoteRepository>();
  repository.Should().NotBeNull("INoteRepository should be registered");
 }

 [Fact]
 public void AddMongoDb_ReturnsBuilderForChaining()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  var result = builder.AddMongoDb();

  // Assert
  result.Should().BeSameAs(builder, "Extension method should return builder for method chaining");
 }

 [Fact]
 public void AddMongoDb_RegistersServicesAsScoped()
 {
  // Arrange
  var builder = new HostApplicationBuilder();

  // Act
  builder.AddMongoDb();

  // Assert
  var contextDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IMongoDbContext));
  contextDescriptor.Should().NotBeNull();
  contextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

  var repositoryDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(INoteRepository));
  repositoryDescriptor.Should().NotBeNull();
  repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
 }
}
