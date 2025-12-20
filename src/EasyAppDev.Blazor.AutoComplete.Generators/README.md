# EasyAppDev.Blazor.AutoComplete.Generators

Source generators for the Blazor AutoComplete component. Enables AOT compilation and trimming compatibility.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.Generators.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.Generators/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EasyAppDev.Blazor.AutoComplete.Generators.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.Generators/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Zero Runtime Overhead**: All code generated at build time
- **AOT Compatible**: No reflection, fully trimmable
- **Build-Time Validation**: Compile-time diagnostics for invalid expressions
- **Automatic**: Installed as a dependency of the core package

## Installation

This package is automatically installed when you add the core AutoComplete package:

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete
```

## Source Generators

### 1. PropertyAccessorGenerator

Generates compiled property accessors at build time, eliminating the need for `Expression.Compile()` at runtime.

```csharp
// Your code
<AutoComplete TextField="@(p => p.Name)" />

// Generated at build time
public static string GetName(Product p) => p.Name;
```

### 2. ExpressionValidatorGenerator

Enforces trimming-safe expression patterns with compile-time diagnostics.

```csharp
// Valid: Simple property access
<AutoComplete TextField="@(p => p.Name)" />

// Invalid: Method call - triggers EBDAC001
<AutoComplete TextField="@(p => p.Name.ToUpper())" />

// Invalid: String interpolation - triggers EBDAC001
<AutoComplete TextField="@(p => $"{p.Name}")" />
```

### 3. ConfigurationApplierGenerator

Auto-generates the configuration application method with 100% parameter coverage.

## Diagnostic Codes

| Code | Description |
|------|-------------|
| **EBDAC001** | Invalid TextField Expression (must be simple property access) |
| **EBDAC002** | Invalid ValueField Expression |
| **EBDAC003** | Unsupported Expression Type (trimming incompatible) |

## AOT Publishing

```bash
dotnet publish -c Release /p:PublishAot=true
```

## Debugging Generators

To view generated code:

```
obj/Debug/net8.0/generated/
obj/Debug/net9.0/generated/
```

## Requirements

- Targets `netstandard2.0` (Roslyn analyzer requirement)
- Works with .NET 8.0, .NET 9.0, and .NET 10.0 projects

## License

MIT
