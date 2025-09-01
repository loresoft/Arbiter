---
title: Entity Framework Core Handlers
description: Comprehensive CQRS implementation for Entity Framework Core with built-in support for caching, auditing, validation, and security
---

# Entity Framework Core Handlers

The Entity Framework Core handlers provide a comprehensive CQRS (Command Query Responsibility Segregation) implementation for Entity Framework Core, enabling you to perform standardized CRUD operations with built-in support for caching, auditing, validation, and security.

## Overview

This package provides pre-built handlers that implement the standard entity operations:

### Query Handlers

- **`EntityIdentifierQueryHandler`** - Retrieve a single entity by its identifier
- **`EntityIdentifiersQueryHandler`** - Retrieve multiple entities by their identifiers  
- **`EntityPagedQueryHandler`** - Retrieve entities with pagination, filtering, and sorting
- **`EntitySelectQueryHandler`** - Retrieve entities with custom filtering and sorting

### Command Handlers

- **`EntityCreateCommandHandler`** - Create new entities with automatic audit tracking
- **`EntityUpdateCommandHandler`** - Update existing entities with change tracking
- **`EntityUpsertCommandHandler`** - Create or update entities (insert or update)
- **`EntityPatchCommandHandler`** - Apply partial updates using JSON patch documents
- **`EntityDeleteCommandHandler`** - Delete entities with soft delete support

## Installation

```powershell
Install-Package Arbiter.CommandQuery.EntityFramework
```

OR

```shell
dotnet add package Arbiter.CommandQuery.EntityFramework
```

## Prerequisites

Before using the Entity Framework handlers, ensure you have:

1. **DbContext**: A configured Entity Framework Core `DbContext`
2. **Entity Classes**: Domain entities that implement `IHaveIdentifier<TKey>`
3. **Model Classes**: Data transfer objects for create, update, and read operations
4. **Mapper**: Implementation of `IMapper` for object mapping
5. **Validator** (optional): Implementation of `IValidator` for business rule validation

## Basic Setup

### 1. Configure Entity Framework Core

```csharp
// Configure your DbContext
services.AddDbContext<TrackerContext>(options =>
    options.UseSqlServer(connectionString)
);
```

### 2. Register Core Services

```csharp
// Register Command Query services
services.AddCommandQuery();

// Register your mapper implementation
services.AddSingleton<IMapper, MyMapper>();

// Register your validator implementation (optional)
services.AddSingleton<IValidator, MyValidator>();
```

### 3. Register Entity Handlers

#### Complete CRUD Operations

```csharp
// Register both queries and commands for an entity
services.AddEntityQueries<TrackerContext, Product, int, ProductReadModel>();
services.AddEntityCommands<TrackerContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

#### Individual Registration

```csharp
// Register only specific operations
services.AddEntityCreateCommand<TrackerContext, Product, int, ProductReadModel, ProductCreateModel>();
services.AddEntityUpdateCommand<TrackerContext, Product, int, ProductReadModel, ProductUpdateModel>();
services.AddEntityDeleteCommand<TrackerContext, Product, int, ProductReadModel>();
services.AddEntityUpsertCommand<TrackerContext, Product, int, ProductReadModel, ProductUpdateModel>();
services.AddEntityPatchCommand<TrackerContext, Product, int, ProductReadModel>();
```

## Entity Requirements

### Basic Entity Interface

Your entities must implement `IHaveIdentifier<TKey>`:

```csharp
public class Product : IHaveIdentifier<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // ... other properties
}
```

### Enhanced Entity Features

#### Audit Tracking

```csharp
public class Product : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Audit fields
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
}
```

#### Soft Delete Support

```csharp
public class Product : IHaveIdentifier<int>, ITrackDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    // ... other properties
}
```

#### Multi-Tenant Support

```csharp
public class Product : IHaveIdentifier<int>, IHaveTenant<int>
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    // ... other properties
}
```

## Model Classes

### Read Model

```csharp
public class ProductReadModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
}
```

### Create Model

```csharp
public class ProductCreateModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### Update Model

```csharp
public class ProductUpdateModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

## Usage Examples

### Query Operations

#### Get Single Entity by ID

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, productId);
var product = await mediator.Send(query);
```

#### Get Multiple Entities by IDs

```csharp
var ids = new[] { 1, 2, 3 };
var query = new EntityIdentifiersQuery<int, ProductReadModel>(principal, ids);
var products = await mediator.Send(query);
```

#### Paged Query with Filtering

```csharp
var entityQuery = new EntityQuery
{
    Page = 1,
    PageSize = 20,
    Sort = new[] { new EntitySort { Name = "created", Direction = "desc" } },
    Filter = new EntityFilter { Name = "price", Operator = ">=", Value = 100 }
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query);
```

#### Select with Custom Filtering

```csharp
var entitySelect = new EntitySelect
{
    Filter = new EntityFilter 
    { 
        Name = "category", 
        Value = "electronics" 
    },
    Sort = new[] { new EntitySort { Name = "name" } }
};

var query = new EntitySelectQuery<ProductReadModel>(principal, entitySelect);
var products = await mediator.Send(query);
```

### Command Operations

#### Create Entity

```csharp
var createModel = new ProductCreateModel 
{ 
    Name = "Gaming Laptop", 
    Price = 1299.99m 
};

var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);
var newProduct = await mediator.Send(command);
```

#### Update Entity

```csharp
var updateModel = new ProductUpdateModel 
{ 
    Name = "Updated Gaming Laptop", 
    Price = 1199.99m 
};

var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, productId, updateModel);
var updatedProduct = await mediator.Send(command);
```

#### Upsert Entity (Create or Update)

```csharp
var updateModel = new ProductUpdateModel 
{ 
    Name = "Gaming Laptop", 
    Price = 1299.99m 
};

var command = new EntityUpsertCommand<int, ProductUpdateModel, ProductReadModel>(principal, productId, updateModel);
var product = await mediator.Send(command);
```

#### Patch Entity

```csharp
var patchDocument = new JsonPatchDocument();
patchDocument.Replace("/price", 999.99m);

var command = new EntityPatchCommand<int, ProductReadModel>(principal, productId, patchDocument);
var patchedProduct = await mediator.Send(command);
```

#### Delete Entity

```csharp
var command = new EntityDeleteCommand<int, ProductReadModel>(principal, productId);
var deletedProduct = await mediator.Send(command);
```

## Pipeline Behaviors

The Entity Framework handlers automatically integrate with various pipeline behaviors:

### Audit Behaviors

- **Create Audit**: Automatically sets `Created`, `CreatedBy`, `Updated`, and `UpdatedBy` fields
- **Update Audit**: Updates `Updated` and `UpdatedBy` fields on modifications
- **Change Notifications**: Publishes `EntityChangeNotification` events after successful operations

### Cache Behaviors

- **Memory Cache**: Caches query results in memory for improved performance
- **Distributed Cache**: Caches query results in distributed cache (Redis, SQL Server, etc.)
- **Hybrid Cache**: Combines memory and distributed caching strategies
- **Cache Invalidation**: Automatically expires cache entries when entities are modified

### Security Behaviors

- **Multi-Tenant**: Automatically filters entities by tenant ID
- **Soft Delete**: Filters out deleted entities from query results
- **Principal Context**: Provides security context for all operations

### Validation Behaviors

- **Entity Validation**: Validates entities using registered `IValidator` implementations
- **Business Rules**: Enforces domain-specific business rules

## Advanced Configuration

### Custom Behaviors

```csharp
// Add custom behaviors to the pipeline
services.AddTransient<IPipelineBehavior<EntityCreateCommand<ProductCreateModel, ProductReadModel>, ProductReadModel>, CustomProductValidationBehavior>();
```

### Cache Configuration

```csharp
// Enable memory caching for entity queries
services.AddEntityQueryMemoryCache<int, ProductReadModel>();

// Enable distributed caching
services.AddEntityQueryDistributedCache<int, ProductReadModel>();

// Enable hybrid caching
services.AddEntityHybridCache<int, ProductReadModel>();
```

### Multi-Tenant Setup

```csharp
// Register tenant-aware behaviors
services.AddEntityQueries<TrackerContext, Product, int, ProductReadModel>();
// Tenant filtering is automatically applied if entity implements IHaveTenant<TKey>
```

## Error Handling

The handlers provide consistent error handling:

- **Entity Not Found**: Returns `null` for queries, throws for commands requiring existing entities
- **Validation Errors**: Throws `ValidationException` with detailed error information
- **Concurrency Conflicts**: Throws `DbUpdateConcurrencyException` for concurrent modifications
- **Domain Exceptions**: Propagates custom `DomainException` instances

## Performance Considerations

### Query Optimization

- Use `EntitySelectQuery` instead of `EntityPagedQuery` when you don't need pagination
- Implement efficient filtering at the database level
- Consider using projection to read models to reduce data transfer

### Caching Strategy

- Enable appropriate caching based on your read/write patterns
- Use memory cache for frequently accessed, small datasets
- Use distributed cache for larger datasets or multi-instance deployments

### Database Optimization

- Ensure proper indexes for filtered and sorted columns
- Use connection pooling for high-throughput scenarios
- Consider read replicas for read-heavy workloads

## Best Practices

1. **Keep Models Simple**: Use separate models for different operations (create, update, read)
2. **Implement Proper Mapping**: Ensure your `IMapper` implementation handles all property mappings correctly
3. **Use Audit Interfaces**: Implement audit interfaces for automatic tracking of changes
4. **Leverage Soft Deletes**: Use `ITrackDeleted` for recoverable delete operations
5. **Configure Caching Appropriately**: Choose caching strategies based on your specific use cases
6. **Handle Validation**: Implement comprehensive validation using the `IValidator` interface
7. **Consider Multi-Tenancy**: Use tenant interfaces if your application serves multiple tenants
