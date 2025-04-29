# Query Abstract Classes

Abstract base classes for queries

## `PrincipalQueryBase` Class

A base query type using the specified `ClaimsPrincipal`.

```c#
public abstract record PrincipalQueryBase<TResponse>(ClaimsPrincipal? principal)
```

### `PrincipalQueryBase` Type Parameters

`TResponse`

The type of the response.

### `PrincipalQueryBase` Constructor Parameters

`principal ClaimsPrincipal`

The `ClaimsPrincipal` this query is run for

## `CacheableQueryBase` Class

A base cacheable query type using the specified `ClaimsPrincipal`.

```c#
public abstract record CacheableQueryBase<TResponse>(ClaimsPrincipal? principal)
```

### `CacheableQueryBase` Type Parameters

`TResponse`

The type of the response.

### `CacheableQueryBase` Constructor Parameters

`principal ClaimsPrincipal`

The `ClaimsPrincipal` this query is run for
