---
title: Caching Behaviors
description: Pipeline behaviors that implement intelligent caching strategies to improve query performance and reduce database load
---

# Caching Behaviors

Pipeline behaviors that implement intelligent caching strategies to improve query performance and reduce database load. These behaviors work with queries that inherit from `CacheableQueryBase<TResponse>` to provide automatic caching capabilities.

## Overview

The Arbiter framework provides three cache behavior implementations that can be used independently or in combination with queries that inherit from `CacheableQueryBase`:

- **MemoryCacheQueryBehavior** - Fast in-memory caching for single application instances
- **DistributedCacheQueryBehavior** - Shared cache across multiple application instances
- **HybridCacheQueryBehavior** - Combined local and distributed caching for optimal performance

All cache behaviors work automatically with cacheable queries without requiring additional configuration in the query implementation.

## MemoryCacheQueryBehavior

The `MemoryCacheQueryBehavior<TRequest, TResponse>` behavior caches query results in local memory for ultra-fast retrieval and reduced database load.

### Memory Cache Characteristics

- **Fastest cache access** (in-process memory)
- **Limited to single application instance**
- **Automatically cleared on application restart**
- **Best for frequently accessed data with small memory footprint**

### Memory Cache Use Cases

- **Single Instance Deployments**: When you don't need to share cache across instances
- **Frequently Accessed Data**: Reference data, lookups, configurations that fit in memory
- **Low Latency Requirements**: When sub-millisecond response times are required
- **Development and Testing**: Simple setup without external dependencies

### Memory Cache Registration

```csharp
// Register memory cache and cache behavior
services.AddMemoryCache();
services.AddEntityMemoryCache();
```

### Memory Cache Usage

```csharp
public record GetUserByIdQuery : CacheableQueryBase<UserReadModel>
{
    public GetUserByIdQuery(ClaimsPrincipal principal, int userId)
        : base(principal)
    {
        UserId = userId;
    }

    public int UserId { get; }

    public override string GetCacheKey() => $"User:Id:{UserId}";
    public override string? GetCacheTag() => "Users";
}

// Usage
var query = new GetUserByIdQuery(principal, 123);
query.Cache(TimeSpan.FromMinutes(15)); // 15-minute sliding expiration

var user = await mediator.Send(query);
```

## DistributedCacheQueryBehavior

The `DistributedCacheQueryBehavior<TRequest, TResponse>` behavior caches query results in a distributed cache (Redis, SQL Server, etc.) for sharing data across multiple application instances and providing persistence across application restarts.

### Distributed Cache Characteristics

- **Shared across multiple application instances**
- **Persists across application restarts**
- **Requires serialization/deserialization**
- **Best for scaled-out environments**

### Distributed Cache Use Cases

- **Multi-Instance Deployments**: Microservices, load-balanced applications
- **Cloud Environments**: Azure, AWS, or other cloud platforms  
- **Horizontal Scaling**: When cache needs to be shared across instances
- **Data Persistence**: When cache should survive application restarts

### Distributed Cache Registration

```csharp
// Register distributed cache with Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "MyApp";
});
services.AddEntityDistributedCache();

// Or with SQL Server
services.AddSqlServerCache(options =>
{
    options.ConnectionString = connectionString;
    options.SchemaName = "dbo";
    options.TableName = "CacheEntries";
});
services.AddEntityDistributedCache();
```

### Distributed Cache Usage

```csharp
public record GetProductsQuery : CacheableQueryBase<IReadOnlyCollection<ProductReadModel>>
{
    public GetProductsQuery(ClaimsPrincipal principal, string category)
        : base(principal)
    {
        Category = category;
    }

    public string Category { get; }

    public override string GetCacheKey() => $"Products:Category:{Category}";
    public override string? GetCacheTag() => "Products";
}

// Usage
var query = new GetProductsQuery(principal, "Electronics");
query.Cache(DateTimeOffset.UtcNow.AddHours(1)); // 1-hour absolute expiration

var products = await mediator.Send(query);
```

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
query.Cache(TimeSpan.FromMinutes(30)); // 30-minute sliding expiration

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

### Memory Cache Configuration

```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Maximum number of entries
    options.CompactionPercentage = 0.25; // Remove 25% when limit reached
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});
```

### Distributed Cache Configuration

```csharp
// Redis configuration
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionString;
    options.ConfigurationOptions = new ConfigurationOptions
    {
        AbortOnConnectFail = false, // Don't crash if Redis is down
        ConnectTimeout = 5000,      // 5 second connection timeout
        SyncTimeout = 1000,         // 1 second operation timeout
    };
});
```

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

1. **Memory Cache**: Use for small, frequently accessed, read-only data in single-instance deployments
2. **Distributed Cache**: Use for larger datasets in multi-instance scenarios  
3. **Hybrid Cache**: Use for production applications requiring both speed and scale

### Query Design Guidelines

1. **Inherit from CacheableQueryBase**: Ensures proper cache behavior integration
2. **Implement meaningful cache keys**: Use `GetCacheKey()` to create unique, descriptive keys
3. **Use cache tags effectively**: Implement `GetCacheTag()` for efficient invalidation
4. **Configure appropriate expiration**: Use sliding for frequently accessed data, absolute for time-sensitive data

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

1. **Use hybrid cache for production environments**
2. **Configure appropriate cache providers and connection settings**
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
