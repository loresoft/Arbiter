---
title: Upsert Command
description: Command to create or update an entity identified by a specific key using the provided update model
---

# Upsert Command

The `EntityUpsertCommand<TKey, TUpdateModel, TReadModel>` represents a command to create or update an entity identified by a specific key using the provided update model. This command intelligently determines whether to create a new entity or update an existing one based on whether the entity with the specified key already exists.

## Overview

This command is a fundamental part of the CQRS (Command Query Responsibility Segregation) pattern in the Arbiter framework. It provides a strongly-typed approach to upsert operations with built-in support for audit tracking, cache invalidation, and security context preservation.

```csharp
public record EntityUpsertCommand<TKey, TUpdateModel, TReadModel>
    : EntityModelCommand<TUpdateModel, TReadModel>, ICacheExpire
```

## Key Features

- **Intelligent Create or Update**: Automatically creates new entities or updates existing ones based on key existence
- **Type Safety**: Strongly-typed key, update model, and read model parameters
- **Security Integration**: Built-in `ClaimsPrincipal` support for user context and authorization
- **Cache Management**: Automatic cache invalidation through `ICacheExpire` interface
- **Audit Support**: Seamless integration with audit tracking behaviors for both create and update scenarios
- **Cross-Database Support**: Works with both Entity Framework Core and MongoDB implementations
- **Validation Ready**: Compatible with validation behaviors and pipeline processing

## Type Parameters

| Parameter      | Description                                                                     |
| -------------- | ------------------------------------------------------------------------------- |
| `TKey`         | The type of the key used to identify the entity (e.g., `int`, `Guid`, `string`) |
| `TUpdateModel` | The type of the update model containing the data for the operation              |
| `TReadModel`   | The type of the read model returned as the result of the command                |

## Constructor Parameters

| Parameter   | Type               | Description                                                        |
| ----------- | ------------------ | ------------------------------------------------------------------ |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization |
| `id`        | `TKey`             | The identifier of the entity to create or update (must not be null) |
| `model`     | `TUpdateModel`     | The update model containing the data for the operation              |

## Automatic Metadata Tracking

When an entity implements tracking interfaces, the command automatically populates metadata:

- **`ITrackCreated`**: Sets `Created` timestamp and `CreatedBy` user identifier for new entities
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier for all operations

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityUpsertCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>
```

### MongoDB Handler

```csharp
EntityUpsertCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>
```

## Service Registration

Register upsert command support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register upsert command individually
services.AddEntityUpsertCommand<MyDbContext, Product, int, ProductUpdateModel, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register upsert command individually
services.AddEntityUpsertCommand<IProductRepository, Product, int, ProductUpdateModel, ProductReadModel>();
```

## Model Mapping with IMapper

The upsert command relies on `IMapper<TSource, TDestination>` to convert between different model types during the entity upsert process. The framework uses two key mapping operations:

### UpdateModel to Entity Mapping

The command handler uses `IMapper<TUpdateModel, TEntity>` to convert the incoming update model into an entity that can be persisted to the database:

```csharp
Mapper.Map<TUpdateModel, TEntity>(request.Model, entity);
```

### Entity to ReadModel Mapping

After the entity is created or updated and saved, `IMapper<TEntity, TReadModel>` converts the persisted entity into a read model for the response:

```csharp
return Mapper.Map<TEntity, TReadModel>(result);
```

## Pipeline Behaviors

The upsert command automatically includes several pipeline behaviors that execute in the pipeline:

- **Tenant Security**: `TenantDefaultCommandBehavior` and `TenantAuthenticateCommandBehavior` (if entity implements `IHaveTenant<TKey>`)
  - `TenantDefaultCommandBehavior`: Automatically sets the tenant ID from the current user's claims when not explicitly provided
  - `TenantAuthenticateCommandBehavior`: Validates that the user has access to the specified tenant and ensures tenant isolation
  
- **Change Tracking**: `TrackChangeCommandBehavior` (if entity implements tracking interfaces)
  - Automatically populates audit fields like `Created`, `CreatedBy`, `Updated`, and `UpdatedBy` based on the current user and timestamp
  
- **Validation**: `ValidateEntityModelCommandBehavior`
  - Performs model validation using data annotations or FluentValidation rules defined on the update model
  - Throws validation exceptions if the model is invalid, which are automatically converted to HTTP 400 responses in web applications
  
- **Notifications**: `EntityChangeNotificationBehavior`
  - Publishes domain events or notifications when entities are created or updated, allowing other parts of the system to react to changes
  - Useful for implementing event-driven architectures and cross-cutting concerns like logging or cache invalidation

## Usage Examples

### Basic Usage

```csharp
var updateModel = new ProductUpdateModel
{
    Name = "Upserted Product",
    Description = "A description of the upserted product",
    Price = 24.99m
};

var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var command = new EntityUpsertCommand<int, ProductUpdateModel, ProductReadModel>(principal, 123, updateModel);

var result = await mediator.Send(command);
Console.WriteLine($"Upserted product: {result?.Name}");
```

### In ASP.NET Core Controller

```csharp
[HttpPost("{id}")]
public async Task<ProductReadModel?> UpsertProduct(int id, [FromBody] ProductUpdateModel updateModel)
{
    var command = new EntityUpsertCommand<int, ProductUpdateModel, ProductReadModel>(User, id, updateModel);
    return await mediator.Send(command);
}
```

### In Minimal API Endpoint

```csharp
app.MapPost("/products/{id:int}", async (
    int id,
    [FromServices] IMediator mediator,
    [FromBody] ProductUpdateModel updateModel,
    ClaimsPrincipal user) =>
{
    var command = new EntityUpsertCommand<int, ProductUpdateModel, ProductReadModel>(user, id, updateModel);
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
```

## Return Values

- **Success**: Returns the `TReadModel` representing the upserted entity
- **Exception**: Throws appropriate exceptions for validation or data access errors

## Error Handling

The command handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the id or model parameters are null
- **`DomainException`**: For business rule violations  
- **Database exceptions**: For data access errors

## Best Practices

1. **Model Validation**: Implement data annotations or FluentValidation on your update models
2. **Mapping Configuration**: Configure Mapper for `TUpdateModel` to `TEntity` mapping
3. **Security**: Always pass the current user's `ClaimsPrincipal` for proper audit tracking
4. **Null Handling**: The command constructor validates that the id and model are not null
5. **Cache Tags**: The framework automatically manages cache invalidation based on entity types
6. **Entity Creation vs Update**: The command automatically determines whether to create or update based on entity existence
7. **Authorization**: Implement appropriate authorization checks in your application layer
