# Blazor AutoComplete

A high-performance AutoComplete component for Blazor with AI-powered semantic search.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**[Live Demo](https://blazorautocomplete.easyappdev.com/)** | [NuGet](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)

## Features

- **High Performance** - Virtualization for large datasets, debounced input
- **Native AOT Ready** - Source generators, zero reflection, fully trimmable
- **AI Semantic Search** - Optional package with OpenAI/Azure embeddings
- **OData Integration** - Optional package for OData v3/v4 server-side filtering
- **Accessible** - WCAG 2.1 AA, ARIA 1.2 Combobox pattern, keyboard navigation
- **Theming** - 4 design presets (Material, Fluent, Modern, Bootstrap), CSS variables
- **8 Display Modes** - Built-in layouts eliminate template boilerplate
- **.NET 8/9/10** - WebAssembly, Server, Auto render modes

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete
```

## Setup

**1. Register services** in `Program.cs`:

```csharp
using EasyAppDev.Blazor.AutoComplete.Extensions;

builder.Services.AddAutoComplete();
```

This registers:
- `IThemeManager` - Theme CSS generation (singleton)
- `IAutoCompleteServiceFactory` - Creates component services (singleton)

**2. Add styles** to your `App.razor` or `index.html`:

```html
<head>
    <link href="_content/EasyAppDev.Blazor.AutoComplete/styles/autocomplete.base.css" rel="stylesheet" />
    <script src="_content/EasyAppDev.Blazor.AutoComplete/scripts/theme-loader.js"></script>
</head>
```

> **Note:** The component works without `AddAutoComplete()` using fallback instances, but registering services enables proper DI, testability, and singleton behavior.

## Basic Usage

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
        new Product { Id = 1, Name = "Apple" },
        new Product { Id = 2, Name = "Banana" },
        new Product { Id = 3, Name = "Cherry" }
    };
    private Product? selectedProduct;
}
```

## Core Features

### Multi-Field Search

Search across multiple properties:

```razor
<AutoComplete TItem="Product"
              Items="@products"
              SearchFields="@(p => new[] { p.Name, p.Description, p.Category })"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct" />
```

### Filter Strategies

```razor
<AutoComplete FilterStrategy="FilterStrategy.StartsWith" ... />  <!-- Default, fastest -->
<AutoComplete FilterStrategy="FilterStrategy.Contains" ... />    <!-- Substring match -->
<AutoComplete FilterStrategy="FilterStrategy.Fuzzy" ... />       <!-- Typo-tolerant -->
<AutoComplete FilterStrategy="FilterStrategy.Custom" CustomFilter="@myFilter" ... />
```

### Display Modes

Built-in layouts eliminate custom template markup:

```razor
@using EasyAppDev.Blazor.AutoComplete.Options

<!-- Simple text only -->
<AutoComplete DisplayMode="ItemDisplayMode.Simple" ... />

<!-- Title + description -->
<AutoComplete DisplayMode="ItemDisplayMode.TitleWithDescription"
              DescriptionField="@(p => p.Category)" ... />

<!-- Title + badge -->
<AutoComplete DisplayMode="ItemDisplayMode.TitleWithBadge"
              BadgeField="@(p => $"${p.Price}")"
              BadgeClass="badge bg-success" ... />

<!-- Icon + title + description -->
<AutoComplete DisplayMode="ItemDisplayMode.IconTitleDescription"
              IconField="@(p => p.Emoji)"
              DescriptionField="@(p => p.Category)" ... />

<!-- Card with all fields -->
<AutoComplete DisplayMode="ItemDisplayMode.Card"
              IconField="@(p => p.Emoji)"
              SubtitleField="@(p => p.Category)"
              DescriptionField="@(p => p.Description)"
              BadgeField="@(p => $"${p.Price}")" ... />
```

**Available modes:** `Custom`, `Simple`, `TitleWithDescription`, `TitleWithBadge`, `TitleDescriptionBadge`, `IconWithTitle`, `IconTitleDescription`, `Card`

### Grouping

```razor
<AutoComplete TItem="Product"
              GroupBy="@(p => p.Category)"
              ... >
    <GroupTemplate Context="group">
        <strong>@group.Key</strong> <span class="badge">@group.Count()</span>
    </GroupTemplate>
</AutoComplete>
```

### Virtualization

For large datasets:

```razor
<AutoComplete Virtualize="true"
              VirtualizationThreshold="100"
              ItemHeight="40"
              ... />
```

### Async Data Source

```razor
<AutoComplete TItem="Product"
              DataSource="@dataSource"
              TextField="@(p => p.Name)"
              @bind-Value="@selectedProduct" />

@code {
    private IAutoCompleteDataSource<Product> dataSource = new RemoteDataSource<Product>(
        async (query, ct) => await httpClient.GetFromJsonAsync<List<Product>>($"/api/products?q={query}", ct)
    );
}
```

### Custom Templates

```razor
<AutoComplete TItem="Product" ... >
    <ItemTemplate Context="product">
        <div class="product-item">
            <strong>@product.Name</strong>
            <span>$@product.Price</span>
        </div>
    </ItemTemplate>
    <NoResultsTemplate>No products found</NoResultsTemplate>
    <LoadingTemplate>Searching...</LoadingTemplate>
    <HeaderTemplate>Select a product</HeaderTemplate>
    <FooterTemplate>@_filteredItems.Count results</FooterTemplate>
</AutoComplete>
```

### Fluent Configuration

```csharp
var config = AutoCompleteConfig<Product>.Create()
    .WithItems(products)
    .WithTextField(p => p.Name)
    .WithSearchFields(p => new[] { p.Name, p.Description })
    .WithDisplayMode(ItemDisplayMode.TitleWithDescription)
    .WithTitleAndDescription(p => p.Description)
    .WithFilterStrategy(FilterStrategy.Contains)
    .WithTheme(Theme.Auto)
    .WithDebounce(300)
    .Build();
```

```razor
<AutoComplete TItem="Product" Config="@config" />
```

## Theming

### Theme Presets

```razor
<AutoComplete ThemePreset="ThemePreset.Material" ... />  <!-- Google Material Design -->
<AutoComplete ThemePreset="ThemePreset.Fluent" ... />    <!-- Microsoft Fluent -->
<AutoComplete ThemePreset="ThemePreset.Modern" ... />    <!-- Minimal/flat -->
<AutoComplete ThemePreset="ThemePreset.Bootstrap" ... /> <!-- Bootstrap 5 -->
```

### Light/Dark Mode

```razor
<AutoComplete Theme="Theme.Auto" ... />   <!-- System preference -->
<AutoComplete Theme="Theme.Light" ... />
<AutoComplete Theme="Theme.Dark" ... />
```

### Bootstrap Color Variants

```razor
<AutoComplete BootstrapTheme="BootstrapTheme.Primary" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Success" ... />
<AutoComplete BootstrapTheme="BootstrapTheme.Danger" ... />
```

**Available:** `Default`, `Primary`, `Secondary`, `Success`, `Danger`, `Warning`, `Info`, `Light`, `Dark`

### Component Sizes

```razor
<AutoComplete Size="ComponentSize.Compact" ... />
<AutoComplete Size="ComponentSize.Default" ... />
<AutoComplete Size="ComponentSize.Large" ... />
```

### Custom Theme Properties

Override individual properties without writing CSS:

```razor
<AutoComplete PrimaryColor="#FF6B6B"
              BackgroundColor="#FFFFFF"
              TextColor="#212529"
              BorderColor="#ced4da"
              BorderRadius="8px"
              FontFamily="Inter, sans-serif"
              FontSize="14px"
              DropdownShadow="0 4px 6px rgba(0,0,0,0.1)"
              ... />
```

Or use structured `ThemeOverrides`:

```razor
<AutoComplete ThemeOverrides="@(new ThemeOptions {
    Colors = new ColorOptions { Primary = "#FF6B6B", Hover = "#f8f9fa" },
    Spacing = new SpacingOptions { BorderRadius = "8px", InputPadding = "12px 16px" },
    Typography = new TypographyOptions { FontFamily = "Inter, sans-serif" }
})" ... />
```

## Accessibility

- **Keyboard:** Arrow keys navigate, Enter selects, Escape closes, Home/End jump
- **ARIA:** `role="combobox"`, `aria-expanded`, `aria-activedescendant`, `aria-selected`
- **Screen Readers:** Live region announcements for loading/results
- **Form Integration:** Works with `EditContext` validation

```razor
<label for="search">Search:</label>
<AutoComplete InputId="search" AriaLabel="Search products" ... />
```

## OData Integration

Optional package for querying OData v3/v4 endpoints with automatic `$filter` generation.

### Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.OData
```

### Usage

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
            EndpointUrl = "https://api.example.com/odata/products",
            FilterStrategy = ODataFilterStrategy.StartsWith,
            Top = 20
        };
        _odataSource = new ODataDataSource<Product>(Http, options, "Name");
    }
}
```

### Multi-Field Search

```csharp
// Search across multiple fields (combined with OR)
_odataSource = new ODataDataSource<Product>(
    Http,
    options,
    searchFieldNames: new[] { "Name", "Description", "Category" });
```

Generated OData: `$filter=(startswith(tolower(Name),'search') or startswith(tolower(Description),'search') or startswith(tolower(Category),'search'))`

### OData v3 Support

```csharp
var options = new ODataOptions
{
    EndpointUrl = "https://legacy-api.example.com/odata/products",
    Version = ODataVersion.V3,  // Use v3 syntax
    FilterStrategy = ODataFilterStrategy.Contains
};
```

### Filter Strategy Mapping

| Strategy | OData v4 | OData v3 |
|----------|----------|----------|
| `StartsWith` | `startswith(field,'value')` | Same |
| `Contains` | `contains(field,'value')` | `substringof('value',field)` |
| `FuzzyFallback` | `contains()` + client re-rank | `substringof()` + client re-rank |

### OData Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `EndpointUrl` | `string` | Required | OData endpoint URL |
| `Version` | `ODataVersion` | `V4` | OData protocol version |
| `FilterStrategy` | `ODataFilterStrategy` | `StartsWith` | Filter type |
| `Top` | `int` | `100` | Max results ($top) |
| `Select` | `string[]?` | `null` | Fields to return ($select) |
| `OrderBy` | `string?` | `null` | Sort order ($orderby) |
| `AdditionalFilter` | `string?` | `null` | Static filter ANDed with search |
| `CaseInsensitive` | `bool` | `true` | Use tolower() wrapper |
| `MinSearchLength` | `int` | `1` | Min chars before API call |
| `TimeoutSeconds` | `int` | `30` | HTTP request timeout |
| `CustomHeaders` | `Dictionary<string,string>?` | `null` | HTTP headers (e.g., Authorization) |
| `ResultsPropertyName` | `string` | `"value"` | JSON property containing results |
| `IncludeCount` | `bool` | `false` | Include $count in response |

### Fluent Builder

```csharp
var config = AutoCompleteConfig<Product>.Create()
    .WithODataSource(Http, "https://api.example.com/odata/products", "Name",
        opts => {
            opts.FilterStrategy = ODataFilterStrategy.Contains;
            opts.Top = 20;
        })
    .WithDisplayMode(ItemDisplayMode.TitleWithDescription)
    .Build();
```

### Service Registration

```csharp
// Configuration-based
builder.Services.AddAutoCompleteOData(builder.Configuration, "ODataSettings");

// Explicit configuration
builder.Services.AddAutoCompleteOData(
    "https://api.example.com/odata/products",
    options => {
        options.FilterStrategy = ODataFilterStrategy.Contains;
        options.Top = 50;
    });
```

## AI Semantic Search

Optional package for meaning-based search using embeddings.

### Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI
```

### Setup

```csharp
// OpenAI
builder.Services.AddAutoCompleteSemanticSearch(builder.Configuration);

// Or with explicit key
builder.Services.AddAutoCompleteSemanticSearch(apiKey: "sk-...", model: "text-embedding-3-small");

// Azure OpenAI
builder.Services.AddAutoCompleteSemanticSearchWithAzure(
    endpoint: "https://my-resource.openai.azure.com/",
    apiKey: "...",
    deploymentName: "text-embedding-ada-002");
```

Configuration in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "text-embedding-3-small"
  }
}
```

### Usage

```razor
@using EasyAppDev.Blazor.AutoComplete.AI

<SemanticAutoComplete TItem="Document"
                      Items="@documents"
                      SearchFields="@(d => new[] { d.Title, d.Description, d.Tags })"
                      TextField="@(d => d.Title)"
                      SimilarityThreshold="0.15"
                      @bind-Value="@selectedDoc"
                      Placeholder="Search by meaning..." />
```

### AI Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SimilarityThreshold` | `float` | `0.15` | Minimum cosine similarity (0-1) for results |
| `MinSearchLength` | `int` | `3` | Characters before semantic search triggers |
| `DebounceMs` | `int` | `500` | Delay before API call |
| `MaxResults` | `int?` | `null` | Maximum results (null = all matching) |
| `ItemCacheDuration` | `TimeSpan` | `1 hour` | Embedding cache TTL for items |
| `QueryCacheDuration` | `TimeSpan` | `15 min` | Embedding cache TTL for queries |
| `MaxItemCacheSize` | `int` | `10,000` | Maximum cached item embeddings |
| `MaxQueryCacheSize` | `int` | `1,000` | Maximum cached query embeddings |
| `PreWarmCache` | `bool` | `false` | Generate all embeddings on init |
| `ShowCacheStatus` | `bool` | `true` | Display cache statistics |

## Vector Database Providers

For production deployments with persistent storage and scalable semantic search, use external vector database providers. These eliminate the need to regenerate embeddings on restart and support millions of items.

### Supported Providers

| Provider | Package | Features |
|----------|---------|----------|
| PostgreSQL (pgvector) | `EasyAppDev.Blazor.AutoComplete.AI.PostgreSql` | Self-hosted, 6 distance functions (Cosine, L2, DotProduct, Manhattan, Hamming, Jaccard), HNSW index |
| Azure AI Search | `EasyAppDev.Blazor.AutoComplete.AI.AzureSearch` | Hybrid search (vector + keyword), semantic ranking, managed service |
| Pinecone | `EasyAppDev.Blazor.AutoComplete.AI.Pinecone` | Serverless, namespaces, automatic scaling |
| Qdrant | `EasyAppDev.Blazor.AutoComplete.AI.Qdrant` | Open-source, self-hosted, advanced filtering |
| Azure CosmosDB | `EasyAppDev.Blazor.AutoComplete.AI.CosmosDb` | Multi-model, global distribution, integrated NoSQL |

### Quick Start (PostgreSQL)

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.PostgreSql
```

```csharp
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Extensions;

// Configure services
builder.Services.AddAutoCompletePostgres<Product>(
    configureOptions: options =>
    {
        options.ConnectionString = "Host=localhost;Database=myapp;Username=user;Password=pass";
        options.CollectionName = "products";
        options.EmbeddingDimensions = 1536;  // text-embedding-3-small
    },
    textSelector: p => $"{p.Name} {p.Description} {p.Category}",
    idSelector: p => p.Id.ToString());

// Add OpenAI embeddings and vector search data source
builder.Services.AddAutoCompleteVectorSearch<Product>(
    openAiApiKey: "sk-...",
    configureOptions: options =>
    {
        options.MaxResults = 20;
        options.MinSimilarityScore = 0.15f;
    });
```

Component usage is unchanged:

```razor
<SemanticAutoComplete TItem="Product"
                      TextField="@(p => p.Name)"
                      @bind-Value="selectedProduct" />
```

### Quick Start (Azure AI Search)

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.AzureSearch
```

```csharp
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Extensions;

builder.Services.AddAutoCompleteAzureSearch<Product>(
    configureOptions: options =>
    {
        options.Endpoint = "https://my-search.search.windows.net";
        options.ApiKey = "your-api-key";
        options.IndexName = "products";
        options.EnableHybridSearch = true;  // Vector + keyword search
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());

builder.Services.AddAutoCompleteVectorSearchWithAzure<Product>(
    endpoint: "https://my-openai.openai.azure.com/",
    apiKey: "your-openai-key",
    deploymentName: "text-embedding-ada-002",
    configureOptions: options =>
    {
        options.EnableHybridSearch = true;
    });
```

### When to Use Vector Providers

| Scenario | Recommendation |
|----------|----------------|
| Development/Prototyping | Use in-memory `SemanticSearchDataSource` |
| Small datasets (< 10K items) | Either approach works |
| Production (> 10K items) | Use vector provider |
| Need persistence across restarts | Use vector provider |
| Multi-instance deployment | Use vector provider (shared database) |
| Need hybrid search (vector + keyword) | Azure AI Search or CosmosDB |

### Indexing Data

Before searching, index your data:

```csharp
// Inject the indexer
public class ProductService
{
    private readonly IVectorIndexer<Product> _indexer;

    public async Task IndexProductsAsync(IEnumerable<Product> products)
    {
        // Ensure collection/index exists
        await _indexer.EnsureCollectionExistsAsync();

        // Index all items (with progress reporting)
        _indexer.ProgressChanged += (s, e) =>
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

See the [Migration Guide](docs/migration-guide.md) for detailed instructions on migrating from in-memory search to vector providers.

## API Reference

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | `null` | Collection of items |
| `DataSource` | `IAutoCompleteDataSource<TItem>?` | `null` | Async data source (takes precedence over Items) |
| `Value` | `TItem?` | `null` | Selected value (two-way) |
| `ValueChanged` | `EventCallback<TItem?>` | | Selection change event |
| `TextField` | `Expression<Func<TItem, string>>?` | `null` | Display text property |
| `SearchFields` | `Expression<Func<TItem, string[]>>?` | `null` | Multi-field search |
| `Placeholder` | `string?` | `null` | Input placeholder |
| `MinSearchLength` | `int` | `1` | Min chars before search |
| `MaxSearchLength` | `int` | `500` | Max input length (security, max 2000) |
| `MaxDisplayedItems` | `int` | `100` | Max items shown |
| `DebounceMs` | `int` | `300` | Debounce delay (ms) |
| `AllowClear` | `bool` | `true` | Show clear button |
| `Disabled` | `bool` | `false` | Disable component |
| `CloseOnSelect` | `bool` | `true` | Close on selection |

### Filtering

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `FilterStrategy` | `FilterStrategy` | `StartsWith` | Filter algorithm |
| `CustomFilter` | `IFilterEngine<TItem>?` | `null` | Custom filter implementation |

### Display Modes

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DisplayMode` | `ItemDisplayMode` | `Custom` | Built-in display layout |
| `DescriptionField` | `Expression<Func<TItem, string>>?` | `null` | Description property |
| `BadgeField` | `Expression<Func<TItem, string>>?` | `null` | Badge property |
| `IconField` | `Expression<Func<TItem, string>>?` | `null` | Icon/emoji property |
| `SubtitleField` | `Expression<Func<TItem, string>>?` | `null` | Subtitle (Card mode) |
| `BadgeClass` | `string` | `"badge bg-primary"` | Badge CSS class |

### Theming

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Theme` | `Theme` | `Auto` | Light/Dark/Auto |
| `ThemePreset` | `ThemePreset` | `None` | Design system (Material, Fluent, Modern, Bootstrap) |
| `BootstrapTheme` | `BootstrapTheme` | `Default` | Bootstrap color variant |
| `Size` | `ComponentSize` | `Default` | Component size (Compact, Default, Large) |
| `EnableThemeTransitions` | `bool` | `true` | Smooth transitions |
| `RightToLeft` | `bool` | `false` | RTL text direction |
| `ThemeOverrides` | `ThemeOptions?` | `null` | Structured overrides |
| `PrimaryColor` | `string?` | `null` | Primary color override |
| `BackgroundColor` | `string?` | `null` | Background override |
| `TextColor` | `string?` | `null` | Text color override |
| `BorderColor` | `string?` | `null` | Border color override |
| `HoverColor` | `string?` | `null` | Hover color override |
| `SelectedColor` | `string?` | `null` | Selected color override |
| `BorderRadius` | `string?` | `null` | Border radius override |
| `FontFamily` | `string?` | `null` | Font family override |
| `FontSize` | `string?` | `null` | Font size override |
| `DropdownShadow` | `string?` | `null` | Shadow override |

### Virtualization

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Virtualize` | `bool` | `false` | Enable virtualization |
| `VirtualizationThreshold` | `int` | `100` | Item count threshold |
| `ItemHeight` | `float` | `40` | Item height (px) |

### Grouping

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `GroupBy` | `Expression<Func<TItem, object>>?` | `null` | Grouping property |
| `GroupTemplate` | `RenderFragment<IGrouping<object, TItem>>?` | `null` | Group header template |

### Templates

| Parameter | Type | Description |
|-----------|------|-------------|
| `ItemTemplate` | `RenderFragment<TItem>?` | Custom item rendering |
| `NoResultsTemplate` | `RenderFragment?` | No results message |
| `LoadingTemplate` | `RenderFragment?` | Loading indicator |
| `HeaderTemplate` | `RenderFragment?` | Dropdown header |
| `FooterTemplate` | `RenderFragment?` | Dropdown footer |

### Accessibility & Forms

| Parameter | Type | Description |
|-----------|------|-------------|
| `AriaLabel` | `string?` | ARIA label |
| `InputId` | `string?` | Input element ID for label association |
| `ValueExpression` | `Expression<Func<TItem?>>?` | Validation expression |
| `Config` | `AutoCompleteConfig<TItem>?` | Fluent configuration object |

## Security

- **CSS Sanitization** - Theme values validated against allowlists
- **Input Limits** - `MaxSearchLength` prevents memory exhaustion (default 500, max 2000)
- **ReDoS Protection** - Regex patterns use 100ms timeouts
- **API Key Redaction** - Sensitive data removed from error messages
- **Centralized Constants** - All limits defined in `AutoCompleteConstants` for consistency

## Packages

| Package | Description |
|---------|-------------|
| `EasyAppDev.Blazor.AutoComplete` | Core component |
| `EasyAppDev.Blazor.AutoComplete.Generators` | Source generators (build-time only) |
| `EasyAppDev.Blazor.AutoComplete.AI` | Semantic search with embeddings |
| `EasyAppDev.Blazor.AutoComplete.OData` | OData v3/v4 server-side filtering |

## Requirements

- .NET 8.0, .NET 9.0, or .NET 10.0
- Blazor WebAssembly, Server, or Auto

## License

MIT License - see [LICENSE](LICENSE)

## Support

- [GitHub Issues](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/issues)
- [GitHub Discussions](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/discussions)
