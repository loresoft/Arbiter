---
title: Query Result Caching
description: Powerful caching capabilities to improve query performance and reduce database load
---

# Query Result Caching

The Arbiter framework provides powerful caching capabilities to improve query performance and reduce database load. The caching system supports multiple cache providers, configurable expiration policies, and automatic cache invalidation strategies.

## Overview

The caching system consists of several key components:

- [`CacheableQueryBase<TResponse>`](#cacheablequerybase) - Base class for cacheable queries with expiration policies
- [Cache Behaviors](#cache-behaviors) - Pipeline behaviors that handle cache operations
- [Cache Providers](#cache-providers) - Memory, distributed, and hybrid cache support
- [Cache Configuration](#cache-configuration) - Expiration and invalidation strategies
- [Service Registration](#service-registration) - Extension methods for setup

## CacheableQueryBase

The `CacheableQueryBase<TResponse>` class provides the foundation for cacheable queries. It extends `PrincipalQueryBase` with caching capabilities including cache key generation, expiration policies, and cache tagging.

### Key Features

- **Cache Key Generation**: Abstract method requiring unique cache key implementation
- **Cache Tagging**: Virtual method for cache invalidation strategies  
- **Expiration Control**: Support for both absolute and sliding expiration policies
- **Cache Behavior Integration**: Compatible with all Arbiter cache behaviors

### Creating a Cacheable Query

```csharp
public record GetProductByIdQuery : CacheableQueryBase<ProductReadModel>
{
    public GetProductByIdQuery(ClaimsPrincipal principal, int productId)
        : base(principal)
    {
        ProductId = productId;
    }

    public int ProductId { get; }

    // Required: Unique cache key for this query
    public override string GetCacheKey()
        => $"Product:Id:{ProductId}";

    // Optional: Cache tag for invalidation
    public override string? GetCacheTag()
        => "Products";
}
```

### Cache Control Methods

The `CacheableQueryBase` provides methods to configure cache expiration:

```csharp
// Sliding expiration - cache expires after period of inactivity
query.Cache(TimeSpan.FromMinutes(15));

// Absolute expiration - cache expires at specific time
query.Cache(DateTimeOffset.UtcNow.AddHours(1));

// Check if query is cacheable (has expiration configured)
bool isCacheable = query.IsCacheable();
```

## Cache Behaviors

Arbiter provides three cache behavior implementations that can be used independently or in combination:

### MemoryCacheQueryBehavior

Caches query results in local memory for fast access within a single application instance.

**Characteristics:**

- Fastest cache access (in-process memory)
- Limited to single application instance
- Automatically cleared on application restart
- Best for frequently accessed data with small memory footprint

### DistributedCacheQueryBehavior

Caches query results in a distributed cache (Redis, SQL Server, etc.) for sharing across multiple application instances.

**Characteristics:**

- Shared across multiple application instances
- Persists across application restarts
- Requires serialization/deserialization
- Best for scaled-out environments

### HybridCacheQueryBehavior

Combines both in-memory and distributed caching for optimal performance and scalability.

**Characteristics:**

- Fast local memory cache with distributed fallback
- Automatic tier management
- Best of both worlds: speed + scalability
- Recommended for production environments

## Cache Providers

### Memory Cache

Uses `IMemoryCache` for in-process caching:

```csharp
// Service registration
services.AddMemoryCache();
services.AddEntityMemoryCache();

// Usage
var query = new GetProductByIdQuery(principal, 123);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute sliding expiration

var result = await mediator.Send(query);
```

### Distributed Cache

Uses `IDistributedCache` for distributed caching:

```csharp
// Service registration with Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
services.AddEntityDistributedCache();

// Usage
var query = new GetProductByIdQuery(principal, 123);
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // Absolute expiration

var result = await mediator.Send(query);
```

### Hybrid Cache

Uses `HybridCache` for combined local and distributed caching:

```csharp
// Service registration with Redis backing
services.AddHybridCache()
    .AddRedisDistributedCache(options =>
    {
        options.Configuration = "localhost:6379";
    });
services.AddEntityHybridCache();

// Usage
var query = new GetProductByIdQuery(principal, 123);
query.Cache(TimeSpan.FromMinutes(30)); // 30-minute sliding expiration

var result = await mediator.Send(query);
```

## Cache Configuration

### Sliding Expiration

Cache entries expire after a period of inactivity. Each cache access resets the expiration timer.

```csharp
// Cache for 15 minutes of inactivity
query.Cache(TimeSpan.FromMinutes(15));

// Cache for 2 hours of inactivity
query.Cache(TimeSpan.FromHours(2));
```

**Best for**: Frequently accessed data that should expire when not being used

### Absolute Expiration

Cache entries expire at a specific date and time, regardless of access patterns.

```csharp
// Expire at end of day
query.Cache(DateTime.Today.AddDays(1));

// Expire in 1 hour
query.Cache(DateTimeOffset.UtcNow.AddHours(1));

// Expire at specific time
query.Cache(new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero));
```

**Best for**: Time-sensitive data that must be refreshed at specific intervals

### Cache Key Strategies

Implement effective cache key generation:

```csharp
public override string GetCacheKey()
{
    // Include user context for user-specific data
    var userId = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return $"UserProfile:Id:{UserId}:User:{userId}";
}

// For public data, omit user context
public override string GetCacheKey()
    => $"Product:Category:{CategoryId}:Page:{Page}:PageSize:{PageSize}";

// Include relevant parameters that affect results
public override string GetCacheKey()
{
    var filterHash = Filter?.GetHashCode() ?? 0;
    var sortHash = Sort?.GetHashCode() ?? 0;
    return $"Products:Filter:{filterHash}:Sort:{sortHash}:Page:{Page}";
}
```

### Cache Tagging

Use cache tags for efficient invalidation:

```csharp
public override string? GetCacheTag()
    => "Products"; // All product-related cache entries

// Multiple tags (space-separated)
public override string? GetCacheTag()
    => $"Products Category:{CategoryId}";

// Hierarchical tags
public override string? GetCacheTag()
    => $"Organization:{OrganizationId} Department:{DepartmentId}";
```

## Service Registration

### Memory Cache Only

Register memory cache behaviors for all entity queries:

```csharp
services.AddMemoryCache();
services.AddEntityMemoryCache();
```

### Distributed Cache Only

Register distributed cache behaviors with a cache provider:

```csharp
// With Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionString;
});
services.AddEntityDistributedCache();

// With SQL Server
services.AddSqlServerCache(options =>
{
    options.ConnectionString = connectionString;
    options.SchemaName = "dbo";
    options.TableName = "CacheEntries";
});
services.AddEntityDistributedCache();
```

### Hybrid Cache (Recommended)

Register hybrid cache with both memory and distributed backing:

```csharp
services.AddHybridCache()
    .AddRedisDistributedCache(options =>
    {
        options.Configuration = connectionString;
    });
services.AddEntityHybridCache();
```

## Entity Query Examples

### Identifier Query with Caching

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, productId);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute sliding expiration

var product = await mediator.Send(query);
```

### Paged Query with Caching

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Category", Value = "Electronics" },
    Sort = new List<EntitySort> { new() { Name = "Name", Direction = "asc" } },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
query.Cache(TimeSpan.FromMinutes(10)); // 10-minute sliding expiration

var result = await mediator.Send(query);
```

### Select Query with Caching

```csharp
var filter = new EntityFilter 
{ 
    Name = "Status", 
    Operator = EntityFilterOperators.Equal, 
    Value = "Active" 
};

var query = new EntitySelectQuery<ProductReadModel>(principal, filter);
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // 1-hour absolute expiration

var products = await mediator.Send(query);
```

## Cache Invalidation

### Automatic Invalidation

When using the full cache registration (with create/update models), cache entries are automatically invalidated when related commands are executed:

```csharp
// This registration includes automatic invalidation
services.AddEntityHybridCache();

// Create command will automatically invalidate product cache entries
var createCommand = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);
await mediator.Send(createCommand); // Cache automatically cleared
```

### Manual Invalidation

For custom invalidation scenarios, implement `ICacheExpire` in your commands:

```csharp
public record CustomProductCommand : PrincipalCommandBase<ProductReadModel>, ICacheExpire
{
    public CustomProductCommand(ClaimsPrincipal principal) : base(principal) { }
    
    public string? GetCacheTag() => "Products";
}
```

### Tag-Based Invalidation

Use cache tags to invalidate related cache entries efficiently:

```csharp
// Query with cache tag
public override string? GetCacheTag() => $"Products Category:{CategoryId}";

// Command that invalidates all products in a category
public string? GetCacheTag() => $"Products Category:{CategoryId}";
```

## Performance Considerations

### Cache Key Design

1. **Keep keys short but descriptive**: Long keys consume more memory
2. **Include relevant parameters**: Ensure cache keys differentiate between different result sets
3. **Use consistent formatting**: Establish naming conventions for maintainability
4. **Avoid user-specific keys for public data**: Improves cache hit rates

```csharp
// Good: Concise and descriptive
public override string GetCacheKey() => $"Product:{ProductId}";

// Better: Includes user context when needed
public override string GetCacheKey()
{
    var userId = Principal?.GetUserId();
    return $"UserProduct:{ProductId}:User:{userId}";
}

// Best: Uses standardized format
public override string GetCacheKey() 
    => CacheKeyBuilder.Create("Product")
        .WithId(ProductId)
        .WithUser(Principal)
        .Build();
```

### Expiration Strategies

1. **Use sliding expiration for frequently accessed data**
2. **Use absolute expiration for time-sensitive data**
3. **Set reasonable expiration times**: Balance performance vs. data freshness
4. **Consider cache warming for critical data**

```csharp
// Frequently accessed reference data
query.Cache(TimeSpan.FromHours(1)); // 1-hour sliding

// Daily reports
query.Cache(DateTime.Today.AddDays(1)); // Absolute daily expiration

// Real-time data
query.Cache(TimeSpan.FromMinutes(5)); // 5-minute sliding
```

### Memory Management

1. **Monitor cache memory usage**: Set appropriate size limits
2. **Use distributed cache for large result sets**: Avoid memory pressure
3. **Implement cache compression**: For distributed cache serialization
4. **Profile cache hit rates**: Optimize key strategies and expiration times

```csharp
// Configure memory cache limits
services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Maximum number of entries
});

// Configure hybrid cache options
services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024; // 1MB max payload
    options.MaximumKeyLength = 1024; // 1KB max key length
});
```

## Best Practices

### Query Design

1. **Inherit from CacheableQueryBase for cacheable queries**
2. **Implement meaningful cache keys and tags**
3. **Configure appropriate expiration policies**
4. **Test cache behavior in development**

```csharp
public record GetProductsQuery : CacheableQueryBase<IReadOnlyCollection<ProductReadModel>>
{
    public GetProductsQuery(ClaimsPrincipal principal, string category)
        : base(principal)
    {
        Category = category;
    }

    public string Category { get; }

    public override string GetCacheKey() 
        => $"Products:Category:{Category}";

    public override string? GetCacheTag() 
        => "Products";
}
```

### Service Configuration

1. **Use hybrid cache for production environments**
2. **Configure appropriate cache providers**
3. **Register cache invalidation for data modification commands**
4. **Monitor cache performance metrics**

```csharp
// Production configuration
services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
})
.AddRedisDistributedCache(options =>
{
    options.Configuration = connectionString;
    options.InstanceName = "MyApp";
});

// Register caching with invalidation
services.AddEntityHybridCache();
```
