---
title: Create Command
description: Command to create a new entity using the specified create model
---

# Create Command

The `EntityCreateCommand<TCreateModel, TReadModel>` represents a command to create a new entity using the specified create model. This command follows the CQRS (Command Query Responsibility Segregation) pattern and returns a read model representing the created entity.

## Overview

The create command is a fundamental part of the Arbiter framework's CRUD operations. It inherits from `EntityModelCommand<TCreateModel, TReadModel>` which provides automatic security context, audit tracking, and JSON serialization support.

```csharp
public record EntityCreateCommand<TCreateModel, TReadModel>
    : EntityModelCommand<TCreateModel, TReadModel>, ICacheExpire
```

## Key Features

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Tracking**: Automatic creation metadata tracking including `Created`, `CreatedBy`, `Updated`, and `UpdatedBy` fields
- **Cache Integration**: Implements `ICacheExpire` for automatic cache invalidation
- **Validation**: Integrated with validation pipeline behaviors
- **Mapping**: Uses Mapper for converting between create models and entities
- **Tenant Support**: Optional multi-tenant support through `IHaveTenant<TKey>` interface

## Type Parameters

| Parameter      | Description                                                         |
| -------------- | ------------------------------------------------------------------- |
| `TCreateModel` | The type of the create model containing the data for the new entity |
| `TReadModel`   | The type of the read model returned as the result of the command    |

## Constructor Parameters

| Parameter   | Type               | Description                                                            |
| ----------- | ------------------ | ---------------------------------------------------------------------- |
| `principal` | `ClaimsPrincipal?` | The user's security context. Used for audit tracking and authorization |
| `model`     | `TCreateModel`     | The create model containing the data for the new entity                |

## Automatic Metadata Tracking

When an entity implements tracking interfaces, the command automatically populates metadata:

- **`ITrackCreated`**: Sets `Created` timestamp and `CreatedBy` user identifier
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier

## Handler Implementations

The Arbiter framework provides built-in handlers for different data access patterns:

### Entity Framework Handler

```csharp
EntityCreateCommandHandler<TContext, TEntity, TKey, TCreateModel, TReadModel>
```

### MongoDB Handler

```csharp
EntityCreateCommandHandler<TRepository, TEntity, TKey, TCreateModel, TReadModel>
```

## Service Registration

Register create command support using the provided extension methods:

### Entity Framework

```csharp
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register create command individually
services.AddEntityCreateCommand<MyDbContext, Product, int, ProductCreateModel, ProductReadModel>();
```

### MongoDB

```csharp
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// Or register create command individually
services.AddEntityCreateCommand<IProductRepository, Product, int, ProductCreateModel, ProductReadModel>();
```

## Model Mapping with IMapper

The create command relies on `IMapper<TSource, TDestination>` to convert between different model types during the entity creation process. The framework uses two key mapping operations:

### CreateModel to Entity Mapping

The command handler uses `IMapper<TCreateModel, TEntity>` to convert the incoming create model into an entity that can be persisted to the database:

```csharp
var entity = Mapper.Map<TCreateModel, TEntity>(request.Model);
```

### Entity to ReadModel Mapping

After the entity is created and saved, `IMapper<TEntity, TReadModel>` converts the persisted entity into a read model for the response:

```csharp
return Mapper.Map<TEntity, TReadModel>(entity);
```

## Pipeline Behaviors

The create command automatically includes several pipeline behaviors that execute in the pipeline:

- **Tenant Security**: `TenantDefaultCommandBehavior` and `TenantAuthenticateCommandBehavior` (if entity implements `IHaveTenant<TKey>`)
  - `TenantDefaultCommandBehavior`: Automatically sets the tenant ID from the current user's claims when not explicitly provided
  - `TenantAuthenticateCommandBehavior`: Validates that the user has access to the specified tenant and ensures tenant isolation
  
- **Change Tracking**: `TrackChangeCommandBehavior` (if entity implements tracking interfaces)
  - Automatically populates audit fields like `Created`, `CreatedBy`, `Updated`, and `UpdatedBy` based on the current user and timestamp
  
- **Validation**: `ValidateEntityModelCommandBehavior`
  - Performs model validation using data annotations or FluentValidation rules defined on the create model
  - Throws validation exceptions if the model is invalid, which are automatically converted to HTTP 400 responses in web applications
  
- **Notifications**: `EntityChangeNotificationBehavior`
  - Publishes domain events or notifications when entities are created, allowing other parts of the system to react to changes
  - Useful for implementing event-driven architectures and cross-cutting concerns like logging or cache invalidation

## Usage Examples

### Basic Usage

```csharp
var createModel = new ProductCreateModel
{
    Name = "New Product",
    Description = "A description of the new product",
    Price = 19.99m
};

var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "JohnDoe")]));
var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);

var result = await mediator.Send(command);
Console.WriteLine($"Created product: {result?.Name}");
```

### In ASP.NET Core Controller

```csharp
[HttpPost]
public async Task<ProductReadModel?> CreateProduct([FromBody] ProductCreateModel createModel)
{
    var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(User, createModel);
    return await mediator.Send(command);
}
```

### In Minimal API Endpoint

```csharp
app.MapPost("/products", async (
    [FromServices] IMediator mediator,
    [FromBody] ProductCreateModel createModel,
    ClaimsPrincipal user) =>
{
    var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(user, createModel);
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
```

## Return Values

- **Success**: Returns the `TReadModel` representing the created entity
- **Exception**: Throws appropriate exceptions for validation or data access errors

## Error Handling

The command handlers include built-in error handling and will throw appropriate exceptions:

- **`ArgumentNullException`**: When the model parameter is null
- **`DomainException`**: For business rule violations  
- **Database exceptions**: For data access errors

## Best Practices

1. **Model Validation**: Implement data annotations or FluentValidation on your create models
2. **Mapping Configuration**: Configure Mapper for `TCreateModel` to `TEntity` mapping
3. **Security**: Always pass the current user's `ClaimsPrincipal` for proper audit tracking
4. **Null Handling**: The command constructor validates that the model is not null
5. **Cache Tags**: The framework automatically manages cache invalidation based on entity types
6. **Authorization**: Implement appropriate authorization checks in your application layer
