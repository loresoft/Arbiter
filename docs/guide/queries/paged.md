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

The `EntityQuery` class extends `EntitySelect` with pagination properties:

```csharp
public class EntityQuery : EntitySelect
{
    public int Page { get; set; }           // Page number (default: 1)
    public int PageSize { get; set; }       // Page size (default: 20)
    
    // Inherited from EntitySelect:
    public string? Query { get; set; }           // Raw LINQ query expression
    public IList<EntitySort>? Sort { get; set; } // Sort criteria
    public EntityFilter? Filter { get; set; }    // Filter criteria
}
```

### Constructor Overloads

The `EntityQuery` provides multiple constructor overloads:

#### Default Constructor

```csharp
public EntityQuery()
```

Creates a query with default pagination (Page = 1, PageSize = 20).

#### Raw Query Constructor

```csharp
public EntityQuery(string? query, int page, int pageSize, string? sort)
```

Creates a query with raw query expression, pagination, and sorting.

#### Filter with Pagination Constructor

```csharp
public EntityQuery(EntityFilter? filter, int page = 1, int pageSize = 20)
```

Creates a query with filtering and pagination.

#### Filter, Sort and Pagination Constructor

```csharp
public EntityQuery(EntityFilter? filter, EntitySort? sort, int page = 1, int pageSize = 20)
public EntityQuery(EntityFilter? filter, IEnumerable<EntitySort>? sort, int page = 1, int pageSize = 20)
```

Creates a query with filtering, sorting, and pagination.

## EntityPagedResult Structure

The result contains both the paged data and total count:

```csharp
public class EntityPagedResult<TReadModel>
{
    public long Total { get; set; }                            // Total number of results
    public IReadOnlyCollection<TReadModel>? Data { get; set; } // Current page data
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
var data = Mapper.Map<IList<TEntity>, IReadOnlyCollection<TReadModel>>(results);
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

- **Caching**: `MemoryCacheQueryBehavior` or `HybridCacheQueryBehavior`
  - Automatically caches query results based on the complete query criteria hash
  - Respects cache expiration policies set on the query
  - Handles cache invalidation using cache tags

## Cache Configuration

Configure caching policies on your queries:

### Sliding Expiration

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute sliding expiration

var result = await mediator.Send(query);
```

### Absolute Expiration

```csharp
var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // Expires at specific time

var result = await mediator.Send(query);
```

### Memory Cache Registration

```csharp
services.AddEntityMemoryCache();
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

Console.WriteLine($"Page 1 of {Math.Ceiling((double)result.Total / entityQuery.PageSize)} pages");
Console.WriteLine($"Showing {result.Data?.Count} of {result.Total} total items");
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

Console.WriteLine($"Total items: {firstPage.Total}");
Console.WriteLine($"Total pages: {Math.Ceiling((double)firstPage.Total / entityQuery.PageSize)}");

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

// Configure 30-minute sliding cache
query.Cache(TimeSpan.FromMinutes(30));

var result = await mediator.Send(query);
```

## Return Values

- **Success**: Returns `EntityPagedResult<TReadModel>` containing:
  - `Total`: Total number of matching entities
  - `Data`: Current page of entities (may be empty if no results)
- **Empty Results**: Returns result with `Total = 0` and empty `Data` collection
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
2. **Cache Strategically**: Cache frequently accessed pages with appropriate expiration
3. **Security**: Always pass the current user's `ClaimsPrincipal` for proper authorization
4. **Filter Design**: Design efficient filters that can leverage database indexes
5. **Total Count Optimization**: Consider caching total counts for expensive queries
6. **Performance**: Use appropriate page sizes based on data size and network conditions
7. **Navigation**: Implement proper pagination navigation in UI components
8. **Tenant Isolation**: Implement `IHaveTenant<TKey>` on read models for multi-tenant scenarios

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
   - Consider streaming for very large datasets
   - Monitor total count calculation performance for large tables

6. **Network Efficiency**:
   - Choose appropriate page sizes for network conditions
   - Consider total payload size including metadata
   - Implement proper loading states in UI components
