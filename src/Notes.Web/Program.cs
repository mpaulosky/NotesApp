var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add a distributed cache
builder.AddRedisOutputCache("RedisCache");

// Data (MongoDB)
builder.AddMongoDb();

// Add AI services (OpenAI)
builder.AddAiServices();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Add services to the container.
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);

	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

// Test endpoints for Redis output cache
app.MapGet("/api/test-cache", () =>
{
	return new
	{
		timestamp = DateTime.UtcNow,
		message = "This response should be cached for 30 seconds"
	};
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromSeconds(30)))
.WithName("TestCache");

app.MapGet("/api/test-cache-with-tag", () =>
{
	return new
	{
		timestamp = DateTime.UtcNow,
		message = "Cached with tag for 5 minutes"
	};
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)).Tag("test-tag"))
.WithName("TestCacheWithTag");

app.MapPost("/api/invalidate-cache", async (IOutputCacheStore cacheStore) =>
{
	await cacheStore.EvictByTagAsync("test-tag", CancellationToken.None);
	return Results.Ok(new { message = "Cache invalidated successfully" });
})
.WithName("InvalidateCache");

app.Run();