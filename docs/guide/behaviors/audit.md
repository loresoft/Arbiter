---
title: Audit Behavior
description: Automatic audit tracking for entity changes with metadata
---

# Audit Behavior

The Arbiter framework provides automatic audit tracking through command handlers that work with entity commands. The handlers inspect entities for tracking interfaces and automatically populate audit metadata without requiring manual intervention.

## Overview

Audit tracking is built directly into the entity command handlers and works automatically with entity commands (`EntityCreateCommand`, `EntityUpdateCommand`, `EntityDeleteCommand`) when entities implement the appropriate tracking interfaces.

**Key Features:**

- **Automatic Metadata Population** - Handlers automatically populate audit fields based on tracking interfaces
- **EntityChangeNotificationBehavior** - Publishes notifications when entity changes occur

All audit functionality works automatically with entity commands when entities implement the appropriate tracking interfaces.

## Automatic Metadata Tracking

When an entity implements tracking interfaces, the command handlers automatically populate metadata:

### For Create Commands (`EntityCreateCommand`)

- **`ITrackCreated`**: Sets `Created` timestamp and `CreatedBy` user identifier for new entities
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier for new entities

### For Update Commands (`EntityUpdateCommand`)

- **`ITrackCreated`**: Sets for new entities, preserves for existing entities during upsert
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier

### For Delete Commands (`EntityDeleteCommand`)

- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier before deletion (for soft deletes)
- **`ITrackDeleted`**: Sets `IsDeleted = true` to mark the entity as deleted

## Tracking Interfaces

Your entities should implement one or more of the following interfaces to enable audit tracking:

### ITrackCreated Interface

```csharp
public interface ITrackCreated
{
    DateTimeOffset Created { get; set; }
    string? CreatedBy { get; set; }
}
```

**Purpose**: Records creation metadata for new entities

**Usage**: Automatically populated during `EntityCreateCommand` and `EntityUpdateCommand` (for new entities during upsert)

### ITrackUpdated Interface

```csharp
public interface ITrackUpdated
{
    DateTimeOffset Updated { get; set; }
    string? UpdatedBy { get; set; }
}
```

**Purpose**: Records modification metadata for entity updates

**Usage**: Automatically populated during `EntityCreateCommand`, `EntityUpdateCommand`, and `EntityDeleteCommand`

### ITrackDeleted Interface

```csharp
public interface ITrackDeleted
{
    bool IsDeleted { get; set; }
}
```

**Purpose**: Enables soft delete functionality by marking entities as deleted instead of physically removing them

**Usage**: Automatically populated during `EntityDeleteCommand` to mark entities as deleted instead of physical removal

## Example Entity Implementation

```csharp
public class Product : ITrackCreated, ITrackUpdated, ITrackDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // ITrackCreated properties
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    
    // ITrackUpdated properties  
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
    
    // ITrackDeleted properties
    public bool IsDeleted { get; set; }
}
```

## EntityChangeNotificationBehavior

The `EntityChangeNotificationBehavior<TKey, TEntityModel, TResponse>` behavior automatically publishes notifications when entity changes occur through commands. This enables reactive programming patterns and event-driven architectures by allowing other parts of the system to react to entity changes.

### Automatic Notifications

The behavior publishes `EntityChangeNotification<TResponse>` events after successful command operations:

- **Create Operations**: `EntityChangeOperation.Created` for `EntityCreateCommand`
- **Update Operations**: `EntityChangeOperation.Updated` for `EntityUpdateCommand` (includes upsert scenarios)
- **Delete Operations**: `EntityChangeOperation.Deleted` for `EntityDeleteCommand`

### Integration with Commands

The behavior automatically integrates with all entity commands:

- **EntityCreateCommand**: Publishes notification after successful entity creation
- **EntityUpdateCommand**: Publishes notification after successful entity update (includes upsert scenarios)
- **EntityDeleteCommand**: Publishes notification after successful deletion (soft or hard)

### Notification Structure

```csharp
public class EntityChangeNotification<TResponse> : INotification
{
    public TResponse Entity { get; set; }
    public EntityChangeOperation Operation { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public enum EntityChangeOperation
{
    Created,
    Updated, 
    Deleted
}
```

### Usage Example

```csharp
// Subscribe to entity change notifications
public class ProductNotificationHandler : INotificationHandler<EntityChangeNotification<ProductReadModel>>
{
    private readonly ILogger<ProductNotificationHandler> _logger;
    
    public ProductNotificationHandler(ILogger<ProductNotificationHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task Handle(EntityChangeNotification<ProductReadModel> notification, CancellationToken cancellationToken)
    {
        var product = notification.Entity;
        var operation = notification.Operation;
        var user = notification.Principal?.Identity?.Name ?? "System";
        
        _logger.LogInformation("Product {ProductId} was {Operation} by {User} at {Timestamp}", 
            product.Id, operation, user, notification.Timestamp);
            
        // Additional reactive logic
        switch (operation)
        {
            case EntityChangeOperation.Created:
                // Handle product creation
                await HandleProductCreated(product, cancellationToken);
                break;
                
            case EntityChangeOperation.Updated:
                // Handle product update
                await HandleProductUpdated(product, cancellationToken);
                break;
                
            case EntityChangeOperation.Deleted:
                // Handle product deletion
                await HandleProductDeleted(product, cancellationToken);
                break;
        }
    }
    
    private async Task HandleProductCreated(ProductReadModel product, CancellationToken cancellationToken)
    {
        // Example: Send welcome email, update inventory, etc.
    }
    
    private async Task HandleProductUpdated(ProductReadModel product, CancellationToken cancellationToken)
    {
        // Example: Invalidate cache, update search index, etc.
    }
    
    private async Task HandleProductDeleted(ProductReadModel product, CancellationToken cancellationToken)
    {
        // Example: Archive data, update reports, etc.
    }
}
```

## Service Registration

### Automatic Registration with Entity Commands

The audit behaviors are automatically registered when using the entity command registration methods:

```csharp
// Entity Framework registration - includes audit behaviors
services.AddEntityCommands<MyDbContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();

// MongoDB registration - includes audit behaviors  
services.AddEntityCommands<IProductRepository, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

### Individual Command Registration

You can also register commands individually, which includes the audit behaviors:

```csharp
// Entity Framework individual registrations
services.AddEntityCreateCommand<MyDbContext, Product, int, ProductCreateModel, ProductReadModel>();
services.AddEntityUpdateCommand<MyDbContext, Product, int, ProductUpdateModel, ProductReadModel>();
services.AddEntityDeleteCommand<MyDbContext, Product, int, ProductReadModel>();

// MongoDB individual registrations
services.AddEntityCreateCommand<IProductRepository, Product, int, ProductCreateModel, ProductReadModel>();
services.AddEntityUpdateCommand<IProductRepository, Product, int, ProductUpdateModel, ProductReadModel>();
services.AddEntityDeleteCommand<IProductRepository, Product, int, ProductReadModel>();
```

### Pipeline Behavior Order

The pipeline behaviors are automatically registered in the correct order:

1. **Authorization behaviors** - Verify user permissions
2. **Tenant behaviors** - Handle multi-tenant scenarios
3. **Validation behaviors** - Validate models and business rules
4. **Core command handler** - Execute the actual operation (including audit field population)
5. **EntityChangeNotificationBehavior** - Publish change notifications

## Usage with Entity Commands

### Create Command with Audit Tracking

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "johndoe")]));

var createModel = new ProductCreateModel
{
    Name = "New Product",
    Description = "Product description",
    Price = 29.99m
};

var createCommand = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);
var createdProduct = await mediator.Send(createCommand);

// Audit fields automatically populated:
// - Created = DateTimeOffset.UtcNow
// - CreatedBy = "johndoe"  
// - Updated = DateTimeOffset.UtcNow
// - UpdatedBy = "johndoe"
```

### Update Command with Audit Tracking

```csharp
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product Name",
    Description = "Updated description",
    Price = 39.99m
};

var updateCommand = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, 123, updateModel);
var updatedProduct = await mediator.Send(updateCommand);

// Audit fields automatically updated:
// - Created = (preserved from original)
// - CreatedBy = (preserved from original)
// - Updated = DateTimeOffset.UtcNow  
// - UpdatedBy = "johndoe"
```

### Delete Command with Audit Tracking

```csharp
var deleteCommand = new EntityDeleteCommand<int, ProductReadModel>(principal, 123);
var deletedProduct = await mediator.Send(deleteCommand);

// For soft delete (ITrackDeleted implemented):
// - Updated = DateTimeOffset.UtcNow
// - UpdatedBy = "johndoe"
// - IsDeleted = true
```
