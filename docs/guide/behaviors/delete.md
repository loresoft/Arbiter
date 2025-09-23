---
title: Soft Delete Behaviors
description: Pipeline behaviors that implement soft delete functionality by automatically filtering out deleted entities
---

# Soft Delete Behaviors

Pipeline behaviors that implement soft delete functionality by automatically filtering out deleted entities and providing consistent deletion semantics across your application. These behaviors work in conjunction with the `EntityDeleteCommand` to provide seamless soft delete capabilities.

## Overview

The Arbiter framework provides automatic soft delete functionality through pipeline behaviors that work with entities implementing the `ITrackDeleted` interface. When an entity supports soft delete, the `EntityDeleteCommand` performs a soft delete instead of physically removing the entity from the database.

**Key Features:**

- **Automatic Detection**: Commands automatically detect entities implementing `ITrackDeleted`
- **Soft Delete by Default**: Entities are marked as deleted rather than physically removed
- **Query Filtering**: Deleted entities are automatically excluded from normal queries
- **Audit Preservation**: Maintains deleted records for compliance and audit trails

## DeletedFilterBehavior

The `DeletedFilterBehavior` behavior automatically excludes soft-deleted entities from query results by applying an `IsDeleted = false` filter. This ensures that deleted entities remain in the database for audit purposes while being invisible to normal application operations.

### Key Features

- **Automatic Filtering**: Transparently excludes deleted entities from all queries
- **Audit Preservation**: Maintains deleted records for compliance and audit trails
- **Query Transparency**: Works without modifying existing query handlers
- **Conditional Application**: Only applies to entities implementing soft delete interfaces
- **Performance Optimized**: Efficient filtering using database indexes

### Required Entity Interface

Your entities must implement the `ITrackDeleted` interface to enable soft delete functionality:

```csharp
public interface ITrackDeleted
{
    bool IsDeleted { get; set; }
}
```

**Purpose**: Enables soft delete functionality by marking entities as deleted instead of physically removing them

**Usage**: When `EntityDeleteCommand` is executed on entities implementing this interface, the entity is marked as deleted rather than removed from the database

### Example Entity Implementation

```csharp
public class User : ITrackDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Soft delete property
    public bool IsDeleted { get; set; }
}

// Entity with combined audit and soft delete tracking
public class Order : ITrackCreated, ITrackUpdated, ITrackDeleted
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
    
    // Creation tracking
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    
    // Update tracking
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft delete tracking
    public bool IsDeleted { get; set; }
}
```

### Filtering Behavior

The behavior automatically modifies queries to exclude deleted entities:

```csharp
// When you execute a normal query
var users = await mediator.Send(new EntityListQuery<UserReadModel>(principal));

// The behavior automatically adds: WHERE IsDeleted = false
// So only active (non-deleted) entities are returned
```

### Filter Logic

1. **Automatic Detection**: Checks if entity implements `ITrackDeleted`
2. **Filter Injection**: Adds `IsDeleted = false` condition to queries
3. **Filter Preservation**: Combines with existing filters using AND logic
4. **Query Transparency**: Works without modifying existing query handlers

## EntityDeleteCommand with Soft Delete

### Automatic Soft Delete

When using `EntityDeleteCommand` with entities that implement `ITrackDeleted`, the command automatically performs a soft delete:

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "johndoe")]));
var deleteCommand = new EntityDeleteCommand<int, UserReadModel>(principal, userId);

var deletedUser = await mediator.Send(deleteCommand);

// Automatic soft delete behavior:
// - IsDeleted = true
// - Updated = DateTimeOffset.UtcNow (if entity implements ITrackUpdated)
// - UpdatedBy = "johndoe" (if entity implements ITrackUpdated)
```

### Hard Delete (No ITrackDeleted)

When an entity doesn't implement `ITrackDeleted`, the command performs a hard delete:

```csharp
// Entity without ITrackDeleted interface
public class TemporaryData
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
}

// This will physically remove the entity from the database
var deleteCommand = new EntityDeleteCommand<int, TemporaryDataReadModel>(principal, tempId);
var result = await mediator.Send(deleteCommand);
```

## Service Registration

### Automatic Registration with Entity Commands

The soft delete behaviors are automatically registered when using entity command registration methods:

```csharp
// Entity Framework registration - includes soft delete behaviors
services.AddEntityCommands<MyDbContext, User, int, UserReadModel, UserCreateModel, UserUpdateModel>();

// MongoDB registration - includes soft delete behaviors
services.AddEntityCommands<IUserRepository, User, int, UserReadModel, UserCreateModel, UserUpdateModel>();
```

### Individual Delete Command Registration

```csharp
// Entity Framework delete command registration
services.AddEntityDeleteCommand<MyDbContext, User, int, UserReadModel>();

// MongoDB delete command registration  
services.AddEntityDeleteCommand<IUserRepository, User, int, UserReadModel>();
```

When you register delete commands, the framework automatically:

- Detects entities implementing `ITrackDeleted` for soft delete behavior
- Registers `DeletedFilterBehavior` to exclude deleted entities from queries
- Configures audit tracking for delete operations

## Query Scenarios

### Include Deleted Entities

For administrative or audit scenarios, create specialized queries that include deleted entities:

```csharp
public class EntityListWithDeletedQuery<TReadModel> : IRequest<IReadOnlyList<TReadModel>>
{
    public EntityFilter? Filter { get; set; }
    public bool IncludeDeleted { get; set; } = true;
}

// Custom behavior that doesn't apply delete filtering
public class AdminQueryBehavior<TRequest, TResponse> : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip delete filtering for admin queries
        return await next().ConfigureAwait(false);
    }
}
```

### Deleted-Only Queries

```csharp
public class DeletedEntitiesQuery<TReadModel> : IRequest<IReadOnlyList<TReadModel>>
{
    public EntityFilter? Filter { get; set; }
}

// Handler that specifically queries deleted entities
public class DeletedEntitiesHandler<TReadModel> : IRequestHandler<DeletedEntitiesQuery<TReadModel>, IReadOnlyList<TReadModel>>
{
    public async Task<IReadOnlyList<TReadModel>> Handle(DeletedEntitiesQuery<TReadModel> request, CancellationToken cancellationToken)
    {
        var filter = new EntityFilter
        {
            Name = nameof(ITrackDeleted.IsDeleted),
            Value = true,
            Operator = FilterOperators.Equal
        };
        
        return await repository.QueryAsync(filter);
    }
}
```

## Restore Operations

### Entity Restoration

```csharp
public class RestoreEntityCommand<TKey> : IRequest<bool>
{
    public TKey Id { get; set; }
    public ClaimsPrincipal Principal { get; set; }
}

public class RestoreEntityHandler<TKey, TEntity> : IRequestHandler<RestoreEntityCommand<TKey>, bool>
    where TEntity : class, ITrackDeleted
{
    public async Task<bool> Handle(RestoreEntityCommand<TKey> request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, includeDeleted: true);
        
        if (entity == null || !entity.IsDeleted)
            return false;
            
        // Restore the entity
        entity.IsDeleted = false;
        
        await repository.UpdateAsync(entity);
        return true;
    }
}
```

## Advanced Configuration

### Custom Delete Filter Implementation

```csharp
public class CustomDeletedFilterBehavior<TEntityModel, TRequest, TResponse> 
    : DeletedFilterBehaviorBase<TEntityModel, TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    public CustomDeletedFilterBehavior(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    protected override EntityFilter? RewriteFilter(EntityFilter? originalFilter, ClaimsPrincipal? principal)
    {
        // Custom logic - e.g., show deleted items to administrators
        if (principal?.IsInRole("Administrator") == true)
            return originalFilter; // Don't filter for admins
            
        return base.RewriteFilter(originalFilter, principal);
    }
}
```

### Conditional Soft Delete

```csharp
public class ConditionalSoftDeleteBehavior<TEntityModel, TRequest, TResponse> 
    : DeletedFilterBehaviorBase<TEntityModel, TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    protected override EntityFilter? RewriteFilter(EntityFilter? originalFilter, ClaimsPrincipal? principal)
    {
        // Only apply soft delete filtering in production
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            return originalFilter;
            
        return base.RewriteFilter(originalFilter, principal);
    }
}
```

## Database Considerations

### Indexing Strategy

```sql
-- Primary index for normal queries (exclude deleted)
CREATE INDEX IX_Users_IsDeleted_Active 
ON Users (IsDeleted) 
WHERE IsDeleted = 0;

-- Index for deleted entity queries
CREATE INDEX IX_Users_IsDeleted_Deleted 
ON Users (IsDeleted) 
WHERE IsDeleted = 1;

-- Composite index for tenant + soft delete scenarios
CREATE INDEX IX_Users_TenantId_IsDeleted 
ON Users (TenantId, IsDeleted) 
WHERE IsDeleted = 0;
```

### Database Schema

```sql
-- Example table with soft delete column
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    
    -- Soft delete tracking
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Creation tracking
    Created DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(100) NULL,
    
    -- Update tracking
    Updated DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy NVARCHAR(100) NULL
);
```

## Best Practices

### Implementation Guidelines

1. **Consistent Interface**: Implement `ITrackDeleted` on all entities requiring soft delete
2. **Database Constraints**: Use database defaults to ensure `IsDeleted` is never null
3. **Index Strategy**: Create appropriate indexes for both active and deleted entity queries
4. **Audit Trail**: Combine with audit behaviors for complete change tracking

### Security Considerations

1. **Permission Checks**: Verify user permissions before allowing delete operations
2. **Audit Logging**: Log all delete and restore operations for security monitoring
3. **Data Retention**: Implement policies for eventual hard deletion of old soft-deleted records
4. **Backup Strategy**: Ensure backup procedures account for soft-deleted data

### Performance Optimization

1. **Index Usage**: Ensure queries use indexes effectively with `IsDeleted = false` conditions
2. **Batch Operations**: Implement efficient batch soft delete operations
3. **Archival Strategy**: Consider moving old deleted records to archive tables
4. **Query Optimization**: Monitor query performance with soft delete filters
