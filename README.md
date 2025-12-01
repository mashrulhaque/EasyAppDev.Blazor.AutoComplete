# Blazor AutoComplete

A high-performance AutoComplete component for Blazor with AI-powered semantic search.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**[Live Demo](https://blazorautocomplete.easyappdev.com/)** | [NuGet](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete/)

## Features

- **High Performance** - Virtualization for large datasets, debounced input
- **Native AOT Ready** - Source generators, zero reflection, fully trimmable
- **AI Semantic Search** - Optional package with OpenAI/Azure embeddings
- **Accessible** - WCAG 2.1 AA, ARIA 1.2 Combobox pattern, keyboard navigation
- **Theming** - 4 design presets (Material, Fluent, Modern, Bootstrap), CSS variables
- **8 Display Modes** - Built-in layouts eliminate template boilerplate
- **.NET 8/9** - WebAssembly, Server, Auto render modes

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete
```

## Setup

Add to your `App.razor` or `index.html`:

```html
<head>
    <link href="_content/EasyAppDev.Blazor.AutoComplete/styles/autocomplete.base.css" rel="stylesheet" />
    <script src="_content/EasyAppDev.Blazor.AutoComplete/scripts/theme-loader.js"></script>
</head>
```

**Optional:** Register services for singleton theme management:

```csharp
builder.Services.AddAutoComplete();
```

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

| Parameter | Default | Description |
|-----------|---------|-------------|
| `SimilarityThreshold` | `0.15` | Minimum cosine similarity (0-1) for results |
| `MinSearchLength` | `3` | Characters before semantic search triggers |
| `DebounceMs` | `500` | Delay before API call |
| `ItemCacheDuration` | `1 hour` | Embedding cache TTL for items |
| `QueryCacheDuration` | `15 min` | Embedding cache TTL for queries |
| `MaxItemCacheSize` | `10,000` | Maximum cached item embeddings |
| `MaxQueryCacheSize` | `1,000` | Maximum cached query embeddings |
| `PreWarmCache` | `false` | Generate all embeddings on init |
| `ShowCacheStatus` | `true` | Display cache statistics |

## API Reference

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | `null` | Collection of items |
| `DataSource` | `IAutoCompleteDataSource<TItem>?` | `null` | Async data source |
| `Value` | `TItem?` | `null` | Selected value (two-way) |
| `ValueChanged` | `EventCallback<TItem?>` | | Selection change event |
| `TextField` | `Expression<Func<TItem, string>>?` | `null` | Display text property |
| `SearchFields` | `Expression<Func<TItem, string[]>>?` | `null` | Multi-field search |
| `Placeholder` | `string?` | `null` | Input placeholder |
| `MinSearchLength` | `int` | `1` | Min chars before search |
| `MaxDisplayedItems` | `int` | `100` | Max items shown |
| `DebounceMs` | `int` | `300` | Debounce delay (ms) |
| `MaxSearchLength` | `int` | `500` | Max input length (security) |
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
| `ThemePreset` | `ThemePreset` | `None` | Design system |
| `BootstrapTheme` | `BootstrapTheme` | `Default` | Bootstrap color variant |
| `Size` | `ComponentSize` | `Default` | Component size |
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
- **ReDoS Protection** - Regex patterns use timeouts
- **API Key Redaction** - Sensitive data removed from error messages

## Packages

| Package | Description |
|---------|-------------|
| `EasyAppDev.Blazor.AutoComplete` | Core component |
| `EasyAppDev.Blazor.AutoComplete.Generators` | Source generators (build-time only) |
| `EasyAppDev.Blazor.AutoComplete.AI` | Semantic search with embeddings |

## Requirements

- .NET 8.0 or .NET 9.0
- Blazor WebAssembly, Server, or Auto

## License

MIT License - see [LICENSE](LICENSE)

## Support

- [GitHub Issues](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/issues)
- [GitHub Discussions](https://github.com/mashrulhaque/EasyAppDev.Blazor.AutoComplete/discussions)
