# Caching Behavior

A behavior for caching the response of a query.

## MemoryCacheQueryBehavior

The `MemoryCacheQueryBehavior` is a behavior that caches the results of queries in memory to improve performance and reduce redundant data retrieval operations. When this behavior is applied, it stores the response of a query in an in-memory cache, so subsequent requests for the same data can be served quickly without re-executing the query. This is especially useful for frequently accessed or expensive queries, as it helps minimize database load and enhances application responsiveness.

### Registering with AddEntityQueryMemoryCache

To use `MemoryCacheQueryBehavior`, register it in your service configuration using the `AddEntityQueryMemoryCache` extension method. This ensures that query responses for your entities are cached in memory.

## DistributedCacheQueryBehavior

The `DistributedCacheQueryBehavior` is a behavior that caches the results of queries using a distributed cache, such as Redis or SQL Server, to improve performance and enable cache sharing across multiple application instances. When this behavior is applied, it stores the response of a query in a distributed cache, allowing subsequent requests for the same data to be served quickly without re-executing the query, even if the requests come from different servers. This is especially useful in cloud or scaled-out environments, as it helps reduce database load and ensures consistent, fast access to frequently requested data across your application.

### Registering with AddEntityQueryDistributedCache

To use `DistributedCacheQueryBehavior`, register it in your service configuration using the `AddEntityQueryDistributedCache` extension method. This ensures that query responses for your entities are cached in a distributed cache.

## HybridCacheQueryBehavior

The `HybridCacheQueryBehavior` is a behavior that combines both in-memory and distributed caching strategies to optimize query performance and scalability. When this behavior is applied, it first attempts to retrieve query results from the in-memory cache for the fastest access. If the data is not found in memory, it checks the distributed cache (such as Redis or SQL Server). If the data is still not found, the query is executed and the result is stored in both caches. This approach provides the low-latency benefits of in-memory caching for frequently accessed data, while also ensuring consistency and availability across multiple application instances through the distributed cache.

### Registering with AddEntityHybridCache

To use `HybridCacheQueryBehavior`, register it in your service configuration using the `AddEntityHybridCache` extension method. This ensures that query responses for your entities are cached using both in-memory and distributed caches for optimal performance and scalability.
