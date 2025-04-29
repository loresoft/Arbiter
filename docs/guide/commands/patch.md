# Patch Command

Represents a command to apply a JSON patch to an entity identified by a specific key. The result of the command will be of type `TReadModel`.

This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to apply partial updates to an entity using a JSON patch document.

```c#
public record EntityPatchCommand<TKey, TReadModel>(
    ClaimsPrincipal? principal, 
    TKey id, 
    JsonPatchDocument patch
)
```

## Type Parameters

`TKey`

The type of the key used to identify the entity.

`TReadModel`

The type of the read model returned as the result of the command.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`id TKey`

The identifier of the entity to which the JSON patch will be applied.

`patch JsonPatchDocument`

The JSON patch document containing the updates to apply.

## Examples

The following example demonstrates how to use the `EntityPatchCommand<TKey, TReadModel>`:

```c#
var patchDocument = new JsonPatchDocument();
patchDocument.Replace("/Name", "Updated Name");

// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var command = new EntityPatchCommand<int, ProductReadModel>(principal, 123, patchDocument);

// Pass the command to a handler or mediator
var result = await mediator.Send(command);
Console.WriteLine($"Updated product name: {result?.Name}");
```
