# Audit Behavior

Pipeline behaviors that automatically track and audit entity changes with metadata such as creation and modification timestamps and user information. These behaviors integrate seamlessly with entity commands to provide consistent audit trails across your application.

## Overview

The Arbiter framework provides automatic audit tracking through pipeline behaviors that work with entity commands. These behaviors inspect entities for tracking interfaces and automatically populate audit metadata without requiring manual intervention in command handlers.

**Key Behaviors:**

- **TrackChangeCommandBehavior** - Automatically populates audit fields based on tracking interfaces
- **EntityChangeNotificationBehavior** - Publishes notifications when entity changes occur

All audit behaviors work automatically with entity commands (`EntityCreateCommand`, `EntityUpdateCommand`, , `EntityUpsertCommand`, `EntityDeleteCommand`) when entities implement the appropriate tracking interfaces.

## TrackChangeCommandBehavior

The `TrackChangeCommandBehavior<TEntityModel, TResponse>` behavior automatically intercepts entity commands to populate audit fields with metadata about who performed the operation and when it occurred. This behavior integrates seamlessly with create, update, upsert and delete commands.

### Automatic Metadata Tracking

When an entity implements tracking interfaces, the behavior automatically populates metadata:

#### For Create Commands (`EntityCreateCommand`)

- **`ITrackCreated`**: Sets `Created` timestamp and `CreatedBy` user identifier for new entities
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier for new entities

#### For Update Commands (`EntityUpdateCommand`, `EntityUpsertCommand`)

- **`ITrackCreated`**: Preserves existing `Created` timestamp and `CreatedBy` user identifier
- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier

#### For Delete Commands (`EntityDeleteCommand`)

- **`ITrackUpdated`**: Sets `Updated` timestamp and `UpdatedBy` user identifier before deletion (for soft deletes)
- **`ITrackDeleted`**: Sets `IsDeleted = true` to mark the entity as deleted

### Tracking Interfaces

Your entities should implement one or more of the following interfaces to enable audit tracking:

#### ITrackCreated Interface

```csharp
public interface ITrackCreated
{
    DateTimeOffset Created { get; set; }
    string? CreatedBy { get; set; }
}
```

**Purpose**: Records creation metadata for new entities

**Usage**: Automatically populated during `EntityCreateCommand` and `EntityUpsertCommand` for new entities

#### ITrackUpdated Interface

```csharp
public interface ITrackUpdated
{
    DateTimeOffset Updated { get; set; }
    string? UpdatedBy { get; set; }
}
```

**Purpose**: Records modification metadata for entity updates

**Usage**: Automatically populated during `EntityCreateCommand`, `EntityUpdateCommand`, `EntityUpsertCommand`, and `EntityDeleteCommand`

#### ITrackDeleted Interface

```csharp
public interface ITrackDeleted
{
    bool IsDeleted { get; set; }
}
```

**Purpose**: Enables soft delete functionality by marking entities as deleted instead of physically removing them

**Usage**: Automatically populated during `EntityDeleteCommand` to mark entities as deleted instead of physical removal

### Example Entity Implementation

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
- **Update Operations**: `EntityChangeOperation.Updated` for `EntityUpdateCommand` and `EntityUpsertCommand`
- **Delete Operations**: `EntityChangeOperation.Deleted` for `EntityDeleteCommand`

### Integration with Commands

The behavior automatically integrates with all entity commands:

- **EntityCreateCommand**: Publishes notification after successful entity creation
- **EntityUpdateCommand**: Publishes notification after successful entity update  
- **EntityUpsertCommand**: Publishes notification after successful upsert operation
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

The audit behaviors are automatically registered in the correct order within the pipeline:

1. **Authorization behaviors** - Verify user permissions
2. **Tenant behaviors** - Handle multi-tenant scenarios
3. **TrackChangeCommandBehavior** - Populate audit fields
4. **Validation behaviors** - Validate models and business rules
5. **Core command handler** - Execute the actual operation
6. **EntityChangeNotificationBehavior** - Publish change notifications

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
