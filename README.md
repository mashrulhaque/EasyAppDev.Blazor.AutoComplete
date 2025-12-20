# Blazor AutoComplete Component

A high-performance, AI-powered AutoComplete (TypeAhead) component for Blazor applications with semantic search, OData integration, and vector database support. Also known as: autosuggest, combobox, searchable dropdown, live search.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EasyAppDev.Blazor.AutoComplete.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)
[![GitHub stars](https://img.shields.io/github/stars/mashrulhaque/EasyAppDev.Blazor.AutoComplete?style=social)](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-blue)](https://dotnet.microsoft.com/)

**[Live Demo](https://blazorautocomplete.easyappdev.com/)** | [NuGet Package](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/) | [Get Started](#quick-start)

<p align="center">
  <img src="https://blazorautocomplete.easyappdev.com/images/demo-preview.gif" alt="Blazor AutoComplete Demo" width="600" />
</p>

---

## Why Choose This Blazor AutoComplete?

- **AI-Powered Semantic Search** - Find results by meaning, not just keywords, using OpenAI, Azure OpenAI, or any embedding provider
- **OData v3/v4 Integration** - Server-side filtering with automatic `$filter` query generation
- **5 Vector Database Providers** - PostgreSQL/pgvector, Azure AI Search, Pinecone, Qdrant, CosmosDB
- **< 15KB Gzipped** - Lightweight core with zero runtime reflection
- **Native AOT Ready** - Source generators enable full ahead-of-time compilation and trimming
- **WCAG 2.1 AA Accessible** - ARIA 1.2 combobox pattern with full keyboard navigation
- **100K+ Items** - Virtualization maintains 60fps scrolling with massive datasets
- **8 Built-in Display Modes** - Eliminate custom template boilerplate

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Features](#core-features)
  - [Multi-Field Search](#multi-field-search)
  - [Filter Strategies](#filter-strategies)
  - [Display Modes](#display-modes)
  - [Grouping](#grouping)
  - [Virtualization](#virtualization)
  - [Async Data Sources](#async-data-sources)
  - [Custom Templates](#custom-templates)
  - [Fluent Configuration API](#fluent-configuration-api)
- [Theming System](#theming-system)
  - [Theme Presets](#theme-presets)
  - [Light/Dark Mode](#lightdark-mode)
  - [Bootstrap Color Variants](#bootstrap-color-variants)
  - [Component Sizes](#component-sizes)
  - [Custom Theme Properties](#custom-theme-properties)
- [OData Integration](#odata-integration)
  - [OData Setup](#odata-setup)
  - [OData Multi-Field Search](#odata-multi-field-search)
  - [OData v3 Support](#odata-v3-support)
  - [OData Filter Strategies](#odata-filter-strategies)
  - [OData Configuration Options](#odata-configuration-options)
- [AI Semantic Search](#ai-semantic-search)
  - [Semantic Search Setup](#semantic-search-setup)
  - [SemanticAutoComplete Component](#semanticautocomplete-component)
  - [Embedding Cache System](#embedding-cache-system)
  - [Semantic Search Parameters](#semantic-search-parameters)
- [Vector Database Providers](#vector-database-providers)
  - [PostgreSQL with pgvector](#postgresql-with-pgvector)
  - [Azure AI Search](#azure-ai-search)
  - [Pinecone](#pinecone)
  - [Qdrant](#qdrant)
  - [Azure CosmosDB](#azure-cosmosdb)
  - [Indexing Data](#indexing-data)
- [Accessibility](#accessibility)
- [Security](#security)
- [API Reference](#api-reference)
- [Packages](#packages)
- [Requirements](#requirements)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Support](#support)

---

## Installation

### Core Package

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete
```

### Optional Packages

```bash
# OData server-side filtering
dotnet add package EasyAppDev.Blazor.AutoComplete.OData

# AI semantic search with embeddings
dotnet add package EasyAppDev.Blazor.AutoComplete.AI

# Vector database providers
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.PostgreSql
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.AzureSearch
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Pinecone
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Qdrant
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.CosmosDb
```

---

## Quick Start

### 1. Register Services

Add to `Program.cs`:

```csharp
using EasyAppDev.Blazor.AutoComplete.Extensions;

builder.Services.AddAutoComplete();
```

This registers:
- `IThemeManager` - Theme CSS generation (singleton)
- `IAutoCompleteServiceFactory` - Component service factory (singleton)

### 2. Add Styles

Add to `App.razor` or `index.html`:

```html
<head>
    <link href="_content/EasyAppDev.Blazor.AutoComplete/styles/autocomplete.base.css" rel="stylesheet" />
    <script src="_content/EasyAppDev.Blazor.AutoComplete/scripts/theme-loader.js"></script>
</head>
```

### 3. Use the Component

```razor
@using EasyAppDev.Blazor.AutoComplete

<AutoComplete TItem="Product"
              Items="@products"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct"
              Placeholder="Search products..." />

@code {
    private List<Product> products = new()
    {
        new Product { Id = 1, Name = "Apple", Category = "Fruits" },
        new Product { Id = 2, Name = "Banana", Category = "Fruits" },
        new Product { Id = 3, Name = "Carrot", Category = "Vegetables" }
    };
    private Product? selectedProduct;
}
```

> **Note:** The component works without `AddAutoComplete()` using fallback instances, but registering services enables proper dependency injection, testability, and singleton behavior.

---

## Core Features

### Multi-Field Search

Search across multiple properties simultaneously:

```razor
<AutoComplete TItem="Product"
              Items="@products"
              SearchFields="@(p => new[] { p.Name, p.Description, p.Category, p.SKU })"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct" />
```

When `SearchFields` is provided, filtering matches against all specified fields with OR logic.

### Filter Strategies

Four built-in filtering algorithms:

```razor
<!-- Prefix matching (default, fastest) -->
<AutoComplete FilterStrategy="FilterStrategy.StartsWith" ... />

<!-- Substring matching anywhere -->
<AutoComplete FilterStrategy="FilterStrategy.Contains" ... />

<!-- Typo-tolerant with Levenshtein distance -->
<AutoComplete FilterStrategy="FilterStrategy.Fuzzy" ... />

<!-- Custom filter implementation -->
<AutoComplete FilterStrategy="FilterStrategy.Custom"
              CustomFilter="@myFilterEngine" ... />
```

**Fuzzy Search** uses Levenshtein distance algorithm with configurable tolerance for handling typos and misspellings.

### Display Modes

Eight built-in layouts eliminate custom template boilerplate:

```razor
@using EasyAppDev.Blazor.AutoComplete.Options

<!-- Simple text only -->
<AutoComplete DisplayMode="ItemDisplayMode.Simple" ... />

<!-- Title + description (two-line) -->
<AutoComplete DisplayMode="ItemDisplayMode.TitleWithDescription"
              DescriptionField="@(p => p.Category)" ... />

<!-- Title + right-aligned badge -->
<AutoComplete DisplayMode="ItemDisplayMode.TitleWithBadge"
              BadgeField="@(p => $"${p.Price:F2}")"
              BadgeClass="badge bg-success" ... />

<!-- Title + description + badge -->
<AutoComplete DisplayMode="ItemDisplayMode.TitleDescriptionBadge"
              DescriptionField="@(p => p.Category)"
              BadgeField="@(p => p.Stock.ToString())" ... />

<!-- Icon/emoji + title -->
<AutoComplete DisplayMode="ItemDisplayMode.IconWithTitle"
              IconField="@(p => p.Emoji)" ... />

<!-- Icon + title + description -->
<AutoComplete DisplayMode="ItemDisplayMode.IconTitleDescription"
              IconField="@(p => p.Emoji)"
              DescriptionField="@(p => p.Category)" ... />

<!-- Card layout with all fields -->
<AutoComplete DisplayMode="ItemDisplayMode.Card"
              IconField="@(p => p.Emoji)"
              SubtitleField="@(p => p.Category)"
              DescriptionField="@(p => p.Description)"
              BadgeField="@(p => $"${p.Price:F2}")" ... />
```

**Available modes:** `Custom`, `Simple`, `TitleWithDescription`, `TitleWithBadge`, `TitleDescriptionBadge`, `IconWithTitle`, `IconTitleDescription`, `Card`

### Grouping

Group items by category with custom headers:

```razor
<AutoComplete TItem="Product"
              Items="@products"
              GroupBy="@(p => p.Category)"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct">
    <GroupTemplate Context="group">
        <div class="group-header">
            <strong>@group.Key</strong>
            <span class="badge bg-secondary">@group.Count()</span>
        </div>
    </GroupTemplate>
</AutoComplete>
```

### Virtualization

Handle massive datasets with smooth 60fps scrolling:

```razor
<AutoComplete TItem="Product"
              Items="@largeDataset"
              Virtualize="true"
              VirtualizationThreshold="100"
              ItemHeight="40"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct" />
```

Virtualization automatically activates when item count exceeds `VirtualizationThreshold`. Tested with 100,000+ items.

### Async Data Sources

Fetch data from remote APIs:

```razor
<AutoComplete TItem="Product"
              DataSource="@dataSource"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct" />

@code {
    private IAutoCompleteDataSource<Product> dataSource = new RemoteDataSource<Product>(
        async (query, cancellationToken) =>
            await httpClient.GetFromJsonAsync<List<Product>>(
                $"/api/products?search={Uri.EscapeDataString(query)}",
                cancellationToken)
    );
}
```

### Custom Templates

Full control over rendering:

```razor
<AutoComplete TItem="Product"
              Items="@products"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct">
    <ItemTemplate Context="product">
        <div class="d-flex align-items-center gap-2">
            <img src="@product.ImageUrl" width="32" height="32" alt="@product.Name" />
            <div>
                <strong>@product.Name</strong>
                <small class="text-muted d-block">@product.Category</small>
            </div>
            <span class="ms-auto badge bg-primary">$@product.Price</span>
        </div>
    </ItemTemplate>
    <NoResultsTemplate>
        <div class="text-center py-3">
            <i class="bi bi-search"></i>
            <p>No products found</p>
        </div>
    </NoResultsTemplate>
    <LoadingTemplate>
        <div class="text-center py-3">
            <div class="spinner-border spinner-border-sm"></div>
            <span>Searching...</span>
        </div>
    </LoadingTemplate>
    <HeaderTemplate>
        <div class="dropdown-header">Select a product</div>
    </HeaderTemplate>
    <FooterTemplate>
        <div class="dropdown-footer text-muted small">
            Type to search products
        </div>
    </FooterTemplate>
</AutoComplete>
```

### Fluent Configuration API

Build complex configurations with method chaining:

```csharp
var config = AutoCompleteConfig<Product>.Create()
    .WithItems(products)
    .WithTextField(p => p.Name)
    .WithSearchFields(p => new[] { p.Name, p.Description, p.Category })
    .WithDisplayMode(ItemDisplayMode.TitleWithDescription)
    .WithTitleAndDescription(p => p.Description)
    .WithFilterStrategy(FilterStrategy.Contains)
    .WithTheme(Theme.Auto)
    .WithBootstrapTheme(BootstrapTheme.Primary)
    .WithVirtualization(threshold: 1000, itemHeight: 45)
    .WithGrouping(p => p.Category)
    .WithDebounce(300)
    .Build();
```

```razor
<AutoComplete TItem="Product" Config="@config" @bind-Value="@selectedProduct" />
```

---

## Theming System

### Theme Presets

Four professional design system presets:

```razor
<!-- Google Material Design 3 -->
<AutoComplete ThemePreset="ThemePreset.Material" ... />

<!-- Microsoft Fluent Design -->
<AutoComplete ThemePreset="ThemePreset.Fluent" ... />

<!-- Clean, minimalist design -->
<AutoComplete ThemePreset="ThemePreset.Modern" ... />

<!-- Bootstrap 5 integration -->
<AutoComplete ThemePreset="ThemePreset.Bootstrap" ... />
```

### Light/Dark Mode

Automatic system preference detection:

```razor
<!-- Follow system preference (default) -->
<AutoComplete Theme="Theme.Auto" ... />

<!-- Force light mode -->
<AutoComplete Theme="Theme.Light" ... />

<!-- Force dark mode -->
<AutoComplete Theme="Theme.Dark" ... />
```

### Bootstrap Color Variants

Nine Bootstrap 5 color schemes:

```razor
<AutoComplete BootstrapTheme="BootstrapTheme.Primary" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Secondary" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Success" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Danger" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Warning" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Info" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Light" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Dark" ... />
```

### Component Sizes

Three size variants:

```razor
<AutoComplete Size="ComponentSize.Compact" ... />  <!-- Smaller padding -->
<AutoComplete Size="ComponentSize.Default" ... />  <!-- Standard size -->
<AutoComplete Size="ComponentSize.Large" ... />    <!-- Larger padding -->
```

### Custom Theme Properties

**Simple overrides (1-5 properties):**

```razor
<AutoComplete PrimaryColor="#FF6B6B"
              BackgroundColor="#FFFFFF"
              TextColor="#212529"
              BorderColor="#ced4da"
              BorderRadius="8px"
              FontFamily="Inter, sans-serif"
              FontSize="14px"
              DropdownShadow="0 4px 12px rgba(0,0,0,0.15)"
              ... />
```

**Structured overrides (advanced):**

```razor
<AutoComplete ThemeOverrides="@(new ThemeOptions {
    Colors = new ColorOptions {
        Primary = "#FF6B6B",
        Hover = "#f8f9fa",
        Selected = "#e9ecef"
    },
    Spacing = new SpacingOptions {
        BorderRadius = "8px",
        InputPadding = "12px 16px"
    },
    Typography = new TypographyOptions {
        FontFamily = "Inter, sans-serif",
        FontSize = "14px"
    },
    Effects = new EffectOptions {
        DropdownShadow = "0 4px 12px rgba(0,0,0,0.15)",
        TransitionDuration = "150ms"
    }
})" ... />
```

**Precedence:** Individual parameters > ThemeOverrides > Theme presets > Defaults

---

## OData Integration

The OData package enables server-side filtering with automatic `$filter` query generation for OData v3 and v4 endpoints.

### OData Setup

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.OData
```

```razor
@using EasyAppDev.Blazor.AutoComplete
@using EasyAppDev.Blazor.AutoComplete.OData
@inject HttpClient Http

<AutoComplete TItem="Product"
              DataSource="@_odataSource"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct"
              Placeholder="Search products..." />

@code {
    private ODataDataSource<Product> _odataSource = null!;
    private Product? selectedProduct;

    protected override void OnInitialized()
    {
        var options = new ODataOptions
        {
            EndpointUrl = "https://api.example.com/odata/Products",
            FilterStrategy = ODataFilterStrategy.StartsWith,
            Top = 20,
            CaseInsensitive = true
        };
        _odataSource = new ODataDataSource<Product>(Http, options, "Name");
    }
}
```

**Generated OData query:**
```
GET /odata/Products?$filter=startswith(tolower(Name),'apple')&$top=20
```

### OData Multi-Field Search

Search across multiple fields with OR combination:

```csharp
// Search Name, Description, and Category simultaneously
_odataSource = new ODataDataSource<Product>(
    Http,
    options,
    searchFieldNames: new[] { "Name", "Description", "Category" }
);
```

**Generated OData query:**
```
$filter=(startswith(tolower(Name),'search') or startswith(tolower(Description),'search') or startswith(tolower(Category),'search'))
```

### OData v3 Support

For legacy OData v3 endpoints:

```csharp
var options = new ODataOptions
{
    EndpointUrl = "https://legacy-api.example.com/odata/Products",
    Version = ODataVersion.V3,  // Use v3 syntax
    FilterStrategy = ODataFilterStrategy.Contains
};
```

### OData Filter Strategies

| Strategy | OData v4 | OData v3 |
|----------|----------|----------|
| `StartsWith` | `startswith(field,'value')` | `startswith(field,'value')` |
| `Contains` | `contains(field,'value')` | `substringof('value',field)` |
| `FuzzyFallback` | `contains()` + client re-rank | `substringof()` + client re-rank |

**FuzzyFallback** performs server-side contains filtering, then applies Levenshtein distance re-ranking on the client for typo tolerance.

### OData Configuration Options

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `EndpointUrl` | `string` | Required | OData endpoint URL |
| `Version` | `ODataVersion` | `V4` | OData protocol version (V3 or V4) |
| `FilterStrategy` | `ODataFilterStrategy` | `StartsWith` | Server-side filter type |
| `Top` | `int` | `100` | Maximum results (`$top`) |
| `Select` | `string[]?` | `null` | Fields to return (`$select`) |
| `OrderBy` | `string?` | `null` | Sort order (`$orderby`) |
| `AdditionalFilter` | `string?` | `null` | Static filter ANDed with search |
| `CaseInsensitive` | `bool` | `true` | Wrap fields with `tolower()` |
| `MinSearchLength` | `int` | `1` | Minimum characters before API call |
| `TimeoutSeconds` | `int` | `30` | HTTP request timeout |
| `CustomHeaders` | `Dictionary<string,string>?` | `null` | HTTP headers (Authorization, etc.) |
| `ResultsPropertyName` | `string` | `"value"` | JSON property containing results array |
| `IncludeCount` | `bool` | `false` | Include `$count` in response |

### OData with Fluent Builder

```csharp
var config = AutoCompleteConfig<Product>.Create()
    .WithODataSource(Http, "https://api.example.com/odata/Products", "Name",
        opts => {
            opts.FilterStrategy = ODataFilterStrategy.Contains;
            opts.Top = 25;
            opts.OrderBy = "Name asc";
            opts.AdditionalFilter = "IsActive eq true";
        })
    .WithDisplayMode(ItemDisplayMode.TitleWithDescription)
    .WithTitleAndDescription(p => p.Description)
    .Build();
```

### OData Service Registration

**Configuration-based:**

```json
// appsettings.json
{
  "ODataSettings": {
    "EndpointUrl": "https://api.example.com/odata/Products",
    "FilterStrategy": "Contains",
    "Top": 50
  }
}
```

```csharp
builder.Services.AddAutoCompleteOData(builder.Configuration, "ODataSettings");
```

**Explicit configuration:**

```csharp
builder.Services.AddAutoCompleteOData(
    "https://api.example.com/odata/Products",
    options => {
        options.FilterStrategy = ODataFilterStrategy.Contains;
        options.Top = 50;
        options.CustomHeaders = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer your-token"
        };
    });
```

---

## AI Semantic Search

The AI package enables meaning-based search using text embeddings and vector similarity. Find "automobile" when searching for "car".

### Semantic Search Setup

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI
```

**OpenAI Configuration:**

```csharp
// Using appsettings.json
builder.Services.AddAutoCompleteSemanticSearch(builder.Configuration);

// Or explicit configuration
builder.Services.AddAutoCompleteSemanticSearch(
    apiKey: "sk-...",
    model: "text-embedding-3-small"  // Default model
);
```

```json
// appsettings.json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "text-embedding-3-small"
  }
}
```

**Azure OpenAI Configuration:**

```csharp
builder.Services.AddAutoCompleteSemanticSearchWithAzure(
    endpoint: "https://my-resource.openai.azure.com/",
    apiKey: "your-azure-openai-key",
    deploymentName: "text-embedding-ada-002"
);
```

```json
// appsettings.json
{
  "AzureOpenAI": {
    "Endpoint": "https://my-resource.openai.azure.com/",
    "ApiKey": "your-azure-openai-key",
    "DeploymentName": "text-embedding-ada-002"
  }
}
```

### SemanticAutoComplete Component

High-level wrapper for AI-powered search:

```razor
@using EasyAppDev.Blazor.AutoComplete.AI

<SemanticAutoComplete TItem="Document"
                      Items="@documents"
                      TextField="@(d => d.Title)"
                      SearchFields="@(d => new[] { d.Title, d.Description, d.Tags })"
                      SimilarityThreshold="0.15"
                      MinSearchLength="3"
                      DebounceMs="500"
                      PreWarmCache="true"
                      ShowCacheStatus="true"
                      @bind-Value="@selectedDocument"
                      Placeholder="Search by meaning..." />

@code {
    private List<Document> documents = new();
    private Document? selectedDocument;
}
```

### Embedding Cache System

The AI package includes an intelligent dual-caching system to minimize API costs:

**Item Cache:**
- Caches embeddings for your data items
- Default TTL: 1 hour
- Default max size: 10,000 items
- LRU eviction when full

**Query Cache:**
- Caches embeddings for user search queries
- Default TTL: 15 minutes
- Default max size: 1,000 queries
- LRU eviction when full

**Pre-warming:**
```razor
<SemanticAutoComplete PreWarmCache="true" ... />
```

Generates embeddings for all items on initialization. Shows progress via `CacheWarmingProgress` event.

### Semantic Search Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SimilarityThreshold` | `float` | `0.15` | Minimum cosine similarity (0-1) for results |
| `MinSearchLength` | `int` | `3` | Characters before semantic search triggers |
| `DebounceMs` | `int` | `500` | Delay before API call |
| `MaxResults` | `int?` | `null` | Maximum results (null = all matching) |
| `ItemCacheDuration` | `TimeSpan` | `1 hour` | Item embedding cache TTL |
| `QueryCacheDuration` | `TimeSpan` | `15 min` | Query embedding cache TTL |
| `MaxItemCacheSize` | `int` | `10,000` | Maximum cached item embeddings |
| `MaxQueryCacheSize` | `int` | `1,000` | Maximum cached query embeddings |
| `PreWarmCache` | `bool` | `false` | Generate all embeddings on init |
| `ShowCacheStatus` | `bool` | `true` | Display cache hit rate statistics |

---

## Vector Database Providers

For production deployments with persistent storage and scalable semantic search, use external vector database providers. These eliminate embedding regeneration on restart and support millions of items.

### Supported Providers

| Provider | Package | Best For |
|----------|---------|----------|
| PostgreSQL/pgvector | `EasyAppDev.Blazor.AutoComplete.AI.PostgreSql` | Self-hosted, existing Postgres infrastructure |
| Azure AI Search | `EasyAppDev.Blazor.AutoComplete.AI.AzureSearch` | Enterprise, hybrid search, managed service |
| Pinecone | `EasyAppDev.Blazor.AutoComplete.AI.Pinecone` | Serverless, automatic scaling |
| Qdrant | `EasyAppDev.Blazor.AutoComplete.AI.Qdrant` | Open-source, self-hosted, advanced filtering |
| Azure CosmosDB | `EasyAppDev.Blazor.AutoComplete.AI.CosmosDb` | Global distribution, multi-model |

### PostgreSQL with pgvector

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.PostgreSql
```

**Configuration:**

```json
{
  "VectorSearch": {
    "PostgreSQL": {
      "ConnectionString": "Host=localhost;Database=myapp;Username=user;Password=pass",
      "CollectionName": "products",
      "EmbeddingDimensions": 1536
    }
  },
  "OpenAI": {
    "ApiKey": "sk-..."
  }
}
```

```csharp
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Extensions;

builder.Services.AddAutoCompletePostgres<Product>(
    builder.Configuration,
    textSelector: p => $"{p.Name} {p.Description} {p.Category}",
    idSelector: p => p.Id.ToString());

builder.Services.AddAutoCompleteVectorSearch<Product>(builder.Configuration);
```

**Features:**
- 6 distance functions (Cosine, L2, DotProduct, Manhattan, Hamming, Jaccard)
- HNSW indexing for fast approximate nearest neighbor search
- Self-hosted with your existing PostgreSQL infrastructure

### Azure AI Search

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.AzureSearch
```

**Configuration:**

```json
{
  "VectorSearch": {
    "AzureSearch": {
      "Endpoint": "https://my-search.search.windows.net",
      "ApiKey": "your-search-api-key",
      "IndexName": "products",
      "EmbeddingDimensions": 1536,
      "EnableHybridSearch": true
    }
  },
  "AzureOpenAI": {
    "Endpoint": "https://my-openai.openai.azure.com/",
    "ApiKey": "your-openai-key",
    "DeploymentName": "text-embedding-ada-002"
  }
}
```

```csharp
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Extensions;

builder.Services.AddAutoCompleteAzureSearch<Product>(
    builder.Configuration,
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());

builder.Services.AddAutoCompleteVectorSearchWithAzure<Product>(builder.Configuration);
```

**Features:**
- Hybrid search (vector + keyword)
- Semantic ranking
- Enterprise-grade managed service

### Pinecone

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Pinecone
```

```json
{
  "VectorSearch": {
    "Pinecone": {
      "ApiKey": "your-pinecone-api-key",
      "IndexName": "products",
      "Namespace": "default",
      "EmbeddingDimensions": 1536
    }
  }
}
```

```csharp
builder.Services.AddAutoCompletePinecone<Product>(
    builder.Configuration,
    textSelector: p => p.Name);
```

**Features:**
- Serverless architecture
- Namespace isolation
- Automatic scaling
- High availability

### Qdrant

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Qdrant
```

```json
{
  "VectorSearch": {
    "Qdrant": {
      "Host": "localhost",
      "Port": 6334,
      "Https": false,
      "ApiKey": "your-api-key-for-cloud",
      "CollectionName": "products",
      "EmbeddingDimensions": 1536
    }
  }
}
```

```csharp
builder.Services.AddAutoCompleteQdrant<Product>(
    builder.Configuration,
    textSelector: p => p.Name);
```

**Features:**
- Open-source
- Self-hosted or cloud
- Advanced filtering with payload
- HNSW indexing

### Azure CosmosDB

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.CosmosDb
```

```json
{
  "VectorSearch": {
    "CosmosDb": {
      "ConnectionString": "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=...",
      "DatabaseName": "myapp",
      "ContainerName": "products",
      "EmbeddingDimensions": 1536,
      "VectorIndexType": "quantizedFlat"
    }
  }
}
```

```csharp
builder.Services.AddAutoCompleteCosmosDb<Product>(
    builder.Configuration,
    textSelector: p => p.Name);
```

**Features:**
- Multi-model database
- Global distribution
- Integrated NoSQL
- Multi-region replication

### Indexing Data

Before searching, index your data:

```csharp
public class ProductService
{
    private readonly IVectorIndexer<Product> _indexer;

    public ProductService(IVectorIndexer<Product> indexer)
    {
        _indexer = indexer;
    }

    public async Task IndexProductsAsync(IEnumerable<Product> products)
    {
        // Ensure collection/index exists
        await _indexer.EnsureCollectionExistsAsync();

        // Index all items (with progress reporting)
        _indexer.ProgressChanged += (sender, e) =>
            Console.WriteLine($"Indexed {e.ProcessedItems}/{e.TotalItems}");

        await _indexer.IndexAsync(products);
    }

    public async Task IndexSingleProductAsync(Product product)
    {
        // Upsert single item
        await _indexer.IndexAsync(product);
    }
}
```

### When to Use Vector Providers

| Scenario | Recommendation |
|----------|----------------|
| Development/Prototyping | In-memory `SemanticSearchDataSource` |
| Small datasets (< 10K items) | Either approach works |
| Production (> 10K items) | Use vector provider |
| Persistence across restarts | Use vector provider |
| Multi-instance deployment | Use vector provider (shared database) |
| Hybrid search (vector + keyword) | Azure AI Search or CosmosDB |

---

## Accessibility

The component follows **WCAG 2.1 AA** guidelines and implements the **ARIA 1.2 Combobox** pattern.

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Arrow Down` | Open dropdown / Select next item |
| `Arrow Up` | Select previous item |
| `Enter` | Confirm selection |
| `Escape` | Close dropdown |
| `Home` | Jump to first item |
| `End` | Jump to last item |
| `Tab` | Close dropdown and move focus |

### ARIA Attributes

- `role="combobox"` on input
- `role="listbox"` on dropdown
- `role="option"` on items
- `aria-expanded` - Dropdown state
- `aria-activedescendant` - Current focus
- `aria-selected` - Selected item

### Screen Reader Support

```razor
<label for="product-search">Search Products:</label>
<AutoComplete InputId="product-search"
              AriaLabel="Search and select a product"
              ... />
```

### Additional Features

- **High Contrast Mode** - Respects `prefers-contrast: high`
- **Reduced Motion** - Respects `prefers-reduced-motion: reduce`
- **RTL Support** - Set `RightToLeft="true"` for right-to-left languages
- **Form Integration** - Works with Blazor `EditForm` validation

---

## Security

### Input Protection

- **CSS Sanitization** - Theme values validated against allowlists
- **Input Limits** - `MaxSearchLength` prevents memory exhaustion (default 500, max 2000)
- **ReDoS Protection** - Regex patterns use 100ms timeouts
- **OData Escaping** - Search terms properly escaped to prevent injection

### API Security

- **API Key Redaction** - Sensitive data removed from error messages and logs
- **Rate Limiting** - Token bucket algorithm protects embedding API quotas
- **Memory Pressure Monitoring** - Automatic cache eviction under memory pressure

### Configuration

```csharp
// All security limits defined in AutoCompleteConstants
public static class AutoCompleteConstants
{
    public const int DefaultMaxSearchLength = 500;
    public const int AbsoluteMaxSearchLength = 2000;
    public const int RegexTimeoutMs = 100;
    // ... more constants
}
```

---

## API Reference

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | `null` | Collection of items |
| `DataSource` | `IAutoCompleteDataSource<TItem>?` | `null` | Async data source (precedence over Items) |
| `Value` | `TItem?` | `null` | Selected value (two-way bindable) |
| `ValueChanged` | `EventCallback<TItem?>` | | Selection change event |
| `TextField` | `Expression<Func<TItem, string>>?` | `null` | Display text property selector |
| `SearchFields` | `Expression<Func<TItem, string[]>>?` | `null` | Multi-field search selectors |
| `Placeholder` | `string?` | `null` | Input placeholder text |
| `MinSearchLength` | `int` | `1` | Minimum characters before search |
| `MaxSearchLength` | `int` | `500` | Maximum input length |
| `MaxDisplayedItems` | `int` | `100` | Maximum items shown |
| `DebounceMs` | `int` | `300` | Debounce delay in milliseconds |
| `AllowClear` | `bool` | `true` | Show clear button |
| `Disabled` | `bool` | `false` | Disable the component |
| `CloseOnSelect` | `bool` | `true` | Close dropdown on selection |
| `Config` | `AutoCompleteConfig<TItem>?` | `null` | Fluent configuration object |

### Filtering Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `FilterStrategy` | `FilterStrategy` | `StartsWith` | Filter algorithm |
| `CustomFilter` | `IFilterEngine<TItem>?` | `null` | Custom filter implementation |

### Display Mode Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DisplayMode` | `ItemDisplayMode` | `Custom` | Built-in display layout |
| `DescriptionField` | `Expression<Func<TItem, string>>?` | `null` | Description property |
| `BadgeField` | `Expression<Func<TItem, string>>?` | `null` | Badge property |
| `IconField` | `Expression<Func<TItem, string>>?` | `null` | Icon/emoji property |
| `SubtitleField` | `Expression<Func<TItem, string>>?` | `null` | Subtitle (Card mode) |
| `BadgeClass` | `string` | `"badge bg-primary"` | Badge CSS class |

### Theming Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Theme` | `Theme` | `Auto` | Light/Dark/Auto |
| `ThemePreset` | `ThemePreset` | `None` | Design system preset |
| `BootstrapTheme` | `BootstrapTheme` | `Default` | Bootstrap color variant |
| `Size` | `ComponentSize` | `Default` | Component size |
| `ThemeOverrides` | `ThemeOptions?` | `null` | Structured theme overrides |
| `PrimaryColor` | `string?` | `null` | Primary color override |
| `BackgroundColor` | `string?` | `null` | Background color override |
| `TextColor` | `string?` | `null` | Text color override |
| `BorderColor` | `string?` | `null` | Border color override |
| `BorderRadius` | `string?` | `null` | Border radius override |
| `FontFamily` | `string?` | `null` | Font family override |
| `FontSize` | `string?` | `null` | Font size override |
| `DropdownShadow` | `string?` | `null` | Dropdown shadow override |
| `EnableThemeTransitions` | `bool` | `true` | Enable smooth transitions |
| `RightToLeft` | `bool` | `false` | RTL text direction |

### Virtualization Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Virtualize` | `bool` | `false` | Enable virtualization |
| `VirtualizationThreshold` | `int` | `100` | Item count threshold |
| `ItemHeight` | `float` | `40` | Item height in pixels |

### Grouping Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `GroupBy` | `Expression<Func<TItem, object>>?` | `null` | Grouping property selector |
| `GroupTemplate` | `RenderFragment<IGrouping<object, TItem>>?` | `null` | Group header template |

### Template Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `ItemTemplate` | `RenderFragment<TItem>?` | Custom item rendering |
| `NoResultsTemplate` | `RenderFragment?` | No results message |
| `LoadingTemplate` | `RenderFragment?` | Loading indicator |
| `HeaderTemplate` | `RenderFragment?` | Dropdown header |
| `FooterTemplate` | `RenderFragment?` | Dropdown footer |

### Accessibility Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `AriaLabel` | `string?` | ARIA label for screen readers |
| `InputId` | `string?` | Input element ID for label association |
| `ValueExpression` | `Expression<Func<TItem?>>?` | Form validation expression |

---

## Packages

| Package | Description | Size |
|---------|-------------|------|
| `EasyAppDev.Blazor.AutoComplete` | Core component library | < 15KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.Generators` | Source generators (build-time only) | 0KB runtime |
| `EasyAppDev.Blazor.AutoComplete.OData` | OData v3/v4 server-side filtering | < 5KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI` | Semantic search with embeddings | < 25KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI.PostgreSql` | PostgreSQL/pgvector provider | < 10KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI.AzureSearch` | Azure AI Search provider | < 10KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI.Pinecone` | Pinecone provider | < 10KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI.Qdrant` | Qdrant provider | < 10KB gzipped |
| `EasyAppDev.Blazor.AutoComplete.AI.CosmosDb` | Azure CosmosDB provider | < 10KB gzipped |

---

## Requirements

- **.NET 8.0**, **.NET 9.0**, or **.NET 10.0**
- **Blazor WebAssembly**, **Blazor Server**, or **Blazor Auto/United** render modes
- Modern browser with ES6 support

---

## Performance

- **Core bundle size:** < 15KB gzipped
- **Filter 100K items:** < 100ms
- **Virtualization:** 60fps scrolling with 100K+ items
- **First render:** < 50ms
- **SIMD acceleration:** 3-5x speedup for semantic similarity calculations

---

## Troubleshooting

### Common Issues

**Component not rendering / styles missing**
```html
<!-- Ensure CSS is added to App.razor or index.html -->
<link href="_content/EasyAppDev.Blazor.AutoComplete/styles/autocomplete.base.css" rel="stylesheet" />
```

**Dropdown not opening**
- Check that `Items` or `DataSource` is not null/empty
- Verify `MinSearchLength` - default is 1 character before filtering

**Items not filtering**
- Ensure `TextField` expression returns a non-null string
- Check `FilterStrategy` matches your use case (StartsWith vs Contains)

**Virtualization not working**
- Set `Virtualize="true"` explicitly
- Ensure `ItemHeight` matches your actual item height in pixels
- Check that item count exceeds `VirtualizationThreshold` (default: 100)

**OData requests failing**
- Verify `EndpointUrl` is correct and accessible
- Check CORS configuration on server
- Use browser DevTools Network tab to inspect generated OData queries

**Semantic search returning no results**
- Lower `SimilarityThreshold` (try 0.1 instead of 0.15)
- Ensure `MinSearchLength` is met (default: 3 for AI)
- Verify OpenAI/Azure API key is valid
- Check browser console for API errors

**AOT/Trimming build errors**
- Ensure you're using the latest package version
- The component is fully AOT-compatible; if issues persist, check for conflicts with other libraries

### Performance Tips

- Use `FilterStrategy.StartsWith` for fastest filtering
- Enable `Virtualize="true"` for datasets > 100 items
- Set appropriate `DebounceMs` (300-500ms) to reduce API calls
- For AI search, enable `PreWarmCache="true"` to pre-generate embeddings

---

## License

MIT License - see [LICENSE](LICENSE)

---

## Support

- [GitHub Issues](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/issues) - Bug reports and feature requests
- [GitHub Discussions](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/discussions) - Questions and community support

---

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting a pull request.

---

If you find this package useful, please consider giving it a star on GitHub!
