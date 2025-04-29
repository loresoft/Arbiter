# Identifiers Query

Represents a query for retrieving multiple entities identified by a list of keys. The result of the query will be a collection of type `TReadModel`.

This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve multiple entities based on their unique identifiers. It supports caching to optimize repeated queries for the same set of entities.

```c#
public record EntityIdentifiersQuery<TKey, TReadModel>(
    ClaimsPrincipal? principal, 
    IReadOnlyCollection<TKey> ids
)
```

## Type Parameters

`TKey`

The type of the keys used to identify the entities.

`TReadModel`

The type of the read model returned as the result of the query.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the query.

`ids IReadOnlyCollection<TKey>`

The list of identifiers for the entities to retrieve.

## Examples

The following example demonstrates how to use the `EntityIdentifiersQuery<TKey, TReadModel>`:

```c#
// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var ids = new List<int> { 1, 2, 3 };
var query = new EntityIdentifiersQuery<int, ProductReadModel>(principal, ids);

// Send the query to the mediator instance
var result = await mediator.Send(query);
Console.WriteLine($"Retrieved {result?.Count} products.");
```
