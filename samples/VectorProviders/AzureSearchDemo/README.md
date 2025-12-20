# Azure AI Search Demo

This sample demonstrates using Azure AI Search with hybrid vector + keyword search in Blazor AutoComplete.

## Prerequisites

1. **Azure AI Search service** (Basic tier or higher for vector search)
2. **Azure OpenAI service** with an embedding model deployment

## Setup

### 1. Create Azure Resources

#### Azure AI Search

1. Go to Azure Portal > Create a resource > Azure AI Search
2. Choose **Basic** tier or higher (Free tier doesn't support vector search)
3. Note down the Endpoint URL and Admin API Key

#### Azure OpenAI

1. Go to Azure Portal > Create a resource > Azure OpenAI
2. Deploy the `text-embedding-ada-002` model (or `text-embedding-3-small`)
3. Note down the Endpoint URL, API Key, and Deployment Name

### 2. Configure Settings

Update `appsettings.json`:

```json
{
  "AzureSearch": {
    "Endpoint": "https://your-search-service.search.windows.net",
    "ApiKey": "your-admin-api-key"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-openai.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "text-embedding-ada-002"
  }
}
```

Or use environment variables:

```bash
export AzureSearch__Endpoint="https://your-search-service.search.windows.net"
export AzureSearch__ApiKey="your-admin-api-key"
export AzureOpenAI__Endpoint="https://your-openai.openai.azure.com/"
export AzureOpenAI__ApiKey="your-api-key"
export AzureOpenAI__DeploymentName="text-embedding-ada-002"
```

### 3. Run the Application

```bash
cd samples/VectorProviders/AzureSearchDemo
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## Hybrid Search

This demo uses Azure AI Search's hybrid search capability, which combines:

1. **Vector Search**: Semantic similarity using embeddings
2. **Keyword Search**: Traditional BM25 text matching

The results are combined using Reciprocal Rank Fusion (RRF) to provide the best of both approaches.

### Benefits of Hybrid Search

| Query Type | Vector Search | Keyword Search | Hybrid Search |
|------------|---------------|----------------|---------------|
| Semantic ("laptop for coding") | Excellent | Poor | Excellent |
| Exact match ("iPhone 15") | Good | Excellent | Excellent |
| Typos ("MacBok") | Good | Poor | Good |
| Product codes ("SKU-12345") | Poor | Excellent | Excellent |

## Configuration Options

```csharp
builder.Services.AddAutoCompleteAzureSearch<Product>(
    configureOptions: options =>
    {
        options.Endpoint = "https://...";
        options.ApiKey = "...";
        options.IndexName = "products";
        options.EnableHybridSearch = true;     // Vector + keyword search
        options.EmbeddingDimensions = 1536;    // text-embedding-ada-002
        options.IndexBatchSize = 100;
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());
```

## Pricing Considerations

- **Azure AI Search**: Basic tier starts at ~$75/month
- **Azure OpenAI**: Pay per 1000 tokens (embedding models are very affordable)

For development/testing, you can share resources with other projects.
