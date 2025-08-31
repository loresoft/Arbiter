# Command Abstracts

The Arbiter framework provides a comprehensive set of abstract base classes for implementing CQRS commands. These base classes follow consistent patterns and provide built-in support for security, auditing, and type safety.

## Overview

All command abstracts in Arbiter follow the CQRS (Command Query Responsibility Segregation) pattern and include:

- **Security Context**: Built-in `ClaimsPrincipal` support for authentication and authorization
- **Audit Trail**: Automatic tracking of who activated the command and when
- **Type Safety**: Strongly-typed generic parameters for keys, models, and responses
- **JSON Serialization**: Full support for serialization with appropriate converters

> **Note**: Arbiter commands implement the `IRequest<TResponse>` interface from `Arbiter.Mediation`, which provides a MediatR-compatible API with additional features specific to the Arbiter framework.

## `PrincipalCommandBase<TResponse>` Class

The foundational base class for all commands that require user context and security information.

```csharp
public abstract record PrincipalCommandBase<TResponse> : IRequest<TResponse>
```

### Core Features

`PrincipalCommandBase` provides the foundation for all commands by:

- Capturing the user's security context through `ClaimsPrincipal`
- Tracking when the command was activated and by whom
- Implementing the `IRequest<TResponse>` interface for Arbiter.Mediation compatibility

### Type Parameters

| Parameter | Description |
|-----------|-------------|
| `TResponse` | The type of the response returned by the command |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Principal` | `ClaimsPrincipal?` | The user's security context |
| `Activated` | `DateTimeOffset` | UTC timestamp when the command was created |
| `ActivatedBy` | `string?` | Username extracted from the principal (defaults to "system") |

### Usage Example

```csharp
public record GetUserDetailsCommand : PrincipalCommandBase<UserDetails>
{
    public GetUserDetailsCommand(ClaimsPrincipal principal) : base(principal)
    {
    }
}

var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
    new Claim(ClaimTypes.Name, "JohnDoe") 
}));
var command = new GetUserDetailsCommand(principal);

var result = await mediator.Send(command);
```

## `EntityIdentifierCommand<TKey, TResponse>` Class

Base class for commands that operate on a single entity identified by a key.

```csharp
public abstract record EntityIdentifierCommand<TKey, TResponse> 
    : PrincipalCommandBase<TResponse>
```

### When to Use

Use `EntityIdentifierCommand` when your command needs to:

- Target a specific entity by its identifier
- Ensure the identifier is not null at compile-time
- Follow consistent patterns for single-entity operations

### Generic Parameters

| Parameter | Description |
|-----------|-------------|
| `TKey` | The type of the key used to identify the entity (e.g., `int`, `Guid`, `string`) |
| `TResponse` | The type of the response returned by the command |

### Available Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `TKey` | The identifier of the target entity (guaranteed non-null) |

### Implementation Example

```csharp
public record GetProductByIdCommand : EntityIdentifierCommand<int, ProductReadModel>
{
    public GetProductByIdCommand(ClaimsPrincipal principal, int id)
        : base(principal, id)
    {
    }
}

var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
    new Claim(ClaimTypes.Name, "JohnDoe") 
}));
var command = new GetProductByIdCommand(principal, 123);

var result = await mediator.Send(command);
Console.WriteLine($"Product Name: {result?.Name}");
```

### Identifier Command Use Cases

- Retrieving a single entity by ID
- Updating an entity by identifier
- Deleting an entity by ID
- Performing operations on a specific entity

## `EntityIdentifiersCommand<TKey, TResponse>` Class

Base class for commands that operate on multiple entities identified by a collection of keys.

```csharp
public abstract record EntityIdentifiersCommand<TKey, TResponse> 
    : PrincipalCommandBase<TResponse>
```

### Identifiers When to Use

Use `EntityIdentifiersCommand` when your command needs to:

- Target multiple entities by their identifiers
- Perform bulk operations efficiently
- Ensure the identifier collection is not null

### Identifiers Generic Parameters

| Parameter | Description |
|-----------|-------------|
| `TKey` | The type of the key used to identify entities |
| `TResponse` | The type of the response returned by the command |

### Identifiers Properties

| Property | Type | Description |
|----------|------|-------------|
| `Ids` | `IReadOnlyCollection<TKey>` | The collection of entity identifiers (guaranteed non-null) |

### Identifiers Implementation Example

```csharp
public record DeleteProductsCommand : EntityIdentifiersCommand<int, int>
{
    public DeleteProductsCommand(ClaimsPrincipal principal, IReadOnlyCollection<int> ids)
        : base(principal, ids)
    {
    }
}

var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
    new Claim(ClaimTypes.Name, "JohnDoe") 
}));
var productIds = new List<int> { 1, 2, 3, 4, 5 };
var command = new DeleteProductsCommand(principal, productIds);

var deletedCount = await mediator.Send(command);
Console.WriteLine($"Deleted {deletedCount} products");
```

### Identifiers Command Use Cases

- Bulk delete operations
- Batch processing of entities
- Multi-select operations
- Archiving multiple records

## `EntityModelCommand<TEntityModel, TReadModel>` Class

Base class for commands that use a data model to perform operations and return a read model.

```csharp
public abstract record EntityModelCommand<TEntityModel, TReadModel> 
    : PrincipalCommandBase<TReadModel>
```

### Design Goals

Use `EntityModelCommand` when your command needs to:

- Accept complex data through a model object
- Transform input models into domain operations
- Return a read model as the result
- Ensure model validation and type safety

### Model Parameters

| Parameter | Description |
|-----------|-------------|
| `TEntityModel` | The type of the input model containing command data |
| `TReadModel` | The type of the read model returned as the result |

### Class Properties

| Property | Type | Description |
|----------|------|-------------|
| `Model` | `TEntityModel` | The input model containing the command data (guaranteed non-null) |

### Model Command Example

```csharp
public record CreateProductCommand : EntityModelCommand<ProductCreateModel, ProductReadModel>
{
    public CreateProductCommand(ClaimsPrincipal principal, ProductCreateModel model)
        : base(principal, model)
    {
    }
}

var createModel = new ProductCreateModel
{
    Name = "New Product",
    Description = "A high-quality product",
    Price = 29.99m,
    CategoryId = 5
};

var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
    new Claim(ClaimTypes.Name, "JohnDoe") 
}));
var command = new CreateProductCommand(principal, createModel);

var result = await mediator.Send(command);
Console.WriteLine($"Created product: {result?.Name} with ID: {result?.Id}");
```

### Model Command Use Cases

- Creating new entities with complex data
- Updating entities with detailed models
- Upserting operations
- Complex business operations requiring multiple fields

## Best Practices

### Security Considerations

1. **Always validate the principal**: Check user permissions in your command handlers
2. **Use strongly-typed claims**: Extract specific claims needed for authorization
3. **Audit sensitive operations**: Leverage the built-in `ActivatedBy` and `Activated` properties

### Type Safety

1. **Use appropriate key types**: Choose `int`, `Guid`, or `string` based on your domain
2. **Define clear model contracts**: Ensure your models have proper validation attributes
3. **Keep models immutable**: Use records and readonly properties where possible

### Performance

1. **Use `EntityIdentifiersCommand` for bulk operations**: More efficient than multiple single-entity commands
2. **Consider pagination**: For large identifier collections, implement paging in your handlers
3. **Cache read models**: Use the `ICacheExpire` interface for cacheable results

## Inheritance Hierarchy

```text
IRequest<TResponse>
└── PrincipalCommandBase<TResponse>
    ├── EntityIdentifierCommand<TKey, TResponse>
    ├── EntityIdentifiersCommand<TKey, TResponse>
    └── EntityModelCommand<TEntityModel, TReadModel>
```
