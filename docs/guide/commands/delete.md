# Delete Command

A command to delete an entity based on the specified identifier. `TReadModel` represents the result of the command.

This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to delete an entity and optionally return a read model representing the deleted entity or a related result.

```c#
public record EntityDeleteCommand<TKey, TReadModel>(
    ClaimsPrincipal? principal, 
    TKey id
)
```

## Type Parameters

`TKey`

The type of the key used to identify the entity.

`TReadModel`

The type of the read model returned after the command execution.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`id TKey`

The identifier of the entity to be deleted.

## Examples

The following example demonstrates how to use the `EntityDeleteCommand<TKey, TReadModel>`:

```c#
// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var command = new EntityDeleteCommand<int, ProductReadModel>(principal, 123);

// Pass the command to a handler or mediator
var result = await mediator.Send(command);
```
