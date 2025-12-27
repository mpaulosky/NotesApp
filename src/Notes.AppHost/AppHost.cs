
using Notes.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Configure resources
var redisCache = builder.AddRedisServices();
var mongoDb = builder.AddMongoDbServices();

builder.AddProject<Projects.Notes_Web>("frontend")
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(redisCache).WaitFor(redisCache)
		.WithReference(mongoDb).WaitFor(mongoDb);

builder.Build().Run();