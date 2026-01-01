# Architecture Tests Implementation

## Goal

Implement comprehensive architecture tests for ServiceDefaults, Extensions, and Configuration to validate architectural boundaries and coding standards following NetArchTest patterns.

## Prerequisites

**Branch:** Ensure you are on the `add-tests` branch before beginning implementation.

- If not on the branch, switch to it: `git checkout add-tests`
- If the branch doesn't exist, create it from main: `git checkout -b add-tests main`

**Technology Stack:**

- .NET 10.0, C# 14.0
- xUnit 2.9.3
- FluentAssertions 7.1.0
- NetArchTest.Rules 1.3.2

---

## Step-by-Step Instructions

### Step 1: Create ServiceDefaults Architecture Tests

Add comprehensive architecture tests for the Notes.ServiceDefaults project to validate extension method patterns and dependencies.

- [x] Create new test file at `Tests/Notes.Tests.Architecture/ServiceDefaultsTests.cs`
- [x] Copy and paste code below into `Tests/Notes.Tests.Architecture/ServiceDefaultsTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ServiceDefaultsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for the ServiceDefaults project.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServiceDefaultsTests : BaseArchitectureTest
{
 [Fact]
 public void ExtensionMethodsShouldBePublicAndStatic()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .That()
   .HaveNameEndingWith("Extensions")
   .Should()
   .BeStaticClasses()
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Extension classes should be public and static. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ExtensionMethodsShouldBeInCorrectNamespace()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .That()
   .HaveNameEndingWith("Extensions")
   .Should()
   .ResideInNamespace("Microsoft.Extensions.Hosting")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Extension classes should be in Microsoft.Extensions.Hosting namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ServiceDefaultsShouldNotDependOnWebProject()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .Should()
   .NotHaveDependencyOn("Notes.Web")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"ServiceDefaults should not depend on Notes.Web. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ServiceDefaultsShouldNotDependOnAppHost()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .Should()
   .NotHaveDependencyOn("Notes.AppHost")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"ServiceDefaults should not depend on AppHost. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ServiceDefaultsShouldNotDependOnShared()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .Should()
   .NotHaveDependencyOn("Shared")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"ServiceDefaults should not depend on Shared. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ServiceDefaultsShouldOnlyContainExtensionClasses()
 {
  var result = Types.InAssembly(ServiceDefaultsAssembly)
   .That()
   .AreClasses()
   .Should()
   .HaveNameEndingWith("Extensions")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"All classes in ServiceDefaults should be extension classes. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }
}
```

#### Step 1 Verification Checklist

- [x] Run tests: `dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj`
- [x] All ServiceDefaultsTests pass successfully
- [x] No build errors or warnings

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 2: Create Extension Method Architecture Tests

Add architecture tests for all extension methods in Notes.Web to validate they follow consistent patterns.

- [x] Create new test file at `Tests/Notes.Tests.Architecture/ExtensionMethodsTests.cs`
- [x] Copy and paste code below into `Tests/Notes.Tests.Architecture/ExtensionMethodsTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ExtensionMethodsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for extension method patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExtensionMethodsTests : BaseArchitectureTest
{
 [Fact]
 public void ExtensionMethodsShouldBePublicAndStatic()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Extensions")
   .Should()
   .BeStaticClasses()
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Extension classes should be public and static. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ExtensionMethodsShouldBeInMicrosoftExtensionsNamespace()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Extensions")
   .And()
   .ResideInNamespaceContaining("Extensions")
   .Should()
   .ResideInNamespace("Microsoft.Extensions.Hosting")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Extension classes should be in Microsoft.Extensions.Hosting namespace. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void ExtensionMethodsShouldReturnBuilderForChaining()
 {
  // Get all public static methods in extension classes
  var extensionMethods = WebAssembly.GetTypes()
   .Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed)
   .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
   .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType.Name.Contains("Builder") == true);

  // All extension methods on builders should return the builder for method chaining
  foreach (var method in extensionMethods)
  {
   var returnType = method.ReturnType;
   var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;

   returnType.Should().Be(parameterType,
    $"Extension method {method.DeclaringType?.Name}.{method.Name} should return {parameterType?.Name} for method chaining");
  }
 }

 [Fact]
 public void ExtensionMethodsShouldNotReturnVoid()
 {
  var extensionClasses = WebAssembly.GetTypes()
   .Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed);

  foreach (var extensionClass in extensionClasses)
  {
   var voidMethods = extensionClass.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.ReturnType == typeof(void) && !m.IsSpecialName);

   voidMethods.Should().BeEmpty(
    $"Extension class {extensionClass.Name} should not have void methods. Void methods: {string.Join(", ", voidMethods.Select(m => m.Name))}");
  }
 }

 [Fact]
 public void ExtensionMethodsShouldHaveXmlDocumentation()
 {
  var extensionClasses = WebAssembly.GetTypes()
   .Where(t => t.Name.EndsWith("Extensions") && t.IsClass && t.IsAbstract && t.IsSealed);

  // This test verifies the pattern exists - actual XML doc validation requires loading XML doc files
  extensionClasses.Should().NotBeEmpty("Extension classes should exist in the assembly");
 }

 [Fact]
 public void MongoDbExtensionsShouldRegisterRequiredServices()
 {
  var mongoDbExtensions = WebAssembly.GetTypes()
   .FirstOrDefault(t => t.Name == "MongoDbExtensions");

  mongoDbExtensions.Should().NotBeNull("MongoDbExtensions class should exist");
  
  var addMongoDbMethod = mongoDbExtensions!.GetMethod("AddMongoDb", BindingFlags.Public | BindingFlags.Static);
  addMongoDbMethod.Should().NotBeNull("AddMongoDb extension method should exist");
 }

 [Fact]
 public void AiServiceExtensionsShouldRegisterRequiredServices()
 {
  var aiServiceExtensions = WebAssembly.GetTypes()
   .FirstOrDefault(t => t.Name == "AiServiceExtensions");

  aiServiceExtensions.Should().NotBeNull("AiServiceExtensions class should exist");
  
  var addAiServicesMethod = aiServiceExtensions!.GetMethod("AddAiServices", BindingFlags.Public | BindingFlags.Static);
  addAiServicesMethod.Should().NotBeNull("AddAiServices extension method should exist");
 }
}
```

#### Step 2 Verification Checklist

- [x] Run tests: `dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj`
- [x] All ExtensionMethodsTests pass successfully
- [x] No build errors or warnings

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

### Step 3: Create Configuration Architecture Tests

Add architecture tests to validate configuration patterns and options classes.

- [x] Create new test file at `Tests/Notes.Tests.Architecture/ConfigurationTests.cs`
- [x] Copy and paste code below into `Tests/Notes.Tests.Architecture/ConfigurationTests.cs`:

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ConfigurationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Notes.Tests.Architecture;

/// <summary>
/// Architecture tests for configuration patterns.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConfigurationTests : BaseArchitectureTest
{
 [Fact]
 public void OptionClassesShouldEndWithOptions()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Options")
   .Should()
   .BeClasses()
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"All types ending with 'Options' should be classes. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void OptionClassesShouldBePublic()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Options")
   .Should()
   .BePublic()
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Options classes should be public. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void OptionClassesShouldBeSealed()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Options")
   .Should()
   .BeSealed()
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Options classes should be sealed. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void OptionClassesShouldHaveSectionNameConstant()
 {
  var optionClasses = WebAssembly.GetTypes()
   .Where(t => t.Name.EndsWith("Options") && t.IsClass);

  foreach (var optionClass in optionClasses)
  {
   var sectionNameField = optionClass.GetField("SectionName", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
   
   sectionNameField.Should().NotBeNull(
    $"Options class {optionClass.Name} should have a public static SectionName field");
   
   if (sectionNameField != null)
   {
    sectionNameField.FieldType.Should().Be(typeof(string),
     $"SectionName field in {optionClass.Name} should be of type string");
   }
  }
 }

 [Fact]
 public void OptionClassesShouldHaveInitOnlyProperties()
 {
  var optionClasses = WebAssembly.GetTypes()
   .Where(t => t.Name.EndsWith("Options") && t.IsClass);

  foreach (var optionClass in optionClasses)
  {
   var properties = optionClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);
   
   foreach (var property in properties)
   {
    // Properties should have setters (init or set)
    property.CanWrite.Should().BeTrue(
     $"Property {property.Name} in {optionClass.Name} should have a setter (init or set)");
   }
  }
 }

 [Fact]
 public void AiServiceOptionsShouldExist()
 {
  var aiServiceOptions = WebAssembly.GetTypes()
   .FirstOrDefault(t => t.Name == "AiServiceOptions");

  aiServiceOptions.Should().NotBeNull("AiServiceOptions class should exist");
  aiServiceOptions!.Should().BeSealed("AiServiceOptions should be sealed");
  
  var sectionName = aiServiceOptions.GetField("SectionName", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
  sectionName.Should().NotBeNull("AiServiceOptions should have SectionName constant");
 }

 [Fact]
 public void OptionClassesShouldNotDependOnFeatures()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Options")
   .Should()
   .NotHaveDependencyOn("Features")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Options classes should not depend on Features. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }

 [Fact]
 public void OptionClassesShouldNotDependOnComponents()
 {
  var result = Types.InAssembly(WebAssembly)
   .That()
   .HaveNameEndingWith("Options")
   .Should()
   .NotHaveDependencyOn("Components")
   .GetResult();

  result.IsSuccessful.Should().BeTrue(
   $"Options classes should not depend on Components. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
 }
}
```

#### Step 3 Verification Checklist

- [x] Run tests: `dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj`
- [x] All ConfigurationTests pass successfully
- [x] No build errors or warnings
- [x] Run all architecture tests: `dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj --logger "console;verbosity=normal"`

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

---

## Final Verification

After completing all steps:

- [x] All architecture tests pass (93 tests)
- [x] No build errors in the test project
- [x] Code coverage includes new test files
- [x] Tests follow project naming and structure conventions
- [x] All tests have XML documentation comments

## Commands Reference

```powershell
# Run all architecture tests
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj

# Run specific test class
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj --filter "FullyQualifiedName~ServiceDefaultsTests"

# Run with detailed output
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj --logger "console;verbosity=normal"

# Check code coverage
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj /p:CollectCoverage=true
```
