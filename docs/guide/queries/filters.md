---
title: Query Filters
description: Powerful mechanisms for filtering and sorting entity queries dynamically
---

# Query Filters

The `EntityFilter` and `EntitySort` classes provide powerful mechanisms for filtering and sorting entity queries dynamically. These components enable complex filtering and sorting operations on entity collections with support for nested logical operations, operator mapping, and dynamic LINQ expression generation.

## Overview

The filtering and sorting system consists of several key components:

- [`EntityQuery`](#entityquery) - Main query container with filtering, sorting, and pagination capabilities
- [`EntityFilter`](#entityfilter) - Represents filtering criteria with support for nested filters and logical operators
- [`EntitySort`](#entitysort) - Represents sorting criteria with field name and direction
- [`FilterOperators`](#filter-operators) - Enum providing type-safe filter operators
- [`FilterLogic`](#filter-logic) - Enum providing type-safe logical operators
- [`SortDirections`](#sort-directions) - Enum providing type-safe sort directions
- [`EntityFilterBuilder`](#entityfilterbuilder) - Helper methods for building common filters
- [`LinqExpressionBuilder`](#linq-expression-builder) - Converts filters to dynamic LINQ expressions

## EntityQuery

The `EntityQuery` class represents a query for selecting entities with filtering, sorting, and pagination capabilities. This class is typically used to define the criteria for querying entities, including filters, sorting, and pagination options.

### EntityQuery Structure

```csharp
public class EntityQuery
{
    // Pagination properties
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    
    // Query properties
    public string? Query { get; set; }           // Raw LINQ query expression
    public IList<EntitySort>? Sort { get; set; } // Sort criteria
    public EntityFilter? Filter { get; set; }    // Filter criteria
    
    // Continuation token for stateless pagination
    public string? ContinuationToken { get; set; }
}
```

### EntityQuery Properties

#### Page

The page number for pagination. When null, pagination may be disabled or use default values.

#### PageSize

The number of items per page. When null, pagination may be disabled or use default values.

#### Query

A raw LINQ query expression string for advanced filtering scenarios.

#### Sort

A list of sort criteria specifying how to order the results.

#### Filter

The filter criteria for selecting entities based on specific conditions.

#### ContinuationToken

A read-only token for stateless pagination, typically provided by previous query results.

### EntityQuery Usage Examples

#### Basic Query with Pagination

```csharp
var query = new EntityQuery
{
    Page = 1,
    PageSize = 20
};
```

#### Query with Filter and Sort

```csharp
var filter = new EntityFilter
{
    Name = "Status",
    Operator = FilterOperators.Equal,
    Value = "Active"
};

var sort = new EntitySort
{
    Name = "Name",
    Direction = SortDirections.Ascending
};

var query = new EntityQuery
{
    Filter = filter,
    Sort = new List<EntitySort> { sort },
    Page = 1,
    PageSize = 20
};
```

#### Complex Query Example

```csharp
var query = new EntityQuery
{
    Filter = new EntityFilter
    {
        Logic = FilterLogic.And,
        Filters = new List<EntityFilter>
        {
            new EntityFilter { Name = "IsActive", Operator = FilterOperators.Equal, Value = true },
            new EntityFilter { Name = "Price", Operator = FilterOperators.GreaterThan, Value = 10.0 }
        }
    },
    Sort = new List<EntitySort>
    {
        new EntitySort { Name = "Priority", Direction = SortDirections.Descending },
        new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
    },
    Page = 1,
    PageSize = 50
};
```

## EntityFilter

The `EntityFilter` class represents a filter for selecting entities based on specific criteria. It supports both simple field-based filters and complex nested filter groups with logical operators.

### Class Definition

```csharp
[JsonConverter(typeof(EntityFilterConverter))]
public class EntityFilter
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("operator")]
    [JsonConverter(typeof(JsonStringEnumConverter<FilterOperators>))]
    public FilterOperators? Operator { get; set; }

    [JsonPropertyName("logic")]
    [JsonConverter(typeof(JsonStringEnumConverter<FilterLogic>))]
    public FilterLogic? Logic { get; set; }

    [JsonPropertyName("filters")]
    public IList<EntityFilter>? Filters { get; set; }
}
```

### Properties

#### Name

The name of the field or property to filter on. This should match the property name of the entity being queried.

#### Operator

The operator to use for the filter. Use the `FilterOperators` enum for type safety:

```csharp
// Version 2.0 - Using FilterOperators enum
var filter = new EntityFilter
{
    Name = "Status",
    Operator = FilterOperators.Equal,  // Type-safe enum value
    Value = "Active"
};
```

The enum provides the following operators:

- `FilterOperators.Equal` - Equals comparison
- `FilterOperators.NotEqual` - Not equals comparison  
- `FilterOperators.GreaterThan` - Greater than comparison
- `FilterOperators.LessThan` - Less than comparison
- `FilterOperators.GreaterThanOrEqual` - Greater than or equal comparison
- `FilterOperators.LessThanOrEqual` - Less than or equal comparison
- `FilterOperators.Contains` - String contains operation
- `FilterOperators.NotContains` - String does not contain operation
- `FilterOperators.StartsWith` - String starts with operation
- `FilterOperators.NotStartsWith` - String does not start with operation
- `FilterOperators.EndsWith` - String ends with operation
- `FilterOperators.NotEndsWith` - String does not end with operation
- `FilterOperators.In` - Value in collection operation
- `FilterOperators.NotIn` - Value not in collection operation
- `FilterOperators.IsNull` - Null check operation
- `FilterOperators.IsNotNull` - Not null check operation
- `FilterOperators.Expression` - Custom expression operation

#### Value

The value to filter for. Can be any object type including primitives, strings, arrays, and collections.

#### Logic

The logical operator to combine nested filters. Use the `FilterLogic` enum for type safety:

- `FilterLogic.And` - All nested filters must match
- `FilterLogic.Or` - Any nested filter must match

#### Filters

A list of nested filters for complex filter groups. When this property is set, the filter acts as a group filter using the specified `Logic` operator.

### Basic Filter Examples

#### Simple Equality Filter

```csharp
var filter = new EntityFilter
{
    Name = "Status",
    Operator = FilterOperators.Equal,
    Value = "Active"
};
```

#### String Operations

```csharp
// Contains operation
var containsFilter = new EntityFilter
{
    Name = "Name",
    Operator = FilterOperators.Contains,
    Value = "berry"
};

// StartsWith operation
var startsWithFilter = new EntityFilter
{
    Name = "Name",
    Operator = FilterOperators.StartsWith,
    Value = "P"
};

// EndsWith operation
var endsWithFilter = new EntityFilter
{
    Name = "Name",
    Operator = FilterOperators.EndsWith,
    Value = "berry"
};
```

#### Comparison Operations

```csharp
// Numeric comparison
var greaterThanFilter = new EntityFilter
{
    Name = "Rank",
    Operator = FilterOperators.GreaterThan,
    Value = 5
};

// Date comparison
var dateFilter = new EntityFilter
{
    Name = "CreatedDate",
    Operator = FilterOperators.GreaterThanOrEqual,
    Value = DateTime.Today
};
```

#### Collection Operations

```csharp
// In operation
var inFilter = new EntityFilter
{
    Name = "Category",
    Operator = FilterOperators.In,
    Value = new[] { "Fruit", "Vegetable", "Grain" }
};
```

#### Null Checks

```csharp
// Null check
var nullFilter = new EntityFilter
{
    Name = "Description",
    Operator = FilterOperators.IsNull
};

// Not null check
var notNullFilter = new EntityFilter
{
    Name = "Description",
    Operator = FilterOperators.IsNotNull
};
```

### Complex Filter Examples

#### Logical OR Group

```csharp
var orFilter = new EntityFilter
{
    Logic = FilterLogic.Or,
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
    Filters = new List<EntityFilter> // Default logic is FilterLogic.And
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
        new EntityFilter { Name = "Rank", Operator = FilterOperators.GreaterThan, Value = 5 },
        new EntityFilter
        {
            Logic = FilterLogic.Or,
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
    Operator = FilterOperators.Expression,
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
    Operator = FilterOperators.Contains, 
    Value = "berry" 
});

// Apply a complex filter
var complexFiltered = fruits.Filter(new EntityFilter
{
    Logic = FilterLogic.And,
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Operator = FilterOperators.GreaterThan, Value = 5 },
        new EntityFilter { Name = "IsActive", Value = true }
    }
});
```

## EntitySort

The `EntitySort` class represents a sort expression for an entity, specifying the property to sort by and the sort direction.

### EntitySort Class Definition

```csharp
public class EntitySort
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SortDirections? Direction { get; set; }

    public static EntitySort? Parse(string? sortString);
    public override string ToString();
    public override int GetHashCode() => HashCode.Combine(Name, Direction);
}
```

### EntitySort Properties

#### Name

The name of the field or property to sort by. This should match the property name of the entity being queried.

#### Direction

The sort direction. Use the `SortDirections` enum for type safety:

- `SortDirections.Ascending` - Ascending order (default)
- `SortDirections.Descending` - Descending order

### Sort Examples

#### Basic Sort

```csharp
var sort = new EntitySort
{
    Name = "Name",
    Direction = SortDirections.Ascending
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
    new EntitySort { Name = "Priority", Direction = SortDirections.Descending },
    new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
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
    Direction = SortDirections.Ascending 
});

// Apply multiple sorts
var multipleSorted = fruits.Sort(new List<EntitySort>
{
    new EntitySort { Name = "Priority", Direction = SortDirections.Descending },
    new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
});
```

## Filter Operators

The `FilterOperators` enum provides type-safe filter operators for Version 2.0:

```csharp
public enum FilterOperators
{
    Equal,
    NotEqual,
    Contains,
    NotContains,
    StartsWith,
    NotStartsWith,
    EndsWith,
    NotEndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    IsNull,
    IsNotNull,
    In,
    NotIn,
    Expression
}
```

### Operator Examples

```csharp
// Using FilterOperators enum (Version 2.0)
var filter = new EntityFilter
{
    Name = "Status",
    Operator = FilterOperators.Equal,
    Value = "Active"
};

var containsFilter = new EntityFilter
{
    Name = "Description",
    Operator = FilterOperators.Contains,
    Value = "important"
};
```

## Filter Logic

The `FilterLogic` enum provides type-safe logical operators for Version 2.0:

```csharp
public enum FilterLogic
{
    And,
    Or
}
```

### Logic Examples

```csharp
// Using FilterLogic enum (Version 2.0)
var orGroup = new EntityFilter
{
    Logic = FilterLogic.Or,
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Status", Value = "Active" },
        new EntityFilter { Name = "Status", Value = "Pending" }
    }
};
```

## Sort Directions

The `SortDirections` enum provides type-safe sort directions for Version 2.0:

```csharp
public enum SortDirections
{
    Ascending,
    Descending
}
```

### Direction Examples

```csharp
// Using SortDirections enum (Version 2.0)
var sort = new EntitySort
{
    Name = "CreatedDate",
    Direction = SortDirections.Descending
};
```

## EntityFilterBuilder

The `EntityFilterBuilder` class provides static helper methods for building common entity filters and queries:

### Common Methods

```csharp
public static class EntityFilterBuilder
{
    // Create search queries and filters
    public static EntityQuery? CreateSearchQuery<TModel>(string searchText, int page = 1, int pageSize = 20) where TModel : class, ISupportSearch;
    public static EntityFilter? CreateSearchFilter<TModel>(string searchText) where TModel : class, ISupportSearch;
    public static EntityFilter? CreateSearchFilter(IEnumerable<string> fields, string searchText);
    
    // Create sort expressions
    public static EntitySort? CreateSort<TModel>() where TModel : class, ISupportSearch;
    public static EntitySort CreateSort(string field, SortDirections? direction = null);
    
    // Create filters
    public static EntityFilter CreateFilter(string field, object? value, FilterOperators? @operator = null);
    
    // Create filter groups
    public static EntityFilter? CreateGroup(params IEnumerable<EntityFilter?> filters);
    public static EntityFilter? CreateGroup(FilterLogic logic, params IEnumerable<EntityFilter?> filters);
}
```

### Builder Examples

```csharp
// Create a simple filter
var statusFilter = EntityFilterBuilder.CreateFilter("Status", "Active", FilterOperators.Equal);

// Create filter groups using CreateGroup
var andGroup = EntityFilterBuilder.CreateGroup(FilterLogic.And, new[]
{
    EntityFilterBuilder.CreateFilter("Status", "Active"),
    EntityFilterBuilder.CreateFilter("Priority", 1, FilterOperators.GreaterThan)
});

// Create an OR group
var orGroup = EntityFilterBuilder.CreateGroup(FilterLogic.Or, new[]
{
    EntityFilterBuilder.CreateFilter("Category", "Fruit"),
    EntityFilterBuilder.CreateFilter("Category", "Vegetable")
});

// Create search filter for specific fields
var searchFilter = EntityFilterBuilder.CreateSearchFilter(
    new[] { "Name", "Description" }, 
    "apple"
);

// Create search query with pagination
var searchQuery = EntityFilterBuilder.CreateSearchQuery<Product>("laptop", 1, 20);

// Create sort expressions
var defaultSort = EntityFilterBuilder.CreateSort<Product>();
var customSort = EntityFilterBuilder.CreateSort("Name", SortDirections.Descending);
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
var containsFilter = new EntityFilter { Name = "Name", Operator = FilterOperators.Contains, Value = "berry" };
builder.Build(containsFilter);
// Result: "Name.Contains(@0)", Parameters: ["berry"]

// Complex group: (Rank > @0 and (Name == @1 or Name == @2))
var complexFilter = new EntityFilter
{
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Rank", Operator = FilterOperators.GreaterThan, Value = 5 },
        new EntityFilter
        {
            Logic = FilterLogic.Or,
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
                new EntitySort { Name = "PublishedDate", Direction = SortDirections.Descending }
            },
            Page = page,
            PageSize = pageSize
        };
    }
}
```

### Dynamic Filtering

```csharp
public class DynamicProductQuery : EntityPagedQuery<ProductReadModel>
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
            Query = new EntityQuery
            {
                Filter = filters.Count == 1 
                    ? filters[0] 
                    : EntityFilterBuilder.CreateGroup(FilterLogic.And, filters)
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
