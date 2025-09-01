---
title: Multi-Tenant Behaviors
description: Pipeline behaviors that enforce tenant isolation and data security in multi-tenant applications
---

# Multi-Tenant Behaviors

Pipeline behaviors that enforce tenant isolation and data security in multi-tenant applications by automatically applying tenant-specific filtering and validation. These behaviors work in conjunction with entity commands to provide seamless multi-tenant capabilities.

## Overview

The Arbiter framework provides automatic multi-tenant functionality through pipeline behaviors that work with entities implementing the `IHaveTenant<TKey>` interface. These behaviors ensure tenant isolation at both the query and command levels.

**Key Features:**

- **Automatic Tenant Detection**: Commands and queries automatically detect entities implementing `IHaveTenant<TKey>`
- **Query Filtering**: Automatically applies tenant-based filtering to queries ensuring data isolation
- **Data Isolation**: Entities are automatically filtered by tenant context
- **Security by Default**: Prevents cross-tenant data access through automatic validation
- **Command Integration**: Works seamlessly with `EntityCreateCommand`, `EntityUpdateCommand`, and `EntityDeleteCommand`

## TenantFilterBehavior

The `TenantFilterBehavior` behavior automatically applies tenant-based filtering to queries, ensuring that each tenant can only access their own data. This is essential for maintaining data isolation and security in multi-tenant applications.

### Automatic Query Filtering

The behavior automatically modifies queries to include tenant filtering:

```csharp
// When you execute a query
var users = await mediator.Send(new EntityListQuery<UserReadModel>(principal));

// The behavior automatically adds: WHERE TenantId = @currentTenantId
// So only entities belonging to the current tenant are returned
```

### Required Entity Interface

Your entities must implement the `IHaveTenant<TKey>` interface to enable tenant filtering:

```csharp
public interface IHaveTenant<TKey>
{
    TKey TenantId { get; set; }
}
```

**Purpose**: Enables multi-tenant data isolation by associating entities with specific tenants

**Usage**: When queries are executed on entities implementing this interface, results are automatically filtered by the current user's tenant context

### Example Entity Implementation

```csharp
public class User : IHaveTenant<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Tenant property
    public int TenantId { get; set; }
}

// Entity with combined audit and tenant tracking
public class Order : ITrackCreated, ITrackUpdated, IHaveTenant<int>
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
    
    // Tenant property
    public int TenantId { get; set; }
}
```

### Filter Logic

1. **Automatic Detection**: Checks if entity implements `IHaveTenant<TKey>`
2. **Tenant Resolution**: Extracts current tenant ID from user context using `ITenantResolver<TKey>`
3. **Filter Injection**: Adds `TenantId = @currentTenantId` condition to queries
4. **Query Transparency**: Works without modifying existing query handlers

## Entity Commands with Multi-Tenant Support

### TenantDefaultCommandBehavior

The `TenantDefaultCommandBehavior` behavior automatically sets the tenant identifier on entities during create operations and validates tenant context during update and delete operations.

#### For Create Commands (`EntityCreateCommand`)

Automatically sets `TenantId` on new entities if not specified:

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity([
    new Claim(ClaimTypes.Name, "johndoe"),
    new Claim("tenant_id", "5")
]));

var createCommand = new EntityCreateCommand<UserCreateModel, UserReadModel>(principal, createModel);
var createdUser = await mediator.Send(createCommand);

// Automatic tenant assignment:
// - TenantId = 5 (from user's tenant context)
```

#### For Update Commands (`EntityUpdateCommand`)

Validates that the entity being updated belongs to the current user's tenant:

```csharp
var updateCommand = new EntityUpdateCommand<int, UserUpdateModel, UserReadModel>(principal, userId, updateModel);
var updatedUser = await mediator.Send(updateCommand);

// Automatic tenant validation:
// - Verifies entity with userId belongs to tenant 5
// - Throws security exception if tenant mismatch
```

#### For Delete Commands (`EntityDeleteCommand`)

Validates tenant context before allowing deletion:

```csharp
var deleteCommand = new EntityDeleteCommand<int, UserReadModel>(principal, userId);
var deletedUser = await mediator.Send(deleteCommand);

// Automatic tenant validation:
// - Verifies entity with userId belongs to tenant 5  
// - Prevents cross-tenant deletion attempts
```

### TenantAuthenticateCommandBehavior

### Security Validation

- **Command Validation**: Verifies tenant context for all tenant-aware commands
- **Cross-Tenant Protection**: Prevents unauthorized access to other tenants' data
- **Context Verification**: Ensures command tenant matches current user's tenant
- **Early Termination**: Stops command processing if tenant validation fails

## Service Registration

### Automatic Registration with Entity Commands

The multi-tenant behaviors are automatically registered when using entity command registration methods:

```csharp
// Entity Framework registration - includes multi-tenant behaviors
services.AddEntityCommands<MyDbContext, User, int, UserReadModel, UserCreateModel, UserUpdateModel>();

// MongoDB registration - includes multi-tenant behaviors
services.AddEntityCommands<IUserRepository, User, int, UserReadModel, UserCreateModel, UserUpdateModel>();
```

### Tenant Resolver Registration

Multi-tenant behaviors require an `ITenantResolver<TKey>` implementation:

```csharp
// Register tenant resolver
services.AddScoped<ITenantResolver<int>, ClaimsTenantResolver>();

// Individual command registration with tenant support
services.AddEntityCreateCommand<MyDbContext, User, int, UserCreateModel, UserReadModel>();
services.AddEntityUpdateCommand<MyDbContext, User, int, UserUpdateModel, UserReadModel>();
services.AddEntityDeleteCommand<MyDbContext, User, int, UserReadModel>();
```

When you register entity commands, the framework automatically:

- Detects entities implementing `IHaveTenant<TKey>` for multi-tenant behavior
- Registers `TenantFilterBehavior` to filter queries by tenant
- Configures `TenantDefaultCommandBehavior` for automatic tenant assignment
- Enables `TenantAuthenticateCommandBehavior` for security validation

## Tenant Resolver Implementation

### ITenantResolver Interface

Multi-tenant behaviors depend on an `ITenantResolver<TKey>` implementation to determine the current tenant context:

```csharp
public interface ITenantResolver<TKey>
{
    ValueTask<TKey> GetTenantId(ClaimsPrincipal? principal);
    ValueTask<string?> GetTenantName(ClaimsPrincipal? principal);
}
```

### Example Implementation

```csharp
public class ClaimsTenantResolver : ITenantResolver<int>
{
    public ValueTask<int> GetTenantId(ClaimsPrincipal? principal)
    {
        if (principal?.FindFirst("tenant_id")?.Value is string tenantIdValue
            && int.TryParse(tenantIdValue, out int tenantId))
        {
            return ValueTask.FromResult(tenantId);
        }
        
        throw new UnauthorizedAccessException("No valid tenant context found");
    }
    
    public ValueTask<string?> GetTenantName(ClaimsPrincipal? principal)
    {
        return ValueTask.FromResult(principal?.FindFirst("tenant_name")?.Value);
    }
}
```
