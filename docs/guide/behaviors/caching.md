---
title: Caching Behaviors
description: Pipeline behaviors that implement intelligent caching strategies to improve query performance and reduce database load
---

# Caching Behaviors

Pipeline behaviors that implement intelligent caching strategies to improve query performance and reduce database load. These behaviors work with queries that inherit from `CacheableQueryBase<TResponse>` to provide automatic caching capabilities.

## Overview

The Arbiter framework provides the `HybridCacheQueryBehavior` implementation that works with queries that inherit from `CacheableQueryBase`. This behavior combines both in-memory and distributed caching for optimal performance and scalability.

The hybrid cache behavior works automatically with cacheable queries without requiring additional configuration in the query implementation.

## HybridCacheQueryBehavior

The `HybridCacheQueryBehavior<TRequest, TResponse>` behavior combines both in-memory and distributed caching for optimal performance and scalability. It implements a multi-level caching strategy that provides the best of both worlds.

### Hybrid Cache Characteristics

- **Fast local memory cache with distributed fallback**
- **Automatic tier management**
- **Best of both worlds: speed + scalability**
- **Recommended for production environments**

### Hybrid Cache Flow

1. **L1 Check**: First checks local memory cache
2. **L2 Check**: If L1 miss, checks distributed cache
3. **Database Query**: If both miss, executes query against database
4. **Cache Population**: Stores result in both L1 and L2 caches
5. **Subsequent Requests**: Served from L1 for maximum speed

### Hybrid Cache Use Cases

- **Production Environments**: When you need robust caching with fallback
- **High-Performance Requirements**: Need both speed and scalability
- **Mixed Access Patterns**: Some data accessed frequently, some occasionally
- **Load-Balanced Applications**: Multiple instances with shared cache needs

### Hybrid Cache Registration

```csharp
// Register hybrid cache with Redis backing
services.AddHybridCache()
    .AddRedisDistributedCache(options =>
    {
        options.Configuration = "localhost:6379";
        options.InstanceName = "MyApp";
    });

services.AddEntityHybridCache();
```

### Hybrid Cache Usage

```csharp
public record GetOrderHistoryQuery : CacheableQueryBase<IReadOnlyCollection<OrderReadModel>>
{
    public GetOrderHistoryQuery(ClaimsPrincipal principal, int customerId)
        : base(principal)
    {
        CustomerId = customerId;
    }

    public int CustomerId { get; }

    public override string GetCacheKey()
    {
        var userId = Principal?.GetUserId();
        return $"Orders:Customer:{CustomerId}:User:{userId}";
    }

    public override string? GetCacheTag() => $"Orders Customer:{CustomerId}";
}

// Usage
var query = new GetOrderHistoryQuery(principal, 456);
query.Cache(TimeSpan.FromMinutes(30)); // 30-minute expiration

var orders = await mediator.Send(query);
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

### Manual Invalidation with ICacheExpire

For custom invalidation scenarios, implement `ICacheExpire` in your commands:

```csharp
public record CustomProductCommand : PrincipalCommandBase<ProductReadModel>, ICacheExpire
{
    public CustomProductCommand(ClaimsPrincipal principal) : base(principal) { }
    
    public string? GetCacheTag() => "Products";
}
```

### Tag-Based Invalidation

Cache behaviors use the cache tags defined in your queries for efficient invalidation:

```csharp
// Query with cache tag
public override string? GetCacheTag() => $"Products Category:{CategoryId}";

// Command that invalidates all products in a category  
public string? GetCacheTag() => $"Products Category:{CategoryId}";
```

## Configuration Options

### Hybrid Cache Configuration

```csharp
services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
    options.MaximumPayloadBytes = 1024 * 1024; // 1MB max payload
    options.MaximumKeyLength = 1024; // 1KB max key length
});
```

## Best Practices

### Cache Strategy Selection

The `HybridCacheQueryBehavior` provides the optimal caching strategy by combining both in-memory and distributed caching capabilities. This unified approach offers:

- **Fast local access** for frequently used data
- **Distributed sharing** across multiple application instances  
- **Automatic tier management** between local and distributed caches
- **Production-ready performance** for all deployment scenarios

### Query Design Guidelines

1. **Inherit from CacheableQueryBase**: Ensures proper cache behavior integration
2. **Implement meaningful cache keys**: Use `GetCacheKey()` to create unique, descriptive keys
3. **Use cache tags effectively**: Implement `GetCacheTag()` for efficient invalidation

```csharp
public record GetDashboardDataQuery : CacheableQueryBase<DashboardReadModel>
{
    public GetDashboardDataQuery(ClaimsPrincipal principal, string scope)
        : base(principal)
    {
        Scope = scope;
    }

    public string Scope { get; }

    // Good cache key: includes user context and parameters
    public override string GetCacheKey()
    {
        var userId = Principal?.GetUserId();
        return $"Dashboard:Scope:{Scope}:User:{userId}";
    }

    // Effective cache tag for invalidation
    public override string? GetCacheTag() => $"Dashboard User:{Principal?.GetUserId()}";
}
```

### Service Configuration Best Practices

1. **Configure hybrid cache for all environments**
2. **Set up appropriate cache providers and connection settings**
3. **Register cache invalidation for data modification commands**
4. **Monitor cache performance and hit rates**

```csharp
// Production-ready configuration
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

// Register caching with automatic invalidation
services.AddEntityHybridCache();
```

### Performance Guidelines

1. **Cache Key Design**: Keep keys short but descriptive, include relevant parameters
2. **Expiration Strategies**: Balance performance gains with data freshness requirements  
3. **Memory Management**: Monitor cache memory usage and set appropriate limits
4. **Serialization Efficiency**: Use efficient serialization for distributed caching

### Security Considerations

1. **Sensitive Data**: Avoid caching sensitive information like passwords or personal data
2. **Access Control**: Ensure cached data respects user permissions and tenant isolation
3. **Cache Key Security**: Use non-predictable cache keys to prevent enumeration attacks
4. **Encryption**: Consider encrypting cached data in distributed scenarios
