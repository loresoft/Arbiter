---
title: Identifier Query
description: Query to retrieve a single entity identified by a specific key
---

# Identifier Query

The `EntityIdentifierQuery<TKey, TReadModel>` represents a query to retrieve a single entity identified by a specific key. This query follows the CQRS (Command Query Responsibility Segregation) pattern and returns a read model representing the requested entity.

## Overview

The identifier query is a fundamental part of the Arbiter framework's query operations. It inherits from `CacheableQueryBase<TReadModel>` which provides automatic security context, caching support, and JSON serialization.

```csharp
public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
```

## Key Features

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Tracking**: Automatic tracking of who activated the query and when
- **Cache Integration**: Built-in caching support with configurable expiration policies
- **Validation**: Integrated with validation pipeline behaviors
- **Mapping**: Uses Mapper for converting between entities and read models
- **Tenant Support**: Optional multi-tenant support through pipeline behaviors
- **Null Safety**: Validates that the identifier is not null at construction time

## Type Parameters

| Parameter    | Description                                                    |
| ------------ | -------------------------------------------------------------- |
| `TKey`       | The type of the key used to identify the entity                |
| `TReadModel` | The type of the read model returned as the result of the query |

## Constructor Parameters

| Parameter   | Type               | Description                                                            |
| ----------- | ------------------ | ---------------------------------------------------------------------- |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization |
| `id`        | `TKey`             | The identifier of the entity to retrieve (guaranteed non-null)         |

## Properties

| Property | Type   | Description                              |
| -------- | ------ | ---------------------------------------- |
| `Id`     | `TKey` | The identifier of the entity to retrieve |

## Caching Features

The identifier query automatically implements caching capabilities:

### Cache Key Generation

```csharp
public override string GetCacheKey()
    => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);
```

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
EntityIdentifierQueryHandler<TContext, TEntity, TKey, TReadModel>
```

Uses Entity Framework to query the database and project results to read models.

### MongoDB Handler

```csharp
EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TReadModel>
```

Uses MongoDB repository pattern to query the database and map results to read models.

## Service Registration

Register identifier query support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityQueries<MyDbContext, Product, int, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityQueries<IProductRepository, Product, int, ProductReadModel>();
```

## Model Mapping with IMapper

The identifier query relies on `IMapper<TSource, TDestination>` to convert between entities and read models:

### Entity Framework Mapping

```csharp
// Projects directly from IQueryable to avoid loading entity into memory
var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);
return await projected.FirstOrDefaultAsync(cancellationToken);
```

### MongoDB Mapping

```csharp
// Loads entity first, then maps to read model
var entity = await Repository.FindAsync(request.Id, cancellationToken);
return Mapper.Map<TEntity, TReadModel>(entity);
```

## Pipeline Behaviors

The identifier query automatically includes several pipeline behaviors:

- **Tenant Security**: `TenantAuthenticateQueryBehavior` (if read model implements `IHaveTenant<TKey>`)
  - Validates that the user has access to the specified tenant
  - Ensures tenant isolation for multi-tenant applications

- **Soft Delete Filtering**: `DeletedFilterQueryBehavior` (if read model implements `ITrackDeleted`)
  - Automatically filters out soft-deleted entities from query results
  - Respects the `IsDeleted` flag on entities

- **Caching**: `MemoryCacheQueryBehavior` or `HybridCacheQueryBehavior`
  - Automatically caches query results based on the cache key
  - Respects cache expiration policies set on the query
  - Handles cache invalidation using cache tags

## Cache Configuration

Configure caching policies on your queries:

### Sliding Expiration

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute sliding expiration

var result = await mediator.Send(query);
```

### Absolute Expiration

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // Expires at specific time

var result = await mediator.Send(query);
```

### Memory Cache Registration

```csharp
services.AddEntityQueryMemoryCache<int, ProductReadModel>();
```

### Hybrid Cache Registration

```csharp
services.AddEntityHybridCache<int, ProductReadModel>();
```

## Usage Examples

### Basic Usage

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);

var result = await mediator.Send(query);
Console.WriteLine($"Product Name: {result?.Name}");
```

### In ASP.NET Core Controller

```csharp
[HttpGet("{id:int}")]
public async Task<ProductReadModel?> GetProduct(int id)
{
    var query = new EntityIdentifierQuery<int, ProductReadModel>(User, id);
    return await mediator.Send(query);
}
```

### In Minimal API Endpoint

```csharp
app.MapGet("/products/{id:int}", async (
    [FromServices] IMediator mediator,
    [FromRoute] int id,
    ClaimsPrincipal user) =>
{
    var query = new EntityIdentifierQuery<int, ProductReadModel>(user, id);
    var result = await mediator.Send(query);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});
```

### With Caching

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);

// Configure 30-minute sliding cache
query.Cache(TimeSpan.FromMinutes(30));

var result = await mediator.Send(query);
```

## Return Values

- **Success**: Returns the `TReadModel` representing the found entity
- **Not Found**: Returns `null` if no entity with the specified identifier exists
- **Exception**: Throws appropriate exceptions for validation or data access errors

## Error Handling

The query handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the id parameter is null
- **`DomainException`**: For business rule violations
- **Database exceptions**: For data access errors
- **Authorization exceptions**: When user lacks access to the entity

## Best Practices

1. **Cache Appropriately**: Use caching for frequently accessed entities with reasonable expiration times
2. **Security**: Always pass the current user's `ClaimsPrincipal` for proper authorization
3. **Key Types**: Use appropriate key types (`int`, `Guid`, `string`) based on your domain
4. **Null Handling**: The query guarantees the id is not null, but the result may be null if entity doesn't exist
5. **Mapping Configuration**: Configure efficient mapping between entities and read models
6. **Projection**: Entity Framework handler uses projection to avoid loading unnecessary data
7. **Tenant Isolation**: Implement `IHaveTenant<TKey>` on read models for multi-tenant scenarios

## Performance Considerations

1. **Entity Framework**: Uses `ProjectTo` for efficient database projection
2. **MongoDB**: Uses `FindAsync` for optimal single-entity retrieval
3. **Caching**: Implement appropriate cache expiration to balance performance and data freshness
4. **Indexing**: Ensure proper database indexes on identifier columns
5. **Read Models**: Design lean read models with only necessary properties
