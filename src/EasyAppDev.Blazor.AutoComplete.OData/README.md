# EasyAppDev.Blazor.AutoComplete.OData

OData integration for the Blazor AutoComplete component. Query OData v3/v4 endpoints with automatic `$filter` generation, multi-field search, and all filter strategies.

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.OData
```

## Quick Start

```razor
@using EasyAppDev.Blazor.AutoComplete
@using EasyAppDev.Blazor.AutoComplete.OData
@inject HttpClient Http

<AutoComplete TItem="Product"
              DataSource="@_odataSource"
              TextField="@(p => p.Name)"
              Placeholder="Search products..." />

@code {
    private ODataDataSource<Product> _odataSource = null!;

    protected override void OnInitialized()
    {
        var options = new ODataOptions
        {
            EndpointUrl = "https://api.example.com/odata/products",
            FilterStrategy = ODataFilterStrategy.Contains,
            Top = 20,
            Select = new[] { "Id", "Name", "Description" }
        };

        _odataSource = new ODataDataSource<Product>(
            Http, options, searchFieldNames: new[] { "Name", "Description" });
    }
}
```

## Features

- **OData v3 + v4 Support** - Automatic syntax selection
- **StartsWith Filter** - `startswith(field,'value')`
- **Contains Filter** - `contains(field,'value')` (v4) / `substringof('value',field)` (v3)
- **Multi-Field Search** - OR-combined filters across multiple properties
- **Additional Filters** - Static filters ANDed with search
- **Fuzzy Fallback** - Client-side re-ranking for typo tolerance
- **Error Handling** - Events and `LastError` property for graceful error handling

## Configuration Options

| Option | Description |
|--------|-------------|
| `EndpointUrl` | OData endpoint URL |
| `Version` | V3 or V4 (default: V4) |
| `FilterStrategy` | StartsWith, Contains, FuzzyFallback |
| `Top` | Max results ($top) |
| `Select` | Fields to return ($select) |
| `OrderBy` | Sort order ($orderby) |
| `AdditionalFilter` | Extra filter conditions |
| `CaseInsensitive` | Use tolower() wrapper |

## Filter Strategy Comparison

| Strategy | OData v4 | OData v3 | Use Case |
|----------|----------|----------|----------|
| `StartsWith` | `startswith(field,'value')` | `startswith(field,'value')` | Fast prefix matching |
| `Contains` | `contains(field,'value')` | `substringof('value',field)` | Find anywhere in string |
| `FuzzyFallback` | `contains()` + client re-rank | `substringof()` + client re-rank | Typo tolerance |

## License

MIT
