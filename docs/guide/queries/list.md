# List Query

Represents a query for selecting entities based on an `EntitySelect`. The result of the query will be a collection of type `TReadModel`.

This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a collection of entities based on filtering and sorting criteria defined in an `EntitySelect`.

```c#
public record EntitySelectQuery<TReadModel>(
    ClaimsPrincipal? principal, 
    EntitySelect? select
)
```

## Type Parameters

`TReadModel`

The type of the read model returned as the result of the query.

## Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the query.

`select EntitySelect`

The EntitySelect defining the filter and sort criteria for the query.

## Examples

The following example demonstrates how to use the `EntitySelectQuery<TReadModel>`:

```C#
// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var filter = new EntityFilter
{
    Name = "Status",
    Operator = "eq",
    Value = "Active"
};
var sort = new EntitySort
{
    Name = "Name",
    Direction = "asc"
};
var select = new EntitySelect(filter, sort);

var query = new EntitySelectQuery<ProductReadModel>(principal, select);

// Send the query to the mediator instance
var result = await mediator.Send(query);
Console.WriteLine($"Retrieved {result?.Count} entities.");
```
