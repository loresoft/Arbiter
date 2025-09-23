---
title: Query Abstracts
description: Comprehensive set of abstract base classes for implementing CQRS queries
---

# Query Abstracts

The Arbiter framework provides a comprehensive set of abstract base classes for implementing CQRS queries. These base classes follow consistent patterns and provide built-in support for security, caching, and type safety.

## Overview

All query abstracts in Arbiter follow the CQRS (Command Query Responsibility Segregation) pattern and include:

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Trail**: Automatic tracking of who activated the query and when  
- **Type Safety**: Strongly-typed generic parameters for keys, models, and responses
- **Caching Support**: Built-in support for memory cache and hybrid cache with configurable expiration
- **JSON Serialization**: Full support for serialization with appropriate converters

> **Note**: Arbiter queries implement the `IRequest<TResponse>` interface from `Arbiter.Mediation`, which provides a MediatR-compatible API with additional features specific to the Arbiter framework.

## `CacheableQueryBase<TResponse>` Class

Base class for queries that support caching with configurable expiration policies.

```csharp
public abstract record CacheableQueryBase<TResponse> : PrincipalCommandBase<TResponse>, ICacheResult
```

### When to Use

Use `CacheableQueryBase` when your query needs to:

- Cache results to improve performance
- Support configurable cache expiration
- Provide cache key generation and tagging
- Integrate with Arbiter's caching behaviors

### Caching Features

`CacheableQueryBase` extends `PrincipalCommandBase` with:

- **Cache Key Generation**: Abstract method requiring implementation for unique cache keys
- **Cache Tagging**: Virtual method for cache invalidation strategies
- **Expiration Control**: Support for both absolute and relative expiration policies
- **Cache Behavior Integration**: Compatible with `HybridCacheQueryBehavior`

### Generic Parameters

| Parameter   | Description                                    |
| ----------- | ---------------------------------------------- |
| `TResponse` | The type of the response returned by the query |

### Abstract Methods

| Method          | Description                                                     |
| --------------- | --------------------------------------------------------------- |
| `GetCacheKey()` | Must be implemented to provide a unique cache key for the query |

### Virtual Methods

| Method          | Description                                                         |
| --------------- | ------------------------------------------------------------------- |
| `GetCacheTag()` | Can be overridden to provide cache tags for invalidation strategies |

### Cache Control Methods

| Method                   | Description                                         |
| ------------------------ | --------------------------------------------------- |
| `Cache(DateTimeOffset?)` | Sets absolute expiration for the cache entry        |
| `Cache(TimeSpan?)`       | Sets relative expiration for the cache entry         |
| `IsCacheable()`          | Returns true if any expiration policy is configured |

### Implementation Example

```csharp
public record GetProductByIdQuery : CacheableQueryBase<ProductReadModel>
{
    public GetProductByIdQuery(ClaimsPrincipal principal, int productId)
        : base(principal)
    {
        ProductId = productId;
    }

    public int ProductId { get; }

    public override string GetCacheKey()
        => $"Product:Id:{ProductId}";

    public override string? GetCacheTag()
        => "Products";
}

var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var query = new GetProductByIdQuery(principal, 123);

// Configure caching
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute relative expiration

var result = await mediator.Send(query);
Console.WriteLine($"Product Name: {result?.Name}");
```

### Caching Use Cases

- Retrieving frequently accessed reference data
- Results from expensive database queries
- API responses from external services
- Computed results that don't change frequently

## Built-in Query Implementations

Arbiter provides several concrete query implementations that inherit from `CacheableQueryBase`:

### `EntityIdentifierQuery<TKey, TReadModel>`

Query for retrieving a single entity by its identifier.

```csharp
public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
```

**Use Cases:**

- Get entity by ID
- Retrieve single records
- Detail views

**Example:**

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);
var product = await mediator.Send(query);
```

### `EntityIdentifiersQuery<TKey, TReadModel>`

Query for retrieving multiple entities by their identifiers.

```csharp
public record EntityIdentifiersQuery<TKey, TReadModel> : CacheableQueryBase<IReadOnlyList<TReadModel>>
```

**Use Cases:**

- Bulk entity retrieval
- Getting related entities
- Multi-select operations

**Example:**

```csharp
var ids = new List<int> { 1, 2, 3, 4, 5 };
var query = new EntityIdentifiersQuery<int, ProductReadModel>(principal, ids);
var products = await mediator.Send(query);
```

### `EntityPagedQuery<TReadModel>`

Query for retrieving a paginated collection of entities based on filtering and sorting criteria.

```csharp
public record EntityPagedQuery<TReadModel> : CacheableQueryBase<EntityPagedResult<TReadModel>>
```

**Use Cases:**

- Paginated lists with filtering and sorting
- Large datasets requiring efficient loading
- API endpoints with pagination requirements

**Example:**

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = FilterOperators.Equal, Value = "Active" },
    Page = 1,
    PageSize = 20
};
var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var pagedResult = await mediator.Send(query);
```

## Best Practices

### Security Considerations

1. **Always validate the principal**: Check user permissions in your query handlers
2. **Use strongly-typed claims**: Extract specific claims needed for authorization
3. **Audit sensitive queries**: Leverage the built-in `ActivatedBy` and `Activated` properties

### Caching Strategies

1. **Choose appropriate expiration**: Use absolute expiration for time-sensitive data.
2. **Implement cache tags**: Use `GetCacheTag()` for efficient cache invalidation
3. **Consider cache key uniqueness**: Ensure cache keys are unique across different query types and parameters
4. **Monitor cache performance**: Use the `HybridCacheQueryBehavior` for optimal caching performance

### Type Safety

1. **Use appropriate key types**: Choose `int`, `Guid`, or `string` based on your domain
2. **Define clear response models**: Ensure your read models have proper validation and serialization attributes
3. **Keep queries immutable**: Use records and readonly properties where possible

### Performance

1. **Use caching judiciously**: Not all queries benefit from caching - profile your application
2. **Implement proper cache keys**: Avoid cache key collisions and ensure uniqueness
3. **Consider query complexity**: Complex filtering should be handled at the data layer, not in memory

## Cache Integration

Arbiter provides hybrid caching behavior for optimal performance:

### Hybrid Cache Integration

```csharp
services.AddEntityHybridCache();
```

The hybrid cache behavior automatically handles:

- Cache key generation
- Expiration policies
- Cache tagging
- Error handling

## Inheritance Hierarchy

```text
IRequest<TResponse>
└── PrincipalCommandBase<TResponse>
    └── CacheableQueryBase<TResponse>
        ├── EntityIdentifierQuery<TKey, TReadModel>
        ├── EntityIdentifiersQuery<TKey, TReadModel>
        └── EntityPagedQuery<TReadModel>
```
