
using Notes.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Configure resources
var redisCache = builder.AddRedisServices();

// Use MongoDB Atlas (cloud) instead of local container
var mongoDb = builder.AddMongoDbAtlasDatabase(DatabaseName);

builder.AddProject<Projects.Notes_Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(mongoDb)
		.WaitFor(mongoDb)
		.WithReference(redisCache)
		.WaitFor(redisCache);

builder.Build().Run();