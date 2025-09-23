---
title: Paged Query
description: Query to retrieve entities in a paginated format based on filtering, sorting, and pagination criteria
---

# Paged Query

The `EntityPagedQuery<TReadModel>` represents a query to retrieve entities in a paginated format based on filtering, sorting, and pagination criteria defined in an `EntityQuery`. This query follows the CQRS (Command Query Responsibility Segregation) pattern and returns an `EntityPagedResult<TReadModel>` containing the paged data and total count.

## Overview

The paged query is a fundamental part of the Arbiter framework's pagination operations. It inherits from `CacheableQueryBase<EntityPagedResult<TReadModel>>` which provides automatic security context, caching support, and JSON serialization.

```csharp
public record EntityPagedQuery<TReadModel> : CacheableQueryBase<EntityPagedResult<TReadModel>>
```

## Key Features

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Tracking**: Automatic tracking of who activated the query and when
- **Cache Integration**: Built-in caching support with hash-based cache keys for query criteria
- **Validation**: Integrated with validation pipeline behaviors
- **Mapping**: Uses Mapper for converting between entities and read models
- **Tenant Support**: Optional multi-tenant support through pipeline behaviors
- **Flexible Filtering**: Support for complex filtering using `EntityFilter` with multiple operators
- **Dynamic Sorting**: Support for multiple sort criteria with `EntitySort`
- **Raw Queries**: Support for raw query expressions using Dynamic LINQ
- **Pagination**: Built-in pagination with configurable page size and page number
- **Total Count**: Efficient total count calculation without loading all data
- **Performance Optimization**: Short-circuit optimization for zero results

## Type Parameters

| Parameter    | Description                                                    |
| ------------ | -------------------------------------------------------------- |
| `TReadModel` | The type of the read model returned as the result of the query |

## Constructor Parameters

| Parameter   | Type               | Description                                                                              |
| ----------- | ------------------ | ---------------------------------------------------------------------------------------- |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization                   |
| `query`     | `EntityQuery?`     | The query criteria defining filters, sorting, and pagination (defaults to empty if null) |

## Properties

| Property | Type          | Description                                                  |
| -------- | ------------- | ------------------------------------------------------------ |
| `Query`  | `EntityQuery` | The query criteria defining filters, sorting, and pagination |

## EntityQuery Structure

The `EntityQuery` class provides filtering, sorting, and pagination properties:

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

**Note**: When `Page`, `PageSize`, and `ContinuationToken` are all null, all matching rows are returned.

## Pagination Options

The `EntityQuery` supports multiple pagination modes:

### Page-Based Pagination

Traditional pagination using page numbers and page sizes:

```csharp
var query = new EntityQuery
{
    Page = 1,
    PageSize = 20
};
```

### No Pagination (All Results)

When `Page`, `PageSize`, and `ContinuationToken` are all null, **all matching rows are returned**:

```csharp
// This will return ALL matching results
var query = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Sort = new List<EntitySort> { new EntitySort { Name = "Name", Direction = "asc" } }
};
```

**Important**: Use this mode carefully with large datasets as it can impact performance and memory usage.

### Continuation Token Pagination

Stateless pagination using continuation tokens (read-only property):

```csharp
// The ContinuationToken is provided by the query results
// and used for retrieving subsequent pages efficiently
var nextPageToken = result.ContinuationToken; // From previous query result
```

**Note**: Continuation tokens provide better performance for large datasets and avoid consistency issues when data changes between page requests.

## EntityPagedResult Structure

The result contains both the paged data and total count:

```csharp
public class EntityPagedResult<TReadModel>
{
    // Continuation token for stateless pagination if provider supports
    public string? ContinuationToken { get; set; }
    
    // Total number of results
    public long? Total { get; set; }
    
    // Current page data or all rows if no pagination specified
    public IReadOnlyList<TReadModel>? Data { get; set; }
}
```

## Caching Features

The paged query automatically implements caching capabilities with hash-based cache keys:

### Cache Key Generation

```csharp
public override string GetCacheKey()
    => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Paged, Query.GetHashCode());
```

The cache key is generated by hashing the entire `EntityQuery` object, including filters, sorting, and pagination parameters.

### Cache Tag Support

```csharp
public override string? GetCacheTag()
    => CacheTagger.GetTag<TReadModel>();
```

Cache tags enable efficient cache invalidation when related entities are modified.

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityPagedQueryHandler<TContext, TEntity, TReadModel>
```

Uses Entity Framework with efficient count and paging operations, then projects results to read models.

### MongoDB Handler

```csharp
EntityPagedQueryHandler<TRepository, TEntity, TKey, TReadModel>
```

Uses MongoDB repository pattern with efficient aggregation pipelines and maps results to read models.

## Service Registration

Register paged query support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityQueries<MyDbContext, Product, int, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityQueries<IProductRepository, Product, int, ProductReadModel>();
```

## Model Mapping with IMapper

The paged query relies on `IMapper<TSource, TDestination>` to convert between entities and read models:

### Entity Framework Mapping

```csharp
var projected = Mapper.ProjectTo<TEntity, TReadModel>(pagedQuery);
var data = await projected.ToListAsync(cancellationToken);

return new EntityPagedResult<TReadModel> { Total = total, Data = data };
```

### MongoDB Mapping

```csharp
var data = Mapper.Map<IList<TEntity>, IReadOnlyList<TReadModel>>(results);
return new EntityPagedResult<TReadModel> { Total = total, Data = data };
```

## Pipeline Behaviors

The paged query automatically includes several pipeline behaviors:

- **Tenant Security**: `TenantPagedQueryBehavior` (if read model implements `IHaveTenant<TKey>`)
  - Validates that the user has access to the specified tenant
  - Automatically filters results to the user's tenant

- **Soft Delete Filtering**: `DeletedFilterQueryBehavior` (if read model implements `ITrackDeleted`)
  - Automatically filters out soft-deleted entities from query results
  - Respects the `IsDeleted` flag on entities

- **Caching**: `HybridCacheQueryBehavior`
  - Automatically caches query results based on the complete query criteria hash
  - Respects cache expiration policies set on the query
  - Handles cache invalidation using cache tags

## Cache Configuration

Configure caching policies on your queries:

### Relative Expiration

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute expiration

var result = await mediator.Send(query);
```

### Absolute Expiration

```csharp
var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // Expires at specific time

var result = await mediator.Send(query);
```

### Hybrid Cache Registration

```csharp
services.AddEntityHybridCache();
```

## Usage Examples

### Basic Pagination

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var entityQuery = new EntityQuery
{
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query);

Console.WriteLine($"Page 1 of {Math.Ceiling((double)(result.Total ?? 0) / (entityQuery.PageSize ?? 20))} pages");
Console.WriteLine($"Showing {result.Data?.Count} of {result.Total ?? 0} total items");
```

### All Results (No Pagination)

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

// When Page, PageSize, and ContinuationToken are all null, ALL matching rows are returned
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Sort = new List<EntitySort> { new EntitySort { Name = "Name", Direction = "asc" } }
    // No pagination properties set - will return all matching results
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query);

Console.WriteLine($"Retrieved all {result.Data?.Count} matching items");
Console.WriteLine($"Total count: {result.Total ?? 0}");
```

### With Filtering and Sorting

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Sort = new List<EntitySort> 
    { 
        new EntitySort { Name = "Name", Direction = "asc" } 
    },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query);
```

### Complex Filtering with Pagination

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter
    {
        Logic = "and",
        Filters = new List<EntityFilter>
        {
            new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
            new EntityFilter { Name = "Price", Operator = "gt", Value = "10.00" },
            new EntityFilter { Name = "CategoryId", Operator = "in", Value = "1,2,3" }
        }
    },
    Sort = new List<EntitySort>
    {
        new EntitySort { Name = "CategoryId", Direction = "asc" },
        new EntitySort { Name = "Name", Direction = "asc" }
    },
    Page = 2,
    PageSize = 50
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query);
```

### Navigation Through Pages

```csharp
// Get first page
var entityQuery = new EntityQuery { Page = 1, PageSize = 10 };
var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var firstPage = await mediator.Send(query);

Console.WriteLine($"Total items: {firstPage.Total ?? 0}");
Console.WriteLine($"Total pages: {Math.Ceiling((double)(firstPage.Total ?? 0) / (entityQuery.PageSize ?? 10))}");

// Get next page
entityQuery.Page = 2;
var nextPageQuery = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var secondPage = await mediator.Send(nextPageQuery);
```

### In ASP.NET Core Controller

```csharp
[HttpGet]
public async Task<EntityPagedResult<ProductReadModel>> GetProducts(
    [FromQuery] string? q = null,
    [FromQuery] string? sort = null,
    [FromQuery] int page = 1,
    [FromQuery] int size = 20)
{
    var entityQuery = new EntityQuery(q, page, size, sort);
    var query = new EntityPagedQuery<ProductReadModel>(User, entityQuery);
    return await mediator.Send(query) ?? new EntityPagedResult<ProductReadModel>();
}
```

### In Minimal API Endpoint

```csharp
app.MapGet("/products", async (
    [FromServices] IMediator mediator,
    [FromQuery] string? q,
    [FromQuery] string? sort,
    [FromQuery] int page = 1,
    [FromQuery] int size = 20,
    ClaimsPrincipal user) =>
{
    var entityQuery = new EntityQuery(q, page, size, sort);
    var query = new EntityPagedQuery<ProductReadModel>(user, entityQuery);
    var result = await mediator.Send(query);
    return Results.Ok(result ?? new EntityPagedResult<ProductReadModel>());
});
```

### With Caching

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);

// Configure 30-minute relative cache
query.Cache(TimeSpan.FromMinutes(30));

var result = await mediator.Send(query);
```

### Using Continuation Tokens

```csharp
// First page
var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var firstPage = await mediator.Send(query);

// Check if there are more pages using continuation token
if (!string.IsNullOrEmpty(firstPage.ContinuationToken))
{
    // Use continuation token for next page (implementation depends on handler)
    // This is typically handled by the specific data provider
    Console.WriteLine($"More pages available. Token: {firstPage.ContinuationToken}");
}
```

## Return Values

- **Success**: Returns `EntityPagedResult<TReadModel>` containing:
  - `Total`: Total number of matching entities (nullable - may be null in some scenarios)
  - `Data`: Current page of entities as `IReadOnlyList<TReadModel>` (may be null or empty if no results)
  - `ContinuationToken`: Token for stateless pagination (may be null if not supported or no more pages)
- **Empty Results**: Can return `EntityPagedResult<TReadModel>.Empty` or a result with `Total = 0`/`null` and empty/null `Data` collection
- **Exception**: Throws appropriate exceptions for validation or data access errors

## Error Handling

The query handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the request parameter is null
- **`DomainException`**: For business rule violations
- **Database exceptions**: For data access errors
- **Filter exceptions**: For invalid filter expressions
- **Authorization exceptions**: When user lacks access to entities
- **Pagination exceptions**: For invalid page numbers or page sizes

## Best Practices

1. **Page Size Limits**: Implement reasonable page size limits (e.g., max 100 items per page)
2. **Null Pagination Behavior**: Be aware that when Page, PageSize, and ContinuationToken are all null, **all matching rows are returned** - use filters appropriately to limit result sets
3. **Cache Strategically**: Cache frequently accessed pages with appropriate expiration
4. **Security**: Always pass the current user's `ClaimsPrincipal` for proper authorization
5. **Filter Design**: Design efficient filters that can leverage database indexes
6. **Total Count Optimization**: Consider caching total counts for expensive queries
7. **Performance**: Use appropriate page sizes based on data size and network conditions
8. **Navigation**: Implement proper pagination navigation in UI components
9. **Tenant Isolation**: Implement `IHaveTenant<TKey>` on read models for multi-tenant scenarios

## Performance Considerations

1. **Entity Framework**:
   - Uses `AsNoTracking()` for read-only operations
   - Performs efficient `CountAsync()` for total count
   - Uses `ProjectTo` for efficient database projection
   - Applies pagination at the database level with `Skip()` and `Take()`
   - Short-circuits when total count is zero

2. **MongoDB**:
   - Uses aggregation pipeline for efficient pagination
   - Applies filtering and sorting at the database level
   - Uses `Skip()` and `Take()` for efficient pagination

3. **Caching**:
   - Hash-based cache keys include pagination parameters
   - Consider caching strategies for different page sizes
   - Implement appropriate cache expiration policies

4. **Database Optimization**:
   - Ensure proper database indexes on filtered and sorted columns
   - Monitor query performance for complex filter expressions
   - Consider the cost of counting large result sets

5. **Memory Usage**:
   - Page sizes directly impact memory usage
   - **No Pagination Warning**: When all pagination properties are null, ALL matching rows are loaded into memory - use appropriate filters to limit result sets
   - Consider streaming for very large datasets
   - Monitor total count calculation performance for large tables

6. **Network Efficiency**:
   - Choose appropriate page sizes for network conditions
   - Consider total payload size including metadata
   - Implement proper loading states in UI components
