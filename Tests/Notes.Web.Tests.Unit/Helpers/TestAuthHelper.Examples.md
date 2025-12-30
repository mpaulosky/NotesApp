# TestAuthHelper Usage Examples

This document provides examples of how to use the enhanced `TestAuthHelper` in your bUnit component tests.

## Overview

The `TestAuthHelper` provides convenient methods for configuring authentication in bUnit tests with support for:

- Authenticated users with custom roles
- Unauthenticated scenarios
- Admin users with admin roles
- Regular users with standard roles

## Usage Examples

### 1. Authenticated User with Custom Roles

```csharp
public class MyComponentTests : BunitContext
{
    [Fact]
    public void Component_WithAuthenticatedUserWithRoles_ShouldRender()
    {
        // Arrange
        TestAuthHelper.RegisterTestAuthentication(Services, "TestUser", new[] { "Admin", "Editor" });
        
        // Act
        var cut = Render<MyComponent>();
        
        // Assert
        cut.Markup.Should().Contain("TestUser");
    }
}
```

### 2. Unauthenticated User

```csharp
[Fact]
public void Component_WhenNotAuthenticated_ShouldShowLoginPrompt()
{
    // Arrange
    TestAuthHelper.RegisterUnauthenticated(Services);
    
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    cut.Markup.Should().Contain("Please log in");
}
```

### 3. Admin User

```csharp
[Fact]
public void Component_WithAdminUser_ShouldShowAdminFeatures()
{
    // Arrange
    TestAuthHelper.RegisterAdminAuthentication(Services, "AdminUser");
    
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    cut.Markup.Should().Contain("Admin Panel");
}
```

### 4. Regular User (No Admin Privileges)

```csharp
[Fact]
public void Component_WithRegularUser_ShouldNotShowAdminFeatures()
{
    // Arrange
    TestAuthHelper.RegisterUserAuthentication(Services, "RegularUser");
    
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    cut.Markup.Should().NotContain("Admin Panel");
}
```

### 5. Custom Admin User Name

```csharp
[Fact]
public void Component_WithCustomAdminName_ShouldDisplayName()
{
    // Arrange
    TestAuthHelper.RegisterAdminAuthentication(Services, "SuperAdmin");
    
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    cut.Markup.Should().Contain("SuperAdmin");
}
```

## Comparison with bUnit's AddTestAuthorization

Both approaches work well. Choose based on your needs:

### Using bUnit's Built-in Method (Recommended for simple scenarios)

```csharp
[Fact]
public void Example_UsingBunitBuiltin()
{
    // Arrange
    var authContext = Services.AddTestAuthorization();
    authContext.SetAuthorized("TestUser");
    authContext.SetRoles("Admin");
    
    // Act & Assert...
}
```

### Using TestAuthHelper (Recommended for role-based testing)

```csharp
[Fact]
public void Example_UsingTestAuthHelper()
{
    // Arrange
    TestAuthHelper.RegisterTestAuthentication(Services, "TestUser", new[] { "Admin", "notes.admin" });
    
    // Act & Assert...
}
```

## Role Constants

The application uses the following role constants (from `Shared.Constants.Constants`):

- `"notes.admin"` - Admin policy role for administrative features

When testing admin functionality, use:

```csharp
TestAuthHelper.RegisterAdminAuthentication(Services);
// This automatically includes both "Admin" and "notes.admin" roles
```

## Notes

1. **Global Usings**: All necessary namespaces are already available via `GlobalUsings.cs`
2. **FakeAuthenticationStateProvider**: The helper uses a custom fake provider that supports the `isAuthenticated` parameter
3. **Flexibility**: You can still use bUnit's `AddTestAuthorization()` for simpler scenarios where role management is not critical
