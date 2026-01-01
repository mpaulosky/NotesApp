# Blazor Component Tests Implementation

## Goal

Implement comprehensive bUnit tests for CreateNote and EditNote Blazor page components to validate UI rendering, form submission, navigation, and error handling.

## Prerequisites

**Branch:** Ensure you are on the `add-tests` branch before beginning implementation.

- If not on the branch, switch to it: `git checkout add-tests`
- Complete Architecture Tests (1-architecture-tests.md) and Unit Tests (2-unit-tests.md) before starting these component tests

**Technology Stack:**

- .NET 10.0, C# 14.0
- xUnit 2.9.3
- FluentAssertions 7.1.0
- bUnit 2.4.2
- NSubstitute 5.3.0

---

## Step-by-Step Instructions

### Step 1: Create CreateNote Component Tests

Add comprehensive bUnit tests for the CreateNote page component.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Components/Pages/CreateNoteTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Components/Pages/CreateNoteTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateNoteTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using Notes.Web.Components.Pages;
using Notes.Web.Services.Notes;

namespace Notes.Web.Tests.Unit.Components.Pages;

/// <summary>
/// bUnit tests for CreateNote Blazor page component.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateNoteTests : BunitContext
{
 private readonly INoteService _noteService;
 private readonly FakeNavigationManager _navigationManager;

 public CreateNoteTests()
 {
  _noteService = Substitute.For<INoteService>();
  _navigationManager = Services.AddFakeNavigationManager();

  Services.AddSingleton(_noteService);

  // Set up authentication
  var authContext = this.AddTestAuthorization();
  authContext.SetAuthorized("test-user");
  authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, "user-123"));
 }

 [Fact]
 public void CreateNote_ShouldRender()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  cut.Should().NotBeNull();
 }

 [Fact]
 public void CreateNote_ShouldDisplayPageHeading()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  cut.FindComponent<PageHeadingComponent>().Should().NotBeNull();
  cut.Markup.Should().Contain("Create Note");
 }

 [Fact]
 public void CreateNote_ShouldRenderTitleInput()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  var titleInput = cut.Find("#title");
  titleInput.Should().NotBeNull();
  titleInput.GetAttribute("type").Should().Be("text");
 }

 [Fact]
 public void CreateNote_ShouldRenderContentTextArea()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  var contentTextArea = cut.Find("#content");
  contentTextArea.Should().NotBeNull();
  contentTextArea.TagName.Should().Be("TEXTAREA");
  contentTextArea.GetAttribute("rows").Should().Be("10");
 }

 [Fact]
 public void CreateNote_ShouldRenderCreateButton()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  var createButton = cut.Find("button[type='submit']");
  createButton.Should().NotBeNull();
  createButton.TextContent.Should().Contain("Create");
 }

 [Fact]
 public void CreateNote_ShouldRenderCancelButton()
 {
  // Arrange & Act
  var cut = Render<CreateNote>();

  // Assert
  var cancelButton = cut.Find("button[type='button']");
  cancelButton.Should().NotBeNull();
  cancelButton.TextContent.Should().Contain("Cancel");
 }

 [Fact]
 public async Task CreateNote_OnValidSubmit_CallsNoteService()
 {
  // Arrange
  var expectedResponse = new CreateNoteResponse
  {
   Success = true,
   NoteId = ObjectId.GenerateNewId()
  };

  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(expectedResponse);

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content for the note");
  await form.SubmitAsync();

  // Assert
  await _noteService.Received(1).CreateNoteAsync(
   "Test Note",
   "Test content for the note",
   "user-123",
   Arg.Any<CancellationToken>()
  );
 }

 [Fact]
 public async Task CreateNote_OnSuccessfulSubmit_NavigatesToNotesList()
 {
  // Arrange
  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(new CreateNoteResponse { Success = true, NoteId = ObjectId.GenerateNewId() });

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content");
  await form.SubmitAsync();

  // Assert
  _navigationManager.Uri.Should().EndWith("/notes");
 }

 [Fact]
 public async Task CreateNote_OnFailedSubmit_DisplaysErrorMessage()
 {
  // Arrange
  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns((CreateNoteResponse?)null);

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content");
  await form.SubmitAsync();

  // Assert
  cut.WaitForAssertion(() =>
  {
   var errorAlert = cut.FindComponent<ErrorAlertComponent>();
   errorAlert.Should().NotBeNull();
   cut.Markup.Should().Contain("Failed to create note");
  });
 }

 [Fact]
 public async Task CreateNote_OnException_DisplaysErrorMessage()
 {
  // Arrange
  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).ThrowsAsync(new Exception("Database connection failed"));

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content");
  await form.SubmitAsync();

  // Assert
  cut.WaitForAssertion(() =>
  {
   cut.Markup.Should().Contain("Error creating note");
   cut.Markup.Should().Contain("Database connection failed");
  });
 }

 [Fact]
 public async Task CreateNote_WhileSaving_DisablesSubmitButton()
 {
  // Arrange
  var tcs = new TaskCompletionSource<CreateNoteResponse>();
  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(tcs.Task);

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content");
  
  var submitTask = form.SubmitAsync();

  // Assert
  cut.WaitForAssertion(() =>
  {
   var submitButton = cut.Find("button[type='submit']");
   submitButton.HasAttribute("disabled").Should().BeTrue();
   submitButton.TextContent.Should().Contain("Creating...");
  });

  // Cleanup
  tcs.SetResult(new CreateNoteResponse { Success = true, NoteId = ObjectId.GenerateNewId() });
  await submitTask;
 }

 [Fact]
 public void CreateNote_OnCancelClick_NavigatesToNotesList()
 {
  // Arrange
  var cut = Render<CreateNote>();

  // Act
  var cancelButton = cut.Find("button[type='button']");
  cancelButton.Click();

  // Assert
  _navigationManager.Uri.Should().EndWith("/notes");
 }

 [Fact]
 public void CreateNote_RequiresAuthentication()
 {
  // Arrange
  var authContext = this.AddTestAuthorization();
  authContext.SetNotAuthorized();

  // Act & Assert
  var act = () => Render<CreateNote>();
  
  act.Should().Throw<UnauthorizedAccessException>();
 }

 [Fact]
 public async Task CreateNote_ExtractsUserSubjectFromClaims()
 {
  // Arrange
  _noteService.CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(new CreateNoteResponse { Success = true, NoteId = ObjectId.GenerateNewId() });

  var cut = Render<CreateNote>();

  // Act
  var titleInput = cut.Find("#title");
  var contentTextArea = cut.Find("#content");
  var form = cut.Find("form");

  titleInput.Change("Test Note");
  contentTextArea.Change("Test content");
  await form.SubmitAsync();

  // Assert
  await _noteService.Received(1).CreateNoteAsync(
   Arg.Any<string>(),
   Arg.Any<string>(),
   "user-123", // The user subject from claims
   Arg.Any<CancellationToken>()
  );
 }
}
```

#### Step 1 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~CreateNoteTests"`
- [ ] All CreateNoteTests pass successfully
- [ ] No build errors or warnings

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 2: Create EditNote Component Tests

Add comprehensive bUnit tests for the EditNote page component.

- [ ] Create new test file at `Tests/Notes.Web.Tests.Unit/Components/Pages/EditNoteTests.cs`
- [ ] Copy and paste code below into `Tests/Notes.Web.Tests.Unit/Components/Pages/EditNoteTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditNoteTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Web.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Notes.Web.Components.Pages;
using Notes.Web.Services.Notes;

namespace Notes.Web.Tests.Unit.Components.Pages;

/// <summary>
/// bUnit tests for EditNote Blazor page component.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditNoteTests : BunitContext
{
 private readonly INoteService _noteService;
 private readonly FakeNavigationManager _navigationManager;
 private readonly ObjectId _testNoteId;

 public EditNoteTests()
 {
  _noteService = Substitute.For<INoteService>();
  _navigationManager = Services.AddFakeNavigationManager();
  _testNoteId = ObjectId.GenerateNewId();

  Services.AddSingleton(_noteService);

  // Set up authentication
  var authContext = this.AddTestAuthorization();
  authContext.SetAuthorized("test-user");
  authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, "user-123"));
 }

 [Fact]
 public async Task EditNote_ShouldLoadNoteOnInitialization()
 {
  // Arrange
  var expectedNote = new GetNoteDetailsResponse
  {
   Success = true,
   Note = new NoteDetailsDto
   {
    Id = _testNoteId,
    Title = "Existing Note",
    Content = "Existing content",
    IsArchived = false
   }
  };

  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(expectedNote);

  // Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  // Assert
  await _noteService.Received(1).GetNoteDetailsAsync(
   _testNoteId,
   "user-123",
   Arg.Any<CancellationToken>()
  );
 }

 [Fact]
 public async Task EditNote_ShouldDisplayLoadingComponentWhileLoading()
 {
  // Arrange
  var tcs = new TaskCompletionSource<GetNoteDetailsResponse>();
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(tcs.Task);

  // Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  // Assert
  var loadingComponent = cut.FindComponent<LoadingComponent>();
  loadingComponent.Should().NotBeNull();

  // Cleanup
  tcs.SetResult(new GetNoteDetailsResponse
  {
   Success = true,
   Note = new NoteDetailsDto
   {
    Id = _testNoteId,
    Title = "Test",
    Content = "Test"
   }
  });
 }

 [Fact]
 public async Task EditNote_WithValidNoteId_DisplaysNoteData()
 {
  // Arrange
  var expectedNote = new GetNoteDetailsResponse
  {
   Success = true,
   Note = new NoteDetailsDto
   {
    Id = _testNoteId,
    Title = "Test Note",
    Content = "Test content",
    IsArchived = false
   }
  };

  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(expectedNote);

  // Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100); // Wait for async initialization

  // Assert
  cut.WaitForAssertion(() =>
  {
   var titleInput = cut.Find("#title");
   var contentTextArea = cut.Find("#content");

   titleInput.GetAttribute("value").Should().Be("Test Note");
   contentTextArea.TextContent.Should().Be("Test content");
  });
 }

 [Fact]
 public async Task EditNote_WithInvalidNoteId_DisplaysErrorMessage()
 {
  // Arrange & Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, "invalid-id"));

  await Task.Delay(100); // Wait for async initialization

  // Assert
  cut.WaitForAssertion(() =>
  {
   var errorAlert = cut.FindComponent<ErrorAlertComponent>();
   errorAlert.Should().NotBeNull();
   cut.Markup.Should().Contain("Invalid note ID");
  });
 }

 [Fact]
 public async Task EditNote_WhenNoteNotFound_DisplaysErrorMessage()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = false,
    Message = "Note not found"
   });

  // Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Assert
  cut.WaitForAssertion(() =>
  {
   var errorAlert = cut.FindComponent<ErrorAlertComponent>();
   errorAlert.Should().NotBeNull();
   cut.Markup.Should().Contain("Note not found");
  });
 }

 [Fact]
 public async Task EditNote_ShouldRenderArchiveCheckbox()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = true,
    Note = new NoteDetailsDto
    {
     Id = _testNoteId,
     Title = "Test",
     Content = "Test",
     IsArchived = false
    }
   });

  // Act
  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Assert
  cut.WaitForAssertion(() =>
  {
   var archiveCheckbox = cut.Find("input[type='checkbox']");
   archiveCheckbox.Should().NotBeNull();
  });
 }

 [Fact]
 public async Task EditNote_OnValidSubmit_CallsUpdateNoteService()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = true,
    Note = new NoteDetailsDto
    {
     Id = _testNoteId,
     Title = "Original Title",
     Content = "Original content",
     IsArchived = false
    }
   });

  _noteService.UpdateNoteAsync(
   Arg.Any<ObjectId>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<bool>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(new UpdateNoteResponse { Success = true });

  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Act
  cut.WaitForAssertion(() =>
  {
   var titleInput = cut.Find("#title");
   var contentTextArea = cut.Find("#content");
   var form = cut.Find("form");

   titleInput.Change("Updated Title");
   contentTextArea.Change("Updated content");
   form.Submit();
  });

  await Task.Delay(100);

  // Assert
  await _noteService.Received(1).UpdateNoteAsync(
   _testNoteId,
   "Updated Title",
   "Updated content",
   Arg.Any<bool>(),
   "user-123",
   Arg.Any<CancellationToken>()
  );
 }

 [Fact]
 public async Task EditNote_OnSuccessfulUpdate_NavigatesToNotesList()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = true,
    Note = new NoteDetailsDto
    {
     Id = _testNoteId,
     Title = "Test",
     Content = "Test",
     IsArchived = false
    }
   });

  _noteService.UpdateNoteAsync(
   Arg.Any<ObjectId>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<bool>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(new UpdateNoteResponse { Success = true });

  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Act
  cut.WaitForAssertion(() =>
  {
   var form = cut.Find("form");
   form.Submit();
  });

  await Task.Delay(200);

  // Assert
  _navigationManager.Uri.Should().EndWith("/notes");
 }

 [Fact]
 public async Task EditNote_OnFailedUpdate_DisplaysErrorMessage()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = true,
    Note = new NoteDetailsDto
    {
     Id = _testNoteId,
     Title = "Test",
     Content = "Test",
     IsArchived = false
    }
   });

  _noteService.UpdateNoteAsync(
   Arg.Any<ObjectId>(),
   Arg.Any<string>(),
   Arg.Any<string>(),
   Arg.Any<bool>(),
   Arg.Any<string>(),
   Arg.Any<CancellationToken>()
  ).Returns(new UpdateNoteResponse
  {
   Success = false,
   Message = "Update failed"
  });

  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Act
  cut.WaitForAssertion(() =>
  {
   var form = cut.Find("form");
   form.Submit();
  });

  await Task.Delay(200);

  // Assert
  cut.WaitForAssertion(() =>
  {
   cut.Markup.Should().Contain("Update failed");
  });
 }

 [Fact]
 public async Task EditNote_OnCancelClick_NavigatesToNotesList()
 {
  // Arrange
  _noteService.GetNoteDetailsAsync(Arg.Any<ObjectId>(), Arg.Any<string>(), Arg.any<CancellationToken>())
   .Returns(new GetNoteDetailsResponse
   {
    Success = true,
    Note = new NoteDetailsDto
    {
     Id = _testNoteId,
     Title = "Test",
     Content = "Test",
     IsArchived = false
    }
   });

  var cut = Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));

  await Task.Delay(100);

  // Act
  cut.WaitForAssertion(() =>
  {
   var cancelButton = cut.Find("button[type='button']");
   cancelButton.Click();
  });

  // Assert
  _navigationManager.Uri.Should().EndWith("/notes");
 }

 [Fact]
 public void EditNote_RequiresAuthentication()
 {
  // Arrange
  var authContext = this.AddTestAuthorization();
  authContext.SetNotAuthorized();

  // Act & Assert
  var act = () => Render<EditNote>(parameters => parameters
   .Add(p => p.Id, _testNoteId.ToString()));
  
  act.Should().Throw<UnauthorizedAccessException>();
 }
}
```

#### Step 2 Verification Checklist

- [ ] Run tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~EditNoteTests"`
- [ ] All EditNoteTests pass successfully
- [ ] No build errors or warnings
- [ ] Run all component tests: `dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~Components.Pages"`

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

## Final Verification

After completing all steps:

- [ ] All component tests pass
- [ ] No build errors in the test project
- [ ] Code coverage includes new test files
- [ ] Tests follow project naming and structure conventions
- [ ] All tests have XML documentation comments

## Commands Reference

```powershell
# Run all component tests
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --filter "FullyQualifiedName~Components.Pages"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateNoteTests"

# Run with code coverage
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj /p:CollectCoverage=true

# Run with detailed output
dotnet test Tests/Notes.Web.Tests.Unit/Notes.Web.Tests.Unit.csproj --logger "console;verbosity=normal"
```
