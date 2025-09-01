---
title: Query Filters
description: Powerful mechanisms for filtering and sorting entity queries dynamically
---

# Query Filters

The `EntityFilter` and `EntitySort` classes provide powerful mechanisms for filtering and sorting entity queries dynamically. These components enable complex filtering and sorting operations on entity collections with support for nested logical operations, operator mapping, and dynamic LINQ expression generation.

## Overview

The filtering and sorting system consists of several key components:

- [`EntityFilter`](#entityfilter) - Represents filtering criteria with support for nested filters and logical operators
- [`EntitySort`](#entitysort) - Represents sorting criteria with field name and direction
- [`EntityFilterOperators`](#filter-operators) - Provides constants for filter operators
- [`EntityFilterLogic`](#filter-logic) - Provides constants for logical operators
- [`EntitySortDirections`](#sort-directions) - Provides constants for sort directions
- [`EntityFilterBuilder`](#entityfilterbuilder) - Helper methods for building common filters
- [`LinqExpressionBuilder`](#linq-expression-builder) - Converts filters to dynamic LINQ expressions

## EntityFilter

The `EntityFilter` class represents a filter for selecting entities based on specific criteria. It supports both simple field-based filters and complex nested filter groups with logical operators.

### Class Definition

```csharp
[JsonConverter(typeof(EntityFilterConverter))]
public class EntityFilter
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("logic")]
    public string? Logic { get; set; }

    [JsonPropertyName("filters")]
    public IList<EntityFilter>? Filters { get; set; }

    public bool IsValid() => Filters?.Any(f => f.IsValid()) == true || Name is not null;
    public override int GetHashCode() => HashCode.Combine(Name, Operator, Value, Logic, Filters);
}
```

### Properties

#### Name

The name of the field or property to filter on. This should match the property name of the entity being queried.

#### Operator

The operator to use for the filter. Supported operators include:

- `eq` (equals) / `==`
- `ne` (not equals) / `!=`
- `gt` (greater than) / `>`
- `lt` (less than) / `<`
- `ge` (greater than or equal) / `>=`
- `le` (less than or equal) / `<=`
- `contains` - String contains operation
- `startswith` - String starts with operation
- `endswith` - String ends with operation
- `in` - Value in collection operation
- `isnull` - Null check operation
- `isnotnull` - Not null check operation
- `expression` - Custom LINQ expression

#### Value

The value to filter for. Can be any object type including primitives, strings, arrays, and collections.

#### Logic

The logical operator to combine nested filters. Uses constants from `EntityFilterLogic`:

- `and` - All nested filters must match
- `or` - Any nested filter must match

#### Filters

A list of nested filters for complex filter groups. When this property is set, the filter acts as a group filter using the specified `Logic` operator.

### Basic Filter Examples

#### Simple Equality Filter

```csharp
var filter = new EntityFilter
{
    Name = "Status",
    Operator = EntityFilterOperators.Equal,
    Value = "Active"
};
```

#### String Operations

```csharp
// Contains operation
var containsFilter = new EntityFilter
{
    Name = "Name",
    Operator = EntityFilterOperators.Contains,
    Value = "berry"
};

// StartsWith operation
var startsWithFilter = new EntityFilter
{
    Name = "Name",
    Operator = EntityFilterOperators.StartsWith,
    Value = "P"
};

// EndsWith operation
var endsWithFilter = new EntityFilter
{
    Name = "Name",
    Operator = EntityFilterOperators.EndsWith,
    Value = "berry"
};
```

#### Comparison Operations

```csharp
// Numeric comparison
var greaterThanFilter = new EntityFilter
{
    Name = "Rank",
    Operator = EntityFilterOperators.GreaterThan,
    Value = 5
};

// Date comparison
var dateFilter = new EntityFilter
{
    Name = "CreatedDate",
    Operator = EntityFilterOperators.GreaterThanOrEqual,
    Value = DateTime.Today
};
```

#### Collection Operations

```csharp
// In operation
var inFilter = new EntityFilter
{
    Name = "Category",
    Operator = EntityFilterOperators.In,
    Value = new[] { "Fruit", "Vegetable", "Grain" }
};
```

#### Null Checks

```csharp
// Null check
var nullFilter = new EntityFilter
{
    Name = "Description",
    Operator = EntityFilterOperators.IsNull
};

// Not null check
var notNullFilter = new EntityFilter
{
    Name = "Description",
    Operator = EntityFilterOperators.IsNotNull
};
```

### Complex Filter Examples

#### Logical OR Group

```csharp
var orFilter = new EntityFilter
{
    Logic = EntityFilterLogic.Or,
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Value = 7 },
        new EntityFilter { Name = "Name", Value = "Apple" }
    }
};
```

#### Logical AND Group

```csharp
var andFilter = new EntityFilter
{
    Filters = new List<EntityFilter> // Default logic is EntityFilterLogic.And
    {
        new EntityFilter { Name = "Rank", Value = 7 },
        new EntityFilter { Name = "Name", Value = "Blueberry" }
    }
};
```

#### Nested Complex Filter

```csharp
var complexFilter = new EntityFilter
{
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Operator = EntityFilterOperators.GreaterThan, Value = 5 },
        new EntityFilter
        {
            Logic = EntityFilterLogic.Or,
            Filters = new List<EntityFilter>
            {
                new EntityFilter { Name = "Name", Value = "Strawberry" },
                new EntityFilter { Name = "Name", Value = "Blueberry" }
            }
        }
    }
};
```

#### Custom Expression Filter

```csharp
var expressionFilter = new EntityFilter
{
    Name = "Locations.Any(it.Id in @0)",
    Operator = EntityFilterOperators.Expression,
    Value = new[] { 100, 200 }
};
```

### Using Filters with Extensions

The `EntityFilter` can be applied to `IQueryable<T>` collections using the `Filter` extension method:

```csharp
var fruits = Fruit.Data().AsQueryable();

// Apply a simple filter
var filtered = fruits.Filter(new EntityFilter 
{ 
    Name = "Name", 
    Operator = EntityFilterOperators.Contains, 
    Value = "berry" 
});

// Apply a complex filter
var complexFiltered = fruits.Filter(new EntityFilter
{
    Logic = EntityFilterLogic.And,
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Operator = EntityFilterOperators.GreaterThan, Value = 5 },
        new EntityFilter { Name = "IsActive", Value = true }
    }
});
```

## EntitySort

The `EntitySort` class represents a sort expression for an entity, specifying the property to sort by and the sort direction.

### EntitySort Class Definition

```csharp
[JsonConverter(typeof(EntitySortConverter))]
public class EntitySort
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("direction")]
    public string? Direction { get; set; }

    public static EntitySort? Parse(string? sort);
    public override string ToString();
    public override int GetHashCode() => HashCode.Combine(Name, Direction);
}
```

### EntitySort Properties

#### Name

The name of the field or property to sort by. This should match the property name of the entity being queried.

#### Direction

The sort direction. Uses constants from `EntitySortDirections`:

- `asc` - Ascending order (default)
- `desc` - Descending order

### Sort Examples

#### Basic Sort

```csharp
var sort = new EntitySort
{
    Name = "Name",
    Direction = EntitySortDirections.Ascending
};
```

#### Parse from String

```csharp
// Parse "name asc" format
var sort1 = EntitySort.Parse("Name asc");
var sort2 = EntitySort.Parse("Rank desc");
var sort3 = EntitySort.Parse("Priority"); // Default to ascending
```

#### Multiple Sorts

```csharp
var sorts = new List<EntitySort>
{
    new EntitySort { Name = "Priority", Direction = EntitySortDirections.Descending },
    new EntitySort { Name = "Name", Direction = EntitySortDirections.Ascending }
};
```

### Using Sorts with Extensions

The `EntitySort` can be applied to `IQueryable<T>` collections using the `Sort` extension methods:

```csharp
var fruits = Fruit.Data().AsQueryable();

// Apply a single sort
var sorted = fruits.Sort(new EntitySort 
{ 
    Name = "Name", 
    Direction = EntitySortDirections.Ascending 
});

// Apply multiple sorts
var multipleSorted = fruits.Sort(new List<EntitySort>
{
    new EntitySort { Name = "Priority", Direction = EntitySortDirections.Descending },
    new EntitySort { Name = "Name", Direction = EntitySortDirections.Ascending }
});
```

## Filter Operators

The `EntityFilterOperators` class provides constants for all supported filter operators:

```csharp
public static class EntityFilterOperators
{
    public const string Equal = "eq";
    public const string NotEqual = "ne";
    public const string LessThan = "lt";
    public const string LessThanOrEqual = "le";
    public const string GreaterThan = "gt";
    public const string GreaterThanOrEqual = "ge";
    public const string StartsWith = "startswith";
    public const string EndsWith = "endswith";
    public const string Contains = "contains";
    public const string IsNull = "isnull";
    public const string IsNotNull = "isnotnull";
    public const string In = "in";
    public const string Expression = "expression";
}
```

### Operator Examples

```csharp
// Using operator constants
var filter = new EntityFilter
{
    Name = "Status",
    Operator = EntityFilterOperators.Equal,
    Value = "Active"
};

var containsFilter = new EntityFilter
{
    Name = "Description",
    Operator = EntityFilterOperators.Contains,
    Value = "important"
};
```

## Filter Logic

The `EntityFilterLogic` class provides constants for logical operators used in filter groups:

```csharp
public static class EntityFilterLogic
{
    public const string And = "and";
    public const string Or = "or";
}
```

### Logic Examples

```csharp
// Using logic constants
var orGroup = new EntityFilter
{
    Logic = EntityFilterLogic.Or,
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Status", Value = "Active" },
        new EntityFilter { Name = "Status", Value = "Pending" }
    }
};
```

## Sort Directions

The `EntitySortDirections` class provides constants for sort directions:

```csharp
public static class EntitySortDirections
{
    public const string Ascending = "asc";
    public const string Descending = "desc";
}
```

### Direction Examples

```csharp
// Using direction constants
var sort = new EntitySort
{
    Name = "CreatedDate",
    Direction = EntitySortDirections.Descending
};
```

## EntityFilterBuilder

The `EntityFilterBuilder` class provides static helper methods for building common entity filters and queries:

### Common Methods

```csharp
public static class EntityFilterBuilder
{
    // Create a simple filter
    public static EntityFilter CreateFilter(string field, object? value, string? @operator = null);
    
    // Create filter groups
    public static EntityFilter? CreateGroup(string logic, params IEnumerable<EntityFilter?> filters);
    public static EntityFilter? CreateAndGroup(params EntityFilter?[] filters);
    public static EntityFilter? CreateOrGroup(params EntityFilter?[] filters);
    
    // Create search filters
    public static EntityFilter? CreateSearchFilter<TModel>(string searchText) where TModel : class, ISupportSearch;
    public static EntityFilter? CreateSearchFilter(IEnumerable<string> fields, string searchText);
    
    // Create search queries
    public static EntityQuery? CreateSearchQuery<TModel>(string searchText, int page = 1, int pageSize = 20) where TModel : class, ISupportSearch;
    
    // Create sort expressions
    public static EntitySort? CreateSort<TModel>() where TModel : class, ISupportSearch;
}
```

### Builder Examples

```csharp
// Create a simple filter
var statusFilter = EntityFilterBuilder.CreateFilter("Status", "Active", EntityFilterOperators.Equal);

// Create an AND group
var andGroup = EntityFilterBuilder.CreateAndGroup(
    EntityFilterBuilder.CreateFilter("Status", "Active"),
    EntityFilterBuilder.CreateFilter("Priority", 1, EntityFilterOperators.GreaterThan)
);

// Create an OR group
var orGroup = EntityFilterBuilder.CreateOrGroup(
    EntityFilterBuilder.CreateFilter("Category", "Fruit"),
    EntityFilterBuilder.CreateFilter("Category", "Vegetable")
);

// Create search filter for specific fields
var searchFilter = EntityFilterBuilder.CreateSearchFilter(
    new[] { "Name", "Description" }, 
    "apple"
);

// Create search query with pagination
var searchQuery = EntityFilterBuilder.CreateSearchQuery<Product>("laptop", 1, 20);
```

## LINQ Expression Builder

The `LinqExpressionBuilder` class converts `EntityFilter` instances into dynamic LINQ expressions that can be applied to `IQueryable<T>` collections.

### LinqExpressionBuilder Class Definition

```csharp
public class LinqExpressionBuilder
{
    public IReadOnlyList<object?> Parameters { get; }
    public string Expression { get; }
    
    public void Build(EntityFilter? entityFilter);
}
```

### How It Works

The builder converts filter structures into parameterized LINQ expressions:

1. **Simple Filters**: Convert to property comparisons
2. **String Operations**: Convert to method calls (Contains, StartsWith, EndsWith)
3. **Collection Operations**: Convert to `in` expressions
4. **Null Checks**: Convert to null comparisons
5. **Group Filters**: Convert to nested expressions with logical operators
6. **Custom Expressions**: Pass through as-is with parameter replacement

### Expression Examples

```csharp
var builder = new LinqExpressionBuilder();

// Simple equality: Name == @0
var simpleFilter = new EntityFilter { Name = "Name", Value = "Apple" };
builder.Build(simpleFilter);
// Result: "Name == @0", Parameters: ["Apple"]

// String contains: Name.Contains(@0)
var containsFilter = new EntityFilter { Name = "Name", Operator = EntityFilterOperators.Contains, Value = "berry" };
builder.Build(containsFilter);
// Result: "Name.Contains(@0)", Parameters: ["berry"]

// Complex group: (Rank > @0 and (Name == @1 or Name == @2))
var complexFilter = new EntityFilter
{
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Operator = EntityFilterOperators.GreaterThan, Value = 5 },
        new EntityFilter
        {
            Logic = EntityFilterLogic.Or,
            Filters = new List<EntityFilter>
            {
                new EntityFilter { Name = "Name", Value = "Strawberry" },
                new EntityFilter { Name = "Name", Value = "Blueberry" }
            }
        }
    }
};
builder.Build(complexFilter);
// Result: "(Rank > @0 and (Name == @1 or Name == @2))", Parameters: [5, "Strawberry", "Blueberry"]
```

## Integration with Queries

The filtering and sorting components integrate seamlessly with the entity query system:

### With EntitySelectQuery

```csharp
public class ProductSelectQuery : EntitySelectQuery<ProductReadModel>
{
    public ProductSelectQuery()
    {
        // Configure base select parameters
        Select = new EntitySelect
        {
            Filter = EntityFilterBuilder.CreateAndGroup(
                EntityFilterBuilder.CreateFilter("IsActive", true),
                EntityFilterBuilder.CreateFilter("CategoryId", 1, EntityFilterOperators.NotEqual)
            ),
            Sort = new List<EntitySort>
            {
                new EntitySort { Name = "Priority", Direction = EntitySortDirections.Descending },
                new EntitySort { Name = "Name", Direction = EntitySortDirections.Ascending }
            }
        };
    }
}
```

### With EntityPagedQuery

```csharp
public class ProductPagedQuery : EntityPagedQuery<ProductReadModel>
{
    public ProductPagedQuery(int page = 1, int pageSize = 20)
    {
        Query = new EntityQuery
        {
            Filter = EntityFilterBuilder.CreateFilter("IsPublished", true),
            Sort = new List<EntitySort>
            {
                new EntitySort { Name = "PublishedDate", Direction = EntitySortDirections.Descending }
            },
            Page = page,
            PageSize = pageSize
        };
    }
}
```

### Dynamic Filtering

```csharp
public class DynamicProductQuery : EntitySelectQuery<ProductReadModel>
{
    public DynamicProductQuery(string? category = null, string? searchText = null)
    {
        var filters = new List<EntityFilter>();
        
        // Add category filter if specified
        if (!string.IsNullOrEmpty(category))
        {
            filters.Add(EntityFilterBuilder.CreateFilter("Category", category));
        }
        
        // Add search filter if specified
        if (!string.IsNullOrEmpty(searchText))
        {
            var searchFilter = EntityFilterBuilder.CreateSearchFilter(
                new[] { "Name", "Description" }, 
                searchText
            );
            if (searchFilter != null)
                filters.Add(searchFilter);
        }
        
        // Combine filters with AND logic
        if (filters.Count > 0)
        {
            Select = new EntitySelect
            {
                Filter = filters.Count == 1 
                    ? filters[0] 
                    : EntityFilterBuilder.CreateAndGroup(filters.ToArray())
            };
        }
    }
}
```

## Best Practices

### Performance Considerations

1. **Index Properties**: Ensure filtered properties have appropriate database indexes
2. **Limit Filter Depth**: Avoid deeply nested filter groups for better performance
3. **Use Specific Operators**: Use the most specific operator for your use case
4. **Consider Caching**: Cache commonly used filter expressions

### Security Considerations

1. **Validate Field Names**: Ensure filter field names match valid entity properties
2. **Sanitize Values**: Validate and sanitize filter values from user input
3. **Limit Filter Complexity**: Set reasonable limits on filter nesting and count
4. **Authorization**: Apply authorization filters to restrict data access

### Code Organization

1. **Use Builder Methods**: Leverage `EntityFilterBuilder` for common filter patterns
2. **Extract Complex Filters**: Move complex filter logic to dedicated methods
3. **Document Custom Expressions**: Clearly document any custom expression filters
4. **Test Filter Logic**: Write comprehensive tests for complex filter scenarios

### Example Best Practices Implementation

```csharp
public class ProductFilterService
{
    private readonly ILogger<ProductFilterService> _logger;
    private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Product.Name),
        nameof(Product.Category),
        nameof(Product.Price),
        nameof(Product.IsActive),
        nameof(Product.CreatedDate)
    };

    public EntityFilter? CreateProductFilter(ProductFilterRequest request)
    {
        var filters = new List<EntityFilter>();

        // Validate and add category filter
        if (!string.IsNullOrEmpty(request.Category))
        {
            filters.Add(EntityFilterBuilder.CreateFilter("Category", request.Category));
        }

        // Validate and add price range filters
        if (request.MinPrice.HasValue)
        {
            filters.Add(EntityFilterBuilder.CreateFilter("Price", request.MinPrice.Value, EntityFilterOperators.GreaterThanOrEqual));
        }
        
        if (request.MaxPrice.HasValue)
        {
            filters.Add(EntityFilterBuilder.CreateFilter("Price", request.MaxPrice.Value, EntityFilterOperators.LessThanOrEqual));
        }

        // Add search filter with validation
        if (!string.IsNullOrEmpty(request.SearchText))
        {
            var searchFilter = EntityFilterBuilder.CreateSearchFilter(
                new[] { "Name", "Description" }, 
                request.SearchText
            );
            if (searchFilter != null)
                filters.Add(searchFilter);
        }

        // Always filter to active products only
        filters.Add(EntityFilterBuilder.CreateFilter("IsActive", true));

        return filters.Count > 0 
            ? EntityFilterBuilder.CreateAndGroup(filters.ToArray())
            : null;
    }

    public EntityFilter ValidateFilter(EntityFilter filter)
    {
        ValidateFilterFields(filter);
        return filter;
    }

    private void ValidateFilterFields(EntityFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.Name) && !AllowedFields.Contains(filter.Name))
        {
            throw new ArgumentException($"Field '{filter.Name}' is not allowed for filtering.");
        }

        if (filter.Filters != null)
        {
            foreach (var nestedFilter in filter.Filters)
            {
                ValidateFilterFields(nestedFilter);
            }
        }
    }
}

### `EntitySort` Examples

The following example demonstrates how to use the `EntitySort` class:

```c#
var sort = new EntitySort
{
    Name = "Name",
    Direction = EntitySortDirections.Ascending
};

Console.WriteLine(sort.ToString()); // Output: "Name:asc"
```

## `EntitySelect` Class

Represents a query for selecting entities with optional filtering and sorting criteria.

This class is typically used to define the selection criteria for querying entities, including filters, sorting, and raw query expressions.

```c#
public class EntitySelect
```

### `EntitySelect` Examples

The following example demonstrates how to use the `EntitySelect` class:

```c#
var filter = new EntityFilter
{
    Name = "Status",
    Operator = EntityFilterOperators.Equal,
    Value = "Active"
};

var sort = new EntitySort
{
    Name = "Name",
    Direction = EntitySortDirections.Ascending
};

var entitySelect = new EntitySelect(filter, sort);
```

## `EntityQuery` Class

Represents a query for selecting entities with filtering, sorting, and pagination capabilities.

This class is typically used to define the criteria for querying entities, including filters, sorting, and pagination options.

```c#
public class EntityQuery
```

### `EntityQuery` Examples

The following example demonstrates how to use the `EntityQuery` class:

```c#
var filter = new EntityFilter
{
    Name = "Status",
    Operator = EntityFilterOperators.Equal,
    Value = "Active"
};

var sort = new EntitySort
{
    Name = "Name",
    Direction = EntitySortDirections.Ascending
};

var query = new EntityQuery(filter, sort, page: 1, pageSize: 20);
Console.WriteLine($"Page: {query.Page}, PageSize: {query.PageSize}");
```
