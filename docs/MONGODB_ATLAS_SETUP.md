# MongoDB Atlas Setup Guide

## Overview

This project now supports MongoDB Atlas (cloud-hosted MongoDB) in addition to local MongoDB containers. The AppHost has been configured with dedicated methods for Atlas integration.

## Setup Instructions

### 1. Create MongoDB Atlas Account

1. Go to [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. Sign up for a free account (M0 tier available)
3. Create a new cluster

### 2. Get Your Connection String

1. In MongoDB Atlas dashboard, click **"Connect"** on your cluster
2. Choose **"Connect your application"**
3. Copy the connection string (it looks like):
   ```
   mongodb+srv://<username>:<password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority
   ```
4. Replace `<username>` and `<password>` with your actual credentials

### 3. Configure User Secrets

Store your MongoDB Atlas connection string securely using .NET User Secrets:

```powershell
# Navigate to the AppHost project directory
cd src\Notes.AppHost

# Set the connection string
dotnet user-secrets set "ConnectionStrings:mongodb" "mongodb+srv://<username>:<password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority"
```

### 4. Update AppHost Configuration

The [AppHost.cs](../src/Notes.AppHost/AppHost.cs) is already configured to use MongoDB Atlas:

```csharp
// Use MongoDB Atlas (cloud) instead of local container
var mongoDb = builder.AddMongoDbAtlasDatabase("NotesDb");

builder.AddProject<Projects.Notes_Web>("frontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(mongoDb)
    .WaitFor(mongoDb);
```

## Switching Between Local and Atlas

### Use Local MongoDB (Container)

```csharp
var mongoDb = builder.AddMongoDbServices();
```

### Use MongoDB Atlas (Cloud)

```csharp
var mongoDb = builder.AddMongoDbAtlasDatabase("NotesDb");
```

## Implementation Details

The [DatabaseService.cs](../src/Notes.AppHost/DatabaseService.cs) contains two new extension methods:

- `AddMongoDbAtlas()` - Adds the MongoDB Atlas server resource
- `AddMongoDbAtlasDatabase(string databaseName)` - Adds a specific database reference

These methods use `.PublishAsConnectionString()` to work with cloud-hosted MongoDB instead of local containers.

## Key Differences

| Feature | Local MongoDB | MongoDB Atlas |
|---------|--------------|---------------|
| Container | ✅ Docker container | ❌ Cloud-hosted |
| Data Volume | ✅ Local storage | ❌ Cloud storage |
| Mongo Express | ✅ Included | ❌ Use Atlas UI |
| Connection | Local port | Connection string |
| Lifetime Management | Persistent/Session | Always running |

## Troubleshooting

### Connection String Not Found

If you get an error about missing connection string:

1. Verify user secrets are set: `dotnet user-secrets list`
2. Ensure you're in the correct project directory
3. Check the connection string name matches: `ConnectionStrings:mongodb`

### Authentication Failed

1. Verify username and password in connection string
2. Check MongoDB Atlas Network Access (whitelist your IP)
3. Verify Database Access user has correct permissions

### Network Access

In MongoDB Atlas:
1. Go to **Network Access**
2. Add your IP address or use `0.0.0.0/0` for testing (not recommended for production)

## Production Deployment

For production deployments, consider:

1. Use Azure Key Vault or similar for connection string storage
2. Configure proper network access rules in MongoDB Atlas
3. Use least-privilege database user credentials
4. Enable IP whitelisting
5. Monitor connection limits and usage

## References

- [MongoDB Atlas Documentation](https://docs.atlas.mongodb.com/)
- [.NET Aspire MongoDB Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)
- [User Secrets in .NET](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
