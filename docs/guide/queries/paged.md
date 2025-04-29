# Paged Query

Represents a query for retrieving paged entities based on an `EntityQuery`. The result of the query will be of type `EntityPagedResult<TReadModel>`.

This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve entities in a paginated format. The `EntityQuery` allows filtering, sorting, and pagination criteria to be specified.

```c#
public record EntityPagedQuery<TReadModel>(
    ClaimsPrincipal? principal, 
    EntityQuery? query
)
```

## Type Parameters

`TReadModel`

The type of the read model returned as the result of the query.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the query.

`query EntityQuery`

The EntityQuery defining the filter, sort, and pagination criteria for the query.

## Examples

The following example demonstrates how to use the `EntityPagedQuery<TReadModel>`:

```c#
// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var entityQuery = new EntityQuery
{
    Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
    Sort = new List<EntitySort> { new EntitySort { Name = "Name", Direction = "asc" } },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);

// Send the query to the mediator instance
var result = await mediator.Send(query);
Console.WriteLine($"Total Results: {result?.Total}");
```
