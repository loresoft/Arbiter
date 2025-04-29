# Update Command

Represents a command to update an entity identified by a specific key using the provided update model. The result of the command will be of type `TReadModel`.

This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to update an entity and return a read model representing the updated entity or a related result.

```c#
public record EntityUpdateCommand<TKey, TUpdateModel, TReadModel>(
    ClaimsPrincipal? principal, 
    TKey id, 
    TUpdateModel model
)
```

## Type Parameters

`TKey`

The type of the key used to identify the entity.

`TUpdateModel`

The type of the update model containing the data for the update.

`TReadModel`

The type of the read model returned as the result of the command.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`id TKey`

The identifier of the entity to update.

`model TUpdateModel`

The update model containing the data for the update.

## Examples

The following example demonstrates how to use the `EntityUpdateCommand<TKey, TUpdateModel, TReadModel>`:

```c#
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product",
    Description = "Updated description of the product",
    Price = 29.99m
};

// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, 123, updateModel);

// Pass the command to a handler or mediator
var result = await mediator.Send(command);
Console.WriteLine($"Updated product name: {result?.Name}");
```
