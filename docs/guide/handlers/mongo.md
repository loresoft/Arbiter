# MongoDB Handlers

The MongoDB handlers provide a comprehensive CQRS (Command Query Responsibility Segregation) implementation for MongoDB, enabling you to perform standardized CRUD operations with built-in support for caching, auditing, validation, and security using the MongoDB.Abstracts library.

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
Install-Package Arbiter.CommandQuery.MongoDB
```

OR

```shell
dotnet add package Arbiter.CommandQuery.MongoDB
```

## Prerequisites

Before using the MongoDB handlers, ensure you have:

1. **MongoDB Database**: A configured MongoDB database connection
2. **Repository Classes**: Custom repository classes that inherit from `MongoEntityRepository<TEntity>`
3. **Entity Classes**: Domain entities that implement `IHaveIdentifier<TKey>` and inherit from `MongoEntity`
4. **Model Classes**: Data transfer objects for create, update, and read operations
5. **Mapper**: Implementation of `IMapper` for object mapping
6. **Validator** (optional): Implementation of `IValidator` for business rule validation

## Basic Setup

### 1. Configure MongoDB Services

```csharp
// Configure MongoDB connection
services.AddSingleton<IMongoClient>(provider => new MongoClient(connectionString));

services.AddSingleton<IMongoDatabase>(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    return client.GetDatabase("TrackerDB");
});

// Add MongoDB Repository services
services.AddMongoRepository();
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

### 3. Register Repository Classes

```csharp
// Register custom repositories
services.AddSingleton<IProductRepository, ProductRepository>();
// OR using generic repository
services.AddSingleton<IMongoEntityRepository<Product>, MongoEntityRepository<Product>>();
```

### 4. Register Entity Handlers

#### Complete CRUD Operations

```csharp
// Register both queries and commands for an entity
services.AddEntityQueries<IProductRepository, Product, string, ProductReadModel>();
services.AddEntityCommands<IProductRepository, Product, string, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

#### Individual Registration

```csharp
// Register only specific operations
services.AddEntityCreateCommand<IProductRepository, Product, string, ProductReadModel, ProductCreateModel>();
services.AddEntityUpdateCommand<IProductRepository, Product, string, ProductReadModel, ProductUpdateModel>();
services.AddEntityDeleteCommand<IProductRepository, Product, string, ProductReadModel>();
services.AddEntityUpsertCommand<IProductRepository, Product, string, ProductReadModel, ProductUpdateModel>();
services.AddEntityPatchCommand<IProductRepository, Product, string, ProductReadModel>();
```

## Entity Requirements

### Basic Entity Interface

Your entities must inherit from `MongoEntity` and implement `IHaveIdentifier<TKey>`:

```csharp
public class Product : MongoEntity, IHaveIdentifier<string>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    // ... other properties
}
```

### Enhanced Entity Features

#### Audit Tracking

```csharp
public class Product : MongoEntity, IHaveIdentifier<string>, ITrackCreated, ITrackUpdated
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    
    // Audit fields
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
}
```

#### Soft Delete Support

```csharp
public class Product : MongoEntity, IHaveIdentifier<string>, ITrackDeleted
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }
    // ... other properties
}
```

#### Multi-Tenant Support

```csharp
public class Product : MongoEntity, IHaveIdentifier<string>, IHaveTenant<string>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string TenantId { get; set; } = string.Empty;
    // ... other properties
}
```

## Repository Implementation

### Custom Repository

```csharp
public interface IProductRepository : IMongoRepository<Product, string>
{
    // Add custom methods if needed
}

[RegisterSingleton]
public class ProductRepository : MongoEntityRepository<Product>, IProductRepository
{
    public ProductRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
    {
    }

    protected override void EnsureIndexes(IMongoCollection<Product> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        // Create custom indexes
        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Name)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.IsDeleted)
            )
        );
    }
}
```

### Generic Repository (Alternative)

```csharp
// Use the generic repository directly
services.AddSingleton<IMongoEntityRepository<Product>>(provider =>
{
    var database = provider.GetRequiredService<IMongoDatabase>();
    return new MongoEntityRepository<Product>(database);
});
```

## Model Classes

### Read Model

```csharp
public class ProductReadModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
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
    public string Category { get; set; } = string.Empty;
}
```

### Update Model

```csharp
public class ProductUpdateModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
```

## Usage Examples

### Query Operations

#### Get Single Entity by ID

```csharp
var query = new EntityIdentifierQuery<string, ProductReadModel>(principal, productId);
var product = await mediator.Send(query);
```

#### Get Multiple Entities by IDs

```csharp
var ids = new[] { "64b8f2a1c5d4e8f9a0b1c2d3", "64b8f2a1c5d4e8f9a0b1c2d4" };
var query = new EntityIdentifiersQuery<string, ProductReadModel>(principal, ids);
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
    Price = 1299.99m,
    Category = "Electronics"
};

var command = new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel);
var newProduct = await mediator.Send(command);
```

#### Update Entity

```csharp
var updateModel = new ProductUpdateModel 
{ 
    Name = "Updated Gaming Laptop", 
    Price = 1199.99m,
    Category = "Electronics"
};

var command = new EntityUpdateCommand<string, ProductUpdateModel, ProductReadModel>(principal, productId, updateModel);
var updatedProduct = await mediator.Send(command);
```

#### Upsert Entity (Create or Update)

```csharp
var updateModel = new ProductUpdateModel 
{ 
    Name = "Gaming Laptop", 
    Price = 1299.99m,
    Category = "Electronics"
};

var command = new EntityUpsertCommand<string, ProductUpdateModel, ProductReadModel>(principal, productId, updateModel);
var product = await mediator.Send(command);
```

#### Patch Entity

```csharp
var patchDocument = new JsonPatchDocument();
patchDocument.Replace("/price", 999.99m);

var command = new EntityPatchCommand<string, ProductReadModel>(principal, productId, patchDocument);
var patchedProduct = await mediator.Send(command);
```

#### Delete Entity

```csharp
var command = new EntityDeleteCommand<string, ProductReadModel>(principal, productId);
var deletedProduct = await mediator.Send(command);
```

## Pipeline Behaviors

The MongoDB handlers automatically integrate with the same pipeline behaviors as Entity Framework:

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
services.AddEntityQueryMemoryCache<string, ProductReadModel>();

// Enable distributed caching
services.AddEntityQueryDistributedCache<string, ProductReadModel>();

// Enable hybrid caching
services.AddEntityHybridCache<string, ProductReadModel>();
```

### Multi-Tenant Setup

```csharp
// Register tenant-aware behaviors
services.AddEntityQueries<IProductRepository, Product, string, ProductReadModel>();
// Tenant filtering is automatically applied if entity implements IHaveTenant<TKey>
```

### MongoDB-Specific Configuration

#### Connection Settings

```csharp
services.AddSingleton<IMongoClient>(provider =>
{
    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.MaxConnectionPoolSize = 500;
    settings.MinConnectionPoolSize = 5;
    settings.WaitQueueTimeout = TimeSpan.FromSeconds(30);
    return new MongoClient(settings);
});
```

#### Collection Configuration

```csharp
public class ProductRepository : MongoEntityRepository<Product>
{
    protected override void ConfigureCollection(IMongoCollection<Product> collection)
    {
        base.ConfigureCollection(collection);
        
        // Additional collection configuration
        // Custom serializers, conventions, etc.
    }
}
```

## Error Handling

The handlers provide consistent error handling:

- **Entity Not Found**: Returns `null` for queries, throws for commands requiring existing entities
- **Validation Errors**: Throws `ValidationException` with detailed error information
- **MongoDB Exceptions**: Propagates MongoDB-specific exceptions (connection issues, write conflicts)
- **Domain Exceptions**: Propagates custom `DomainException` instances

## Performance Considerations

### Query Optimization

- Use proper indexing in your repository's `EnsureIndexes` method
- Implement efficient filtering at the database level using MongoDB query operators
- Consider using projection to read models to reduce data transfer

### Caching Strategy

- Enable appropriate caching based on your read/write patterns
- Use memory cache for frequently accessed, small datasets
- Use distributed cache for larger datasets or multi-instance deployments

### MongoDB Optimization

- Create appropriate indexes for filtered and sorted fields
- Use connection pooling for high-throughput scenarios
- Consider read preferences for read-heavy workloads
- Implement proper sharding strategies for large datasets

## Best Practices

1. **Implement Custom Repositories**: Create specific repository classes for each entity to encapsulate MongoDB-specific logic
2. **Design Proper Indexes**: Use the `EnsureIndexes` method to create optimal indexes for your queries
3. **Use MongoDB ObjectIds**: Consider using MongoDB's ObjectId as the primary key type (string representation)
4. **Keep Models Simple**: Use separate models for different operations (create, update, read)
5. **Implement Proper Mapping**: Ensure your `IMapper` implementation handles MongoDB-specific types correctly
6. **Use Audit Interfaces**: Implement audit interfaces for automatic tracking of changes
7. **Leverage Soft Deletes**: Use `ITrackDeleted` for recoverable delete operations
8. **Configure Connection Properly**: Set appropriate connection pool sizes and timeouts
9. **Handle ObjectId Conversion**: Ensure proper conversion between ObjectId and string representations
10. **Consider Multi-Tenancy**: Use tenant interfaces if your application serves multiple tenants

## MongoDB vs Entity Framework Differences

### Key Differences

- **Primary Keys**: MongoDB typically uses string-based ObjectIds instead of integer keys
- **Repository Pattern**: MongoDB uses the repository pattern directly, not DbContext
- **Schema-less**: MongoDB is document-based and schema-less, providing more flexibility
- **Indexing**: Manual index creation through repository configuration
- **Transactions**: MongoDB supports transactions but with different semantics than SQL databases
