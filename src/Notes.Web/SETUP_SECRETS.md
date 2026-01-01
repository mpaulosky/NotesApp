# Setting up OpenAI API Key

For security, you should store your OpenAI API key in User Secrets rather than in appsettings.json.

## Using User Secrets (Recommended)

1. Right-click on the `Notes.Web` project in Visual Studio
2. Select "Manage User Secrets"
3. Add the following configuration:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-openai-api-key-here"
  }
}
```

## Or use .NET CLI

```bash
cd src/Notes.Web
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-openai-api-key-here"
```

## Configuration Priority

The configuration will be merged in this order (later values override earlier ones):
1. appsettings.json
2. appsettings.Development.json
3. User Secrets (Development only)
4. Environment variables

## Required Configuration

The following settings are required in your configuration:
- `OpenAI:ApiKey` - Your OpenAI API key
- `OpenAI:ChatModel` - Model for chat completions (default: "gpt-4o-mini")
- `OpenAI:EmbeddingModel` - Model for embeddings (default: "text-embedding-3-small")
- `OpenAI:MaxSummaryTokens` - Max tokens for summaries (default: 150)
- `OpenAI:RelatedNotesCount` - Number of related notes (default: 5)
- `OpenAI:SimilarityThreshold` - Similarity threshold 0-1 (default: 0.7)

## MongoDB Connection String

Ensure your MongoDB connection string is configured:
- Add to appsettings.json or user secrets:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017/notesdb"
  }
}
```
