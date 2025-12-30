# Architecture Tests for NotesApp

This project contains comprehensive architecture tests using **NetArchTest.Rules** and **FluentAssertions** to enforce architectural patterns and coding standards across the NotesApp solution.

## Test Coverage (42 Tests - 100% Passing)

### Naming Convention Tests (7 tests)
- ? Interfaces should start with 'I'
- ? MediatR requests should end with 'Command' or 'Query'
- ? MediatR queries should follow naming convention
- ? MediatR handlers should end with 'Handler'
- ? Repositories should end with 'Repository'
- ? Services should end with 'Service' or 'Wrapper'
- ? Entities should not have 'Entity' suffix

### Dependency Tests (7 tests)
- ? Shared should not depend on Notes.Web
- ? Shared should not depend on ServiceDefaults
- ? Shared should not depend on AppHost
- ? Entities should not depend on Features
- ? Features should not depend on Components
- ? Components should not depend on Data
- ? MediatR handlers should reside in Features namespace
- ? Repositories should be in correct namespace
- ? Services should not depend on Features

### Layering Tests (8 tests)
- ? Features should be in correct namespace
- ? Handlers should be in Features namespace
- ? Requests should be records (C# records)
- ? Entities should be in Shared assembly
- ? Repositories should implement interfaces
- ? Services should implement interfaces
- ? Data context should be in Data namespace
- ? Abstractions should be in Shared assembly

### Blazor Component Tests (7 tests)
- ? Components should inherit from ComponentBase
- ? Components should not be sealed (allow inheritance)
- ? Components should not directly use MediatR
- ? Routable components should be in appropriate namespace
- ? Component parameters should have [Parameter] attribute
- ? Layout components should be in Layout namespace
- ? Components should be in Notes.Web.Components namespace

### General Architecture Tests (13 tests)
- ? Classes should not have public fields
- ? Async methods should have 'Async' suffix (with framework exceptions)
- ? Option classes should be in correct namespace
- ? Exceptions should end with 'Exception'
- ? Extension method classes should be static
- ? DTOs should be records
- ? Nullable context should be enabled
- ? Production code should not reference xUnit
- ? Production code should not reference NUnit
- ? Production code should not reference MSTest
- ? Methods should have correct accessibility

## Technology Stack

- **.NET 10.0** - Target framework
- **xUnit 2.9.3** - Test framework
- **NetArchTest.Rules 1.3.2** - Architecture testing library
- **FluentAssertions 7.1.0** - Fluent assertion library

## Project Structure

```
Tests/Notes.Tests.Architecture/
??? BaseArchitectureTest.cs           # Base class with assembly references
??? NamingConventionTests.cs          # Naming pattern enforcement
??? DependencyTests.cs                # Dependency rule validation
??? LayeringTests.cs                  # Layer separation rules
??? BlazorComponentTests.cs           # Blazor-specific patterns
??? GeneralArchitectureTests.cs       # General coding standards
??? Notes.Tests.Architecture.csproj   # Project configuration
```

## Architecture Rules Enforced

### Clean Architecture Principles
- **Separation of Concerns**: UI, business logic, and data layers are properly isolated
- **Dependency Inversion**: Higher layers don't depend on lower layers
- **Single Responsibility**: Each component has a well-defined purpose

### CQRS Pattern
- Commands modify state and use `Command` suffix
- Queries read state and use `Query` suffix
- All requests are immutable records
- Handlers are internal and use the `Handle` method name per MediatR convention

### Blazor Best Practices
- Components inherit from `ComponentBase`
- Components don't directly use MediatR (separation of concerns)
- Layout components are properly namespaced
- Component parameters use attributes correctly

### Code Quality
- Nullable reference types enabled
- No public fields (encapsulation)
- Async methods follow naming convention
- Test frameworks stay in test projects

## Running the Tests

### Run all architecture tests
```bash
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~NamingConventionTests"
```

### Run with detailed output
```bash
dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj --verbosity detailed
```

## CI/CD Integration

These tests should be run as part of your continuous integration pipeline:

```yaml
# Example GitHub Actions
- name: Run Architecture Tests
  run: dotnet test Tests/Notes.Tests.Architecture/Notes.Tests.Architecture.csproj
```

## Customizing Rules

### Adding New Tests
1. Create a new test class inheriting from `BaseArchitectureTest`
2. Use NetArchTest fluent API to define rules
3. Assert with FluentAssertions for clear error messages

### Example Test
```csharp
[Fact]
public void ControllersShould EndWithController()
{
    var result = Types.InAssembly(WebAssembly)
        .That()
        .ResideInNamespaceContaining("Controllers")
        .And()
        .AreClasses()
        .Should()
        .HaveNameEndingWith("Controller")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"controllers should end with 'Controller'. Failures: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
```

## Framework Exceptions

Some tests include exceptions for framework requirements:

- **MediatR Handlers**: Use `Handle` method name (not `HandleAsync`)
- **Repository Methods**: May not have `Async` suffix when implementing interfaces
- **Compiler-Generated**: Anonymous types and state machines are excluded
- **Layout Components**: Can inherit from `ComponentBase` or `LayoutComponentBase`

## Benefits

? **Prevent Architecture Drift**: Automated checks prevent violations before code review  
? **Documentation**: Tests serve as living documentation of architectural decisions  
? **Onboarding**: New developers understand patterns through failing tests  
? **Refactoring Safety**: Ensure architectural integrity during major changes  
? **Consistency**: Enforce naming and structure conventions across the codebase  

## Troubleshooting

### Test Failures
- Check the detailed error message - it shows exactly which types violate the rule
- Review the failing type against the architectural pattern
- Consider if the rule needs adjustment for your specific case

### Performance
- Architecture tests use reflection and may be slower than unit tests
- Run them separately in CI if needed: `dotnet test --filter Category=Architecture`

## Further Reading

- [NetArchTest Documentation](https://github.com/BenMorris/NetArchTest)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Blazor Component Best Practices](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/)

---

**Maintained by**: NotesApp Team  
**Last Updated**: December 2025  
**Status**: ? All 42 Tests Passing
