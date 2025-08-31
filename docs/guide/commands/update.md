# Update Command

The `EntityUpdateCommand<TKey, TUpdateModel, TReadModel>` represents a command to update an entire entity identified by a specific key using the provided update model. Unlike patch operations that modify specific fields, update commands replace the complete entity data while preserving its identity.

## Overview

This command is a fundamental part of the CQRS (Command Query Responsibility Segregation) pattern in the Arbiter framework. It provides a strongly-typed approach to entity updates with built-in support for audit tracking, cache invalidation, and security context preservation.

```csharp
public record EntityUpdateCommand<TKey, TUpdateModel, TReadModel>
    : EntityModelCommand<TUpdateModel, TReadModel>, ICacheExpire
```

## Key Features

- **Complete Entity Replacement**: Updates the entire entity with new data while maintaining its identity
- **Type Safety**: Strongly-typed key, update model, and read model parameters
- **Security Integration**: Built-in `ClaimsPrincipal` support for user context and authorization
- **Cache Management**: Automatic cache invalidation through `ICacheExpire` interface
- **Audit Support**: Seamless integration with audit tracking behaviors
- **Cross-Database Support**: Works with both Entity Framework Core and MongoDB implementations
- **Validation Ready**: Compatible with validation behaviors and pipeline processing

## Type Parameters

| Parameter      | Description                                                                     |
| -------------- | ------------------------------------------------------------------------------- |
| `TKey`         | The type of the key used to identify the entity (e.g., `int`, `Guid`, `string`) |
| `TUpdateModel` | The type of the update model containing the complete data for replacement       |
| `TReadModel`   | The type of the read model returned as the result of the command                |

## Constructor Parameters

| Parameter   | Type               | Description                                                        |
| ----------- | ------------------ | ------------------------------------------------------------------ |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization |
| `id`        | `TKey`             | The identifier of the entity to update (must not be null)          |
| `model`     | `TUpdateModel`     | The update model containing the complete replacement data          |

## Automatic Metadata Tracking

When an entity implements tracking interfaces, the command automatically populates metadata:

- **`ITrackCreated`**: Preserves existing `Created` timestamp and `CreatedBy` user identifier
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityUpdateCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>
```

### MongoDB Handler

```csharp
EntityUpdateCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>
```

## Service Registration

Register update command support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register update command individually
services.AddEntityUpdateCommand<MyDbContext, Product, int, ProductUpdateModel, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register update command individually
services.AddEntityUpdateCommand<IProductRepository, Product, int, ProductUpdateModel, ProductReadModel>();
```

## Model Mapping with IMapper

The update command relies on `IMapper<TSource, TDestination>` to convert between different model types during the entity update process. The framework uses two key mapping operations:

### UpdateModel to Entity Mapping

The command handler uses `IMapper<TUpdateModel, TEntity>` to convert the incoming update model into an entity that can be persisted to the database:

```csharp
Mapper.Map<TUpdateModel, TEntity>(request.Model, entity);
```

### Entity to ReadModel Mapping

After the entity is updated and saved, `IMapper<TEntity, TReadModel>` converts the persisted entity into a read model for the response:

```csharp
return Mapper.Map<TEntity, TReadModel>(entity);
```

## Pipeline Behaviors

The update command automatically includes several pipeline behaviors that execute in the pipeline:

- **Tenant Security**: `TenantDefaultCommandBehavior` and `TenantAuthenticateCommandBehavior` (if entity implements `IHaveTenant<TKey>`)
  - `TenantDefaultCommandBehavior`: Automatically sets the tenant ID from the current user's claims when not explicitly provided
  - `TenantAuthenticateCommandBehavior`: Validates that the user has access to the specified tenant and ensures tenant isolation
  
- **Change Tracking**: `TrackChangeCommandBehavior` (if entity implements tracking interfaces)
  - Automatically populates audit fields like `Created`, `CreatedBy`, `Updated`, and `UpdatedBy` based on the current user and timestamp
  
- **Validation**: `ValidateEntityModelCommandBehavior`
  - Performs model validation using data annotations or FluentValidation rules defined on the update model
  - Throws validation exceptions if the model is invalid, which are automatically converted to HTTP 400 responses in web applications
  
- **Notifications**: `EntityChangeNotificationBehavior`
  - Publishes domain events or notifications when entities are updated, allowing other parts of the system to react to changes
  - Useful for implementing event-driven architectures and cross-cutting concerns like logging or cache invalidation

## Usage Examples

### Basic Usage

```csharp
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product",
    Description = "An updated description of the product",
    Price = 29.99m
};

var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, 123, updateModel);

var result = await mediator.Send(command);
Console.WriteLine($"Updated product: {result?.Name}");
```

### In ASP.NET Core Controller

```csharp
[HttpPut("{id}")]
public async Task<ProductReadModel?> UpdateProduct(int id, [FromBody] ProductUpdateModel updateModel)
{
    var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(User, id, updateModel);
    return await mediator.Send(command);
}
```

### In Minimal API Endpoint

```csharp
app.MapPut("/products/{id:int}", async (
    int id,
    [FromServices] IMediator mediator,
    [FromBody] ProductUpdateModel updateModel,
    ClaimsPrincipal user) =>
{
    var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(user, id, updateModel);
    var result = await mediator.Send(command);
    return result != null ? Results.Ok(result) : Results.NotFound();
});
```

## Return Values

- **Success**: Returns the `TReadModel` representing the updated entity
- **Entity Not Found**: Returns `null` when the entity with the specified ID doesn't exist
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
6. **Entity Existence**: The command automatically checks if the entity exists before updating
7. **Authorization**: Implement appropriate authorization checks in your application layer
