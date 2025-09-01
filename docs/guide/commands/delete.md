---
title: Delete Command
description: Command to delete an entity by its identifier
---

# Delete Command

The `EntityDeleteCommand<TKey, TReadModel>` represents a command to delete an entity by its identifier. This command follows the CQRS (Command Query Responsibility Segregation) pattern and returns a read model representing the deleted entity.

## Overview

The delete command is a fundamental part of the Arbiter framework's CRUD operations. It inherits from `EntityIdentifierCommand<TKey, TReadModel>` which provides automatic security context, audit tracking, and JSON serialization support.

```csharp
public record EntityDeleteCommand<TKey, TReadModel>
    : EntityIdentifierCommand<TKey, TReadModel>, ICacheExpire
```

## Key Features

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Soft Delete Support**: Automatic soft delete when entity implements `ITrackDeleted` interface
- **Hard Delete Support**: Physical deletion when entity doesn't support soft delete
- **Audit Tracking**: Automatic update metadata tracking before deletion
- **Cache Integration**: Implements `ICacheExpire` for automatic cache invalidation
- **History Tracking**: Special handling for entities that implement `ITrackHistory`
- **Entity Validation**: Validates entity exists before attempting deletion

## Type Parameters

| Parameter    | Description                                                         |
| ------------ | ------------------------------------------------------------------- |
| `TKey`       | The type of the key used to identify the entity                     |
| `TReadModel` | The type of the read model returned representing the deleted entity |

## Constructor Parameters

| Parameter   | Type               | Description                                                            |
| ----------- | ------------------ | ---------------------------------------------------------------------- |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization |
| `id`        | `TKey`             | The identifier of the entity to be deleted                             |

## Delete Behavior Types

### Soft Delete

When an entity implements `ITrackDeleted`, the command performs a soft delete:

- Sets `IsDeleted = true`
- Updates audit metadata (`Updated`, `UpdatedBy`)
- Entity remains in database but marked as deleted

### Hard Delete

When an entity doesn't implement `ITrackDeleted`, the command performs a hard delete:

- Physically removes the entity from the database
- Updates audit metadata before deletion (if entity supports `ITrackUpdated`)
- Special handling for history tracking entities

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityDeleteCommandHandler<TContext, TEntity, TKey, TReadModel>
```

### MongoDB Handler

```csharp
EntityDeleteCommandHandler<TRepository, TEntity, TKey, TReadModel>
```

## Service Registration

Register delete command support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register delete command individually
services.AddEntityDeleteCommand<MyDbContext, Product, int, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register delete command individually  
services.AddEntityDeleteCommand<IProductRepository, Product, int, ProductReadModel>();
```

## Model Mapping with IMapper

The delete command relies on `IMapper<TSource, TDestination>` to convert the deleted entity into a read model for the response:

### Entity to ReadModel Mapping

After the entity is deleted (or marked as deleted), `IMapper<TEntity, TReadModel>` converts the entity into a read model for the response:

```csharp
return Mapper.Map<TEntity, TReadModel>(entity);
```

## Pipeline Behaviors

The delete command automatically includes pipeline behaviors that execute in the pipeline:

- **Change Notifications**: `EntityChangeNotificationBehavior` for publishing delete events
  - Publishes domain events or notifications when entities are deleted, allowing other parts of the system to react to changes
  - Useful for implementing event-driven architectures and cross-cutting concerns like logging or cache invalidation

## Usage Examples

### Basic Usage

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var command = new EntityDeleteCommand<int, ProductReadModel>(principal, 123);

var result = await mediator.Send(command);
Console.WriteLine($"Deleted product: {result?.Name}");
```

### In ASP.NET Core Controller

```csharp
[HttpDelete("{id}")]
public async Task<ProductReadModel?> DeleteProduct([FromRoute] int id)
{
    var command = new EntityDeleteCommand<int, ProductReadModel>(User, id);
    return await mediator.Send(command);
}
```

### In Minimal API Endpoint

```csharp
app.MapDelete("/products/{id}", async (
    [FromServices] IMediator mediator,
    [FromRoute] int id,
    ClaimsPrincipal user) =>
{
    var command = new EntityDeleteCommand<int, ProductReadModel>(user, id);
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
```

## Return Values

- **Success**: Returns the `TReadModel` representing the deleted entity
- **Entity Not Found**: Returns `null` when the entity with the specified ID doesn't exist
- **Exception**: Throws appropriate exceptions for validation or data access errors

## Error Handling

The command handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the id parameter is null
- **`DomainException`**: For business rule violations  
- **Database exceptions**: For data access errors

## Best Practices

1. **Entity Existence**: The command automatically checks if the entity exists before deletion
2. **Audit Tracking**: Always pass the current user's `ClaimsPrincipal` for proper audit trails
3. **Soft Delete**: Implement `ITrackDeleted` on entities that should support soft delete
4. **History Tracking**: Consider implementing `ITrackHistory` for entities requiring audit trails
5. **Cache Management**: The framework automatically handles cache invalidation
6. **Error Handling**: Handle potential null returns when entities don't exist
7. **Authorization**: Implement appropriate authorization checks in your application layer
