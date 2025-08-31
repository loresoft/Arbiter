# Patch Command

The `EntityPatchCommand<TKey, TReadModel>` represents a command to apply a JSON patch to an entity identified by a specific key. This command follows the CQRS (Command Query Responsibility Segregation) pattern and is designed for applying partial updates to entities using standardized JSON patch operations.

## Overview

The patch command is part of the Arbiter framework's CRUD operations, specifically designed for partial updates. It inherits from `EntityIdentifierCommand<TKey, TReadModel>` which provides automatic security context, audit tracking, and JSON serialization support. The command uses the System.Text.Json implementation of JSON Patch (RFC 6902) for applying changes.

```csharp
public record EntityPatchCommand<TKey, TReadModel>
    : EntityIdentifierCommand<TKey, TReadModel>, ICacheExpire
```

## Key Features

- **JSON Patch Support**: Uses standardized RFC 6902 JSON Patch operations for partial updates
- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Tracking**: Automatic update metadata tracking including `Updated` and `UpdatedBy` fields
- **Cache Integration**: Implements `ICacheExpire` for automatic cache invalidation
- **Entity Validation**: Validates entity exists before applying patch operations
- **Change Notifications**: Integrated with notification pipeline for publish/subscribe patterns
- **Atomic Operations**: Ensures all patch operations are applied as a single transaction

## Type Parameters

| Parameter    | Description                                                         |
| ------------ | ------------------------------------------------------------------- |
| `TKey`       | The type of the key used to identify the entity                     |
| `TReadModel` | The type of the read model returned representing the patched entity |

## Constructor Parameters

| Parameter   | Type                | Description                                                            |
| ----------- | ------------------- | ---------------------------------------------------------------------- |
| `principal` | `ClaimsPrincipal?`  | The user's security context. Used for audit tracking and authorization |
| `id`        | `TKey`              | The identifier of the entity to which the JSON patch will be applied   |
| `patch`     | `JsonPatchDocument` | The JSON patch document containing the operations to apply             |

## Automatic Metadata Tracking

When an entity implements tracking interfaces, the command automatically populates metadata:

- **`ITrackCreated`**: Preserves existing `Created` timestamp and `CreatedBy` user identifier
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier

## JSON Patch Operations

The command supports all standard JSON Patch operations:

- **Add**: Adds a new value to the specified path
- **Remove**: Removes the value at the specified path
- **Replace**: Replaces the value at the specified path
- **Move**: Moves a value from one path to another
- **Copy**: Copies a value from one path to another
- **Test**: Tests that the value at the specified path equals the given value

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityPatchCommandHandler<TContext, TEntity, TKey, TReadModel>
```

### MongoDB Handler

```csharp
EntityPatchCommandHandler<TRepository, TEntity, TKey, TReadModel>
```

## Service Registration

Register patch command support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register patch command individually
services.AddEntityPatchCommand<MyDbContext, Product, int, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register patch command individually
services.AddEntityPatchCommand<IProductRepository, Product, int, ProductReadModel>();
```

## Model Mapping with IMapper

The patch command relies on `IMapper<TSource, TDestination>` to convert the patched entity into a read model for the response:

### Entity to ReadModel Mapping

After the JSON patch operations are applied to the entity, `IMapper<TEntity, TReadModel>` converts the updated entity into a read model for the response:

```csharp
// In EntityPatchCommandHandler
return Mapper.Map<TEntity, TReadModel>(entity);
```

## Pipeline Behaviors

The patch command automatically includes pipeline behaviors that execute in the pipeline:

- **Change Notifications**: `EntityChangeNotificationBehavior` for publishing patch events
  - Publishes domain events or notifications when entities are patched, allowing other parts of the system to react to changes
  - Useful for implementing event-driven architectures and cross-cutting concerns like logging or cache invalidation

## Usage Examples

### Basic Usage

```csharp
var patchDocument = new JsonPatchDocument();
patchDocument.Replace("/Name", "Updated Product Name");
patchDocument.Replace("/Price", 29.99m);

var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var command = new EntityPatchCommand<int, ProductReadModel>(principal, 123, patchDocument);

var result = await mediator.Send(command);
Console.WriteLine($"Updated product: {result?.Name}");
```

### Multiple Operations

```csharp
var patchDocument = new JsonPatchDocument();
patchDocument.Replace("/Name", "New Name");
patchDocument.Add("/Tags/-", "New Tag");           // Add to end of array
patchDocument.Remove("/Description");              // Remove property
patchDocument.Test("/IsActive", true);             // Verify value before applying other operations

var command = new EntityPatchCommand<int, ProductReadModel>(User, productId, patchDocument);
var result = await mediator.Send(command);
```

### In ASP.NET Core Controller

```csharp
[HttpPatch("{id}")]
public async Task<ProductReadModel?> PatchProduct(
    [FromRoute] int id,
    [FromBody] JsonPatchDocument jsonPatch)
{
    var command = new EntityPatchCommand<int, ProductReadModel>(User, id, jsonPatch);
    return await mediator.Send(command);
}
```

### In Minimal API Endpoint

```csharp
app.MapPatch("/products/{id}", async (
    [FromServices] IMediator mediator,
    [FromRoute] int id,
    [FromBody] JsonPatchDocument jsonPatch,
    ClaimsPrincipal user) =>
{
    var command = new EntityPatchCommand<int, ProductReadModel>(user, id, jsonPatch);
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
```

### Complex Patch Operations

```csharp
var patchDocument = new JsonPatchDocument();

// Update nested properties
patchDocument.Replace("/Address/Street", "123 New Street");
patchDocument.Replace("/Address/City", "New City");

// Array operations
patchDocument.Add("/Categories/-", "Electronics");     // Add to end
patchDocument.Remove("/Categories/0");                 // Remove first item
patchDocument.Replace("/Categories/1", "Updated");     // Replace by index

// Conditional operations
patchDocument.Test("/Status", "Active");               // Only proceed if status is Active
patchDocument.Replace("/LastModified", DateTime.UtcNow);

var command = new EntityPatchCommand<int, ProductReadModel>(User, productId, patchDocument);
var result = await mediator.Send(command);
```

## Return Values

- **Success**: Returns the `TReadModel` representing the patched entity
- **Entity Not Found**: Returns `null` when the entity with the specified ID doesn't exist
- **Exception**: Throws appropriate exceptions for validation, patch operation, or data access errors

## JSON Patch Path Format

Paths in JSON patch operations use JSON Pointer format (RFC 6901):

```csharp
// Simple properties
patchDocument.Replace("/Name", "New Name");
patchDocument.Replace("/Price", 99.99m);

// Nested objects
patchDocument.Replace("/Address/Street", "123 Main St");
patchDocument.Replace("/Contact/Email", "new@example.com");

// Array elements by index
patchDocument.Replace("/Tags/0", "Updated Tag");
patchDocument.Add("/Tags/-", "New Tag");        // Add to end

// Property names with special characters (escaped)
patchDocument.Replace("/Properties/~1Special~1Name", "value");  // "Properties/Special/Name"
```

## Error Handling

The command handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the id or patch parameters are null
- **`JsonPatchException`**: When patch operations fail or contain invalid paths
- **`DomainException`**: For business rule violations  
- **Database exceptions**: For data access errors

## Best Practices

1. **Entity Validation**: The command automatically checks if the entity exists before applying patches
2. **Audit Tracking**: Always pass the current user's `ClaimsPrincipal` for proper audit trails
3. **Path Validation**: Ensure JSON patch paths are valid and point to existing properties
4. **Atomic Operations**: All patch operations are applied in a single transaction
5. **Test Operations**: Use `Test` operations to validate preconditions before applying changes
6. **Error Handling**: Handle potential null returns when entities don't exist
7. **Cache Management**: The framework automatically handles cache invalidation
8. **Security**: Validate that users have permission to modify the requested properties
9. **Property Types**: Ensure replacement values match the target property types
10. **Array Bounds**: Validate array indices to prevent out-of-bounds errors
