# Create Command

Represents a command to create a new entity using the specified `TCreateModel`. The result of the command will be of type `TReadModel`.

This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to create a new entity and return a read model representing the created entity or a related result.

```c#
public record EntityCreateCommand<TCreateModel, TReadModel>(
    ClaimsPrincipal? principal, 
    TCreateModel model
)
```

## Type Parameters

`TCreateModel`

The type of the create model used to provide data for the new entity.

`TReadModel`

The type of the read model returned as the result of the command.

## Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user for whom this command is executed.

`model TCreateModel`

The create model containing the data for the new entity.

## Examples

The following example demonstrates how to use the `EntityCreateCommand<TCreateModel, TReadModel>`:

```c#
var createModel = new ProductCreateModel
{
    Name = "New Product",
    Description = "A description of the new product",
    Price = 19.99m
};

// sample user claims, usually gotten from controller context or equivalent
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));

var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);

// Pass the command to the mediator for execution
var result = await mediator.Send(command);
Console.WriteLine($"Created product: {result?.Name}");
```
