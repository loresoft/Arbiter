# Identifier Query

Represents a query for retrieving a single entity identified by a specific key. The result of the query will be of type `TReadModel`.

This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a single entity based on its unique identifier. It supports caching to optimize repeated queries for the same entity.

```c#
public record EntityIdentifierQuery<TKey, TReadModel>(
    ClaimsPrincipal? principal, 
    TKey id
)
```

## Type Parameters

`TKey`

The type of the key used to identify the entity.

`TReadModel`

The type of the read model returned as the result of the query.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the query.

`id TKey`

The identifier of the entity to retrieve.

## Examples

The following example demonstrates how to use the `EntityIdentifierQuery<TKey, TReadModel>`:

```c#
// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);

// Send the query to the mediator instance
var result = await mediator.Send(query);
Console.WriteLine($"Product Name: {result?.Name}");
```
