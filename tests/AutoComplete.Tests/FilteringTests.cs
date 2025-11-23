using EasyAppDev.Blazor.AutoComplete.Filtering;
using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for filtering functionality
/// </summary>
public class FilteringTests : TestContext
{
    private readonly List<Product> _testProducts = new()
    {
        new Product { Id = 1, Name = "Apple iPhone", Category = "Electronics" },
        new Product { Id = 2, Name = "Samsung Galaxy", Category = "Electronics" },
        new Product { Id = 3, Name = "Apple MacBook", Category = "Computers" },
        new Product { Id = 4, Name = "Microsoft Surface", Category = "Computers" },
        new Product { Id = 5, Name = "Apple Watch", Category = "Wearables" }
    };

    [Fact]
    public void StartsWithFilter_FiltersCorrectly()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "Apple", p => p.Name);

        // Assert
        result.Should().HaveCount(3);
        result.All(p => p.Name.StartsWith("Apple", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public void StartsWithFilter_IsCaseInsensitive()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "apple", p => p.Name);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void ContainsFilter_FiltersCorrectly()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "Book", p => p.Name);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Apple MacBook");
    }

    [Fact]
    public void ContainsFilter_IsCaseInsensitive()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "GALAXY", p => p.Name);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Samsung Galaxy");
    }

    [Fact]
    public void FuzzyFilter_FiltersCorrectly()
    {
        // Arrange
        var filter = new FuzzyFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "Aple", p => p.Name); // Typo: Aple instead of Apple

        // Assert
        result.Should().NotBeEmpty();
        result.Any(p => p.Name.Contains("Apple")).Should().BeTrue();
    }

    [Fact]
    public void AutoComplete_UsesStartsWithFilterByDefault()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().HaveCount(3);
        });
    }

    [Fact]
    public void AutoComplete_UsesContainsFilterWhenConfigured()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Galaxy");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().HaveCount(1);
            items[0].TextContent.Should().Contain("Samsung Galaxy");
        });
    }

    [Fact]
    public void AutoComplete_UsesFuzzyFilterWhenConfigured()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Fuzzy)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Smsng"); // Fuzzy match for Samsung

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().NotBeEmpty();
        });
    }

    [Fact]
    public void AutoComplete_UsesCustomFilterWhenProvided()
    {
        // Arrange
        var customFilter = new CustomExactMatchFilter<Product>();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Custom)
            .Add(p => p.CustomFilter, customFilter)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple iPhone");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().HaveCount(1);
            items[0].TextContent.Should().Contain("Apple iPhone");
        });
    }

    [Fact]
    public void AutoComplete_FiltersEmptyString_ReturnsNoResults()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("");

        // Assert
        cut.FindAll(".ebd-ac-dropdown").Should().BeEmpty();
    }

    #region FilterEngineBase Tests

    [Fact]
    public void FilterEngineBase_WithNullSearchText_ReturnsAllItems()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, null!, p => p.Name);

        // Assert
        result.Should().HaveCount(_testProducts.Count);
    }

    [Fact]
    public void FilterEngineBase_WithEmptySearchText_ReturnsAllItems()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "", p => p.Name);

        // Assert
        result.Should().HaveCount(_testProducts.Count);
    }

    [Fact]
    public void FilterEngineBase_WithWhitespaceSearchText_ReturnsAllItems()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act
        var result = filter.Filter(_testProducts, "   ", p => p.Name);

        // Assert
        result.Should().HaveCount(_testProducts.Count);
    }

    [Fact]
    public void FilterEngineBase_ConvertsSearchTextToLowercase()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act - using uppercase search text
        var resultUpper = filter.Filter(_testProducts, "APPLE", p => p.Name);
        var resultLower = filter.Filter(_testProducts, "apple", p => p.Name);

        // Assert - should return same results
        resultUpper.Should().HaveCount(resultLower.Count());
        resultUpper.Should().BeEquivalentTo(resultLower);
    }

    [Fact]
    public void FilterEngineBase_HandlesNullItemText()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();
        var productsWithNull = new List<Product>
        {
            new Product { Id = 1, Name = "Apple", Category = null! },
            new Product { Id = 2, Name = "Samsung", Category = "Electronics" }
        };

        // Act
        var result = filter.Filter(productsWithNull, "Elec", p => p.Category);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    #endregion

    #region Multi-Field Filter Tests

    [Fact]
    public void StartsWithFilter_MultiField_MatchesAnyFieldPrefix()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple", Category = "Fruit" },
            new() { Id = 2, Name = "Banana", Category = "Fruit" },
            new() { Id = 3, Name = "Carrot", Category = "Vegetable" }
        };

        // Act
        var result = filter.FilterMultiField(
            products,
            "Veg",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().ContainSingle()
            .Which.Name.Should().Be("Carrot");
    }

    [Fact]
    public void StartsWithFilter_MultiField_MatchesNameField()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();

        // Act
        var result = filter.FilterMultiField(
            _testProducts,
            "App",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().HaveCount(3);
        result.All(p => p.Name.StartsWith("Apple", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public void ContainsFilter_MultiField_MatchesAnyFieldSubstring()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple iPhone", Category = "Electronics" },
            new() { Id = 2, Name = "Samsung Phone", Category = "Electronics" }
        };

        // Act
        var result = filter.FilterMultiField(
            products,
            "iPhone",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().ContainSingle()
            .Which.Name.Should().Contain("Apple");
    }

    [Fact]
    public void ContainsFilter_MultiField_MatchesCategory()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act
        var result = filter.FilterMultiField(
            _testProducts,
            "Comp",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Category == "Computers").Should().BeTrue();
    }

    [Fact]
    public void FuzzyFilter_MultiField_FindsMinimumDistance()
    {
        // Arrange
        var filter = new FuzzyFilter<Product>(maxDistance: 2);
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Mobile", Category = "Phone" },
            new() { Id = 2, Name = "Laptop", Category = "Computer" }
        };

        // Act
        var result = filter.FilterMultiField(
            products,
            "mobi",  // 1 char off from "Mobile"
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().ContainSingle()
            .Which.Name.Should().Be("Mobile");
    }

    [Fact]
    public void FuzzyFilter_MultiField_SortsByMinimumDistance()
    {
        // Arrange
        var filter = new FuzzyFilter<Product>(maxDistance: 3);
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Surface", Category = "Laptop" },     // "Laptop" exact match (distance 0)
            new() { Id = 2, Name = "MacBook", Category = "Computer" },   // No close match
            new() { Id = 3, Name = "Laptop Pro", Category = "Computer" } // "Laptop" exact match (distance 0)
        };

        // Act
        var result = filter.FilterMultiField(
            products,
            "Laptop",
            p => new[] { p.Name, p.Category });

        // Assert - items with exact match should come first
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].Id.Should().Be(1); // Surface has "Laptop" in category
        resultList[1].Id.Should().Be(3); // Laptop Pro has "Laptop" in name
    }

    [Fact]
    public void MultiField_WithNullFields_HandlesGracefully()
    {
        // Arrange
        var filter = new StartsWithFilter<Product>();
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Category = null! },
            new() { Id = 2, Name = null!, Category = "Electronics" }
        };

        // Act
        var result = filter.FilterMultiField(
            products,
            "Elec",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().ContainSingle()
            .Which.Id.Should().Be(2);
    }

    [Fact]
    public void MultiField_WithEmptySearchText_ReturnsAllItems()
    {
        // Arrange
        var filter = new ContainsFilter<Product>();

        // Act
        var result = filter.FilterMultiField(
            _testProducts,
            "",
            p => new[] { p.Name, p.Category });

        // Assert
        result.Should().HaveCount(_testProducts.Count);
    }

    #endregion

    #region Component Multi-Field Tests

    [Fact]
    public void AutoComplete_MultiFieldWithContains_FiltersCorrectly()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.SearchFields, p => new[] { p.Name, p.Category })
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - search for "Watch" which is in middle of "Apple Watch"
        input.Input("Watch");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
            items[0].TextContent.Should().Contain("Apple Watch");
        });
    }

    [Fact]
    public void AutoComplete_MultiFieldWithFuzzy_FiltersCorrectly()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.SearchFields, p => new[] { p.Name, p.Category })
            .Add(p => p.FilterStrategy, FilterStrategy.Fuzzy)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - search with typo "Aple" instead of "Apple"
        input.Input("Aple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().NotBeEmpty();
            items.Should().OnlyContain(item => item.TextContent.Contains("Apple"));
        });
    }

    [Fact]
    public void AutoComplete_MultiField_MatchesCategoryField()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.SearchFields, p => new[] { p.Name, p.Category })
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - search for category "Electronics"
        input.Input("Electron");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().HaveCount(2); // Apple iPhone and Samsung Galaxy
        });
    }

    #endregion
}

/// <summary>
/// Custom filter for testing purposes - only exact matches
/// </summary>
public class CustomExactMatchFilter<TItem> : IFilterEngine<TItem>
{
    public IEnumerable<TItem> Filter(IEnumerable<TItem> items, string searchText, Func<TItem, string> textSelector)
    {
        return items.Where(item => textSelector(item).Equals(searchText, StringComparison.OrdinalIgnoreCase));
    }
}
