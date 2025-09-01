---
title: Minimal API Endpoints
description: Base endpoint classes for implementing CQRS patterns using ASP.NET Core Minimal APIs with built-in CRUD operations, routing, and exception handling.
---

# Minimal API Endpoints

The **Arbiter.CommandQuery.Endpoints** package provides a set of base endpoint classes that simplify the implementation of CQRS (Command Query Responsibility Segregation) patterns using ASP.NET Core Minimal APIs. These endpoints leverage the mediator pattern to handle commands and queries for entity operations using a functional approach.

## Overview

This package extends the core Arbiter.CommandQuery functionality by providing ready-to-use endpoint base classes that handle common CRUD operations, pagination, filtering, and routing functionality. The endpoints follow RESTful conventions and provide consistent API patterns while leveraging the performance benefits of Minimal APIs.

## Key Features

- **Minimal API Endpoints**: Lightweight, high-performance API endpoints
- **RESTful Conventions**: Standard HTTP methods for CRUD operations
- **Functional Approach**: Route handlers defined as methods rather than controller actions
- **Built-in Routing**: Automatic route configuration with customizable prefixes
- **Exception Handling**: Problem Details format for consistent error responses
- **Base Address Resolution**: Flexible base address resolution for API endpoints
- **OpenAPI Integration**: Automatic swagger documentation generation

## Endpoint Base Classes

### IEndpointRoute Interface

The foundation interface for defining endpoint routes that enables modular and composable API design:

```csharp
public interface IEndpointRoute
{
    void AddRoutes(IEndpointRouteBuilder endpoints);
}
```

**Key Characteristics:**

- **Modular Design**: Each endpoint class implements this interface to define its own routes
- **Composable Architecture**: Multiple endpoint implementations can be registered and automatically discovered
- **Dependency Injection Friendly**: Works seamlessly with ASP.NET Core's service container
- **Route Group Support**: Enables logical grouping of related endpoints under common prefixes
- **Extensible**: Custom implementations can add specialized routing logic

**Registration Patterns:**

```csharp
// Single registration
services.AddScoped<IEndpointRoute, ProductEndpoint>();

// Multiple registrations for different entities
services.AddScoped<IEndpointRoute, ProductEndpoint>();
services.AddScoped<IEndpointRoute, CustomerEndpoint>();
services.AddScoped<IEndpointRoute, OrderEndpoint>();
```

**Automatic Discovery:**

The `MapEndpointRoutes` extension method automatically discovers all registered `IEndpointRoute` implementations and calls their `AddRoutes` method:

```csharp
// Discovers and maps all registered IEndpointRoute implementations
app.MapEndpointRoutes("/api");

// This internally does:
// var endpointRoutes = serviceProvider.GetServices<IEndpointRoute>();
// foreach (var route in endpointRoutes)
//     route.AddRoutes(routeGroupBuilder);
```

**Implementation Example:**

Here's a simple example of implementing `IEndpointRoute` directly for custom routing scenarios:

```csharp
public class HealthCheckEndpoint : IEndpointRoute
{
    private readonly ILogger<HealthCheckEndpoint> _logger;

    public HealthCheckEndpoint(ILogger<HealthCheckEndpoint> logger)
    {
        _logger = logger;
    }

    public void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("health");

        group.MapGet("", GetHealth)
            .WithName("GetHealth")
            .WithSummary("Get application health status")
            .WithTags("Health");

        group.MapGet("ready", GetReadiness)
            .WithName("GetReadiness")
            .WithSummary("Get application readiness status")
            .WithTags("Health");
    }

    private async Task<IResult> GetHealth()
    {
        _logger.LogInformation("Health check requested");
        return TypedResults.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    private async Task<IResult> GetReadiness()
    {
        // Perform readiness checks (database connectivity, external services, etc.)
        var isReady = await CheckDatabaseConnection();
        
        return isReady 
            ? TypedResults.Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow })
            : TypedResults.Problem("Service not ready", statusCode: 503);
    }

    private async Task<bool> CheckDatabaseConnection()
    {
        // Implementation for database connectivity check
        await Task.Delay(10); // Simulate check
        return true;
    }
}
```

This custom endpoint can be registered and will be automatically discovered:

```csharp
services.AddScoped<IEndpointRoute, HealthCheckEndpoint>();
```

When mapped, it creates endpoints like:

- `GET /api/health` - Returns health status
- `GET /api/health/ready` - Returns readiness status

### EntityQueryEndpointBase

Provides read-only operations for entities including:

- **GET /{prefix}/{id}** - Retrieve a single entity by ID
- **GET /{prefix}** - Query entities with filtering and sorting
- **POST /{prefix}/query** - Query entities using request body
- **GET /{prefix}/page** - Paginated entity retrieval
- **POST /{prefix}/page** - Paginated queries using request body

```csharp
public abstract class EntityQueryEndpointBase<TKey, TListModel, TReadModel> : IEndpointRoute
{
    protected EntityQueryEndpointBase(ILoggerFactory loggerFactory, string entityName, string? routePrefix = null)
    {
        // Implementation provides query endpoints
    }
}
```

### EntityCommandEndpointBase

Extends query functionality with full CRUD operations:

- **GET /{prefix}/{id}/update** - Get update model for entity
- **POST /{prefix}** - Create new entity
- **POST /{prefix}/{id}** - Upsert (create or update) entity
- **PUT /{prefix}/{id}** - Update existing entity
- **PATCH /{prefix}/{id}** - Partial update using JSON Patch
- **DELETE /{prefix}/{id}** - Delete entity

```csharp
public abstract class EntityCommandEndpointBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityQueryEndpointBase<TKey, TListModel, TReadModel>
{
    // Implementation provides CRUD endpoints
}
```

## Real-World Example

The [MongoDB sample project](https://github.com/loresoft/Arbiter/tree/main/samples/MongoDB/Tracker.WebService) demonstrates real usage of the endpoints. Here's the `TaskEndpoint` implementation:

```csharp
[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class TaskEndpoint : EntityCommandEndpointBase<string, TaskReadModel, TaskReadModel, TaskCreateModel, TaskUpdateModel>
{
    public TaskEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Task")
    { }

    protected override void MapGroup(RouteGroupBuilder group)
    {
        base.MapGroup(group);

        // Add custom endpoint in addition to standard CRUD operations
        group
            .MapGet("Process", GetProcessCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}Process")
            .WithSummary("Get entity process action")
            .WithDescription("Get entity process action");
    }

    private async Task<Results<Ok<CompleteModel>, ProblemHttpResult>> GetProcessCommand(
       [FromServices] IMediator mediator,
       [FromQuery] string? action = null,
       ClaimsPrincipal? user = default,
       CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new TaskProcessCommand(user, action!);
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error GetProcessCommand: {ErrorMessage}", ex.Message);
            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }
}
```

This example shows how to:

- Extend the base endpoint with custom operations
- Use dependency injection attributes for registration
- Follow the same error handling patterns
- Add custom metadata for OpenAPI documentation

## Usage Example

Here's how to create endpoints for managing products:

```csharp
public class ProductEndpoint : EntityCommandEndpointBase<int, ProductListModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductEndpoint(ILoggerFactory loggerFactory) 
        : base(loggerFactory, "Product", "products")
    {
    }
}
```

Register the endpoint in your application:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddEndpointRoutes();
    services.AddScoped<IEndpointRoute, ProductEndpoint>();
}

public void Configure(WebApplication app)
{
    app.MapEndpointRoutes("/api");
}
```

This provides a complete REST API for product management:

```http
GET    /api/products/123          # Get product by ID
GET    /api/products              # List products with filtering
POST   /api/products/query        # Query products
GET    /api/products/page         # Paginated products
POST   /api/products/page         # Paginated query
GET    /api/products/123/update   # Get update model
POST   /api/products              # Create product
POST   /api/products/123          # Upsert product
PUT    /api/products/123          # Update product
PATCH  /api/products/123          # Partial update
DELETE /api/products/123          # Delete product
```

## Registration and Configuration

### Service Registration

Add the required services to your dependency injection container:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add core endpoint services
    services.AddEndpointRoutes();
    
    // Register your endpoint implementations
    services.AddScoped<IEndpointRoute, ProductEndpoint>();
    services.AddScoped<IEndpointRoute, CustomerEndpoint>();    
}
```

### Endpoint Mapping

Map the endpoints in your application startup:

```csharp
public void Configure(WebApplication app)
{
    // Map all registered endpoint routes
    app.MapEndpointRoutes("/api");
    
    // Alternative: Map with custom prefix
    app.MapEndpointRoutes("/v1/api");
    
    // Alternative: Map with service key
    app.MapEndpointRoutes("/v2/api", "version-v2");
}
```

## Exception Handling

### ProblemDetailsCustomizer

The package includes a customizer for converting exceptions to Problem Details format:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddProblemDetails(options =>
    {
        ProblemDetailsCustomizer.Configure(options);
    });
}
```

Features:

- Converts `ValidationException` to 400 Bad Request with validation errors
- Handles `DomainException` with custom status codes and error details
- Includes trace IDs for debugging
- Shows exception details in development environments
- Follows RFC 7807 Problem Details standard

### Automatic Error Handling

All endpoint methods automatically handle exceptions and return appropriate Problem Details responses:

```csharp
protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> GetQuery(
    [FromServices] IMediator mediator,
    [FromRoute] TKey id,
    ClaimsPrincipal? user = default,
    CancellationToken cancellationToken = default)
{
    try
    {
        var command = new EntityIdentifierQuery<TKey, TReadModel>(user, id);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error GetQuery: {ErrorMessage}", ex.Message);
        var details = ex.ToProblemDetails();
        return TypedResults.Problem(details);
    }
}
```

## Route Customization

### Route Handler Builder Extensions

The package provides extensions for adding common metadata to endpoints:

```csharp
public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithEntityMetadata(this RouteHandlerBuilder builder, string entityName)
    {
        return builder
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithTags(entityName);
    }
}
```

### Custom Route Prefixes

You can customize route prefixes when creating endpoints:

```csharp
public class ProductEndpoint : EntityCommandEndpointBase<int, ProductListModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductEndpoint(ILoggerFactory loggerFactory) 
        : base(loggerFactory, "Product", "inventory/products") // Custom prefix
    {
    }
}
```

## Query Parameters

### Filtering and Sorting

The endpoints support query parameters for filtering and sorting:

```http
GET /api/products?q=category:electronics&sort=name
GET /api/products/page?q=price>100&sort=name&page=2&size=25
```

### Pagination

Pagination parameters:

- `page` - Page number (1-based, default: 1)
- `size` - Page size (default: 20)
- `q` - Query/filter expression
- `sort` - Sort expression

## HTTP Status Codes

The endpoints follow standard HTTP status code conventions:

- **200 OK** - Successful operation
- **400 Bad Request** - Validation errors
- **404 Not Found** - Entity not found (handled by underlying queries)
- **500 Internal Server Error** - Server errors
- **499 Client Closed Request** - Request cancellation

## JSON Patch Support

The `PATCH` endpoints support JSON Patch operations for partial updates:

```json
[
  { "op": "replace", "path": "/name", "value": "New Product Name" },
  { "op": "remove", "path": "/description" }
]
```

## OpenAPI Integration

The endpoints automatically generate OpenAPI documentation with:

- Proper operation IDs (e.g., `GetProduct`, `CreateProduct`)
- Request/response models
- Status code documentation
- Entity-based tags for grouping
- Summary and description metadata

## Base Address Resolution

Similar to the MVC package, the endpoints include base address resolution:

```csharp
services.AddScoped<IBaseAddressResolver, BaseAddressResolver>();
```

Resolution order:

1. Blazor `NavigationManager` (if available)
2. HTTP context (current request)
3. Configuration ("BaseAddress" key)

## Dependencies

The package requires:

- **Arbiter.CommandQuery** - Core CQRS functionality
- **Microsoft.AspNetCore.App** - ASP.NET Core framework
- **SystemTextJsonPatch** - JSON Patch support (for PATCH operations)

## Performance Benefits

Minimal APIs provide several performance advantages:

- **Reduced Memory Allocation** - No controller instantiation overhead
- **Faster Routing** - Direct method delegation
- **AOT Friendly** - Better support for ahead-of-time compilation
- **Smaller Binary Size** - Reduced framework dependencies

## Best Practices

1. **Entity Naming**: Use clear, consistent entity names for better API documentation
2. **Route Prefixes**: Use meaningful route prefixes that align with your domain model
3. **Logging**: Leverage the built-in `ILogger` for consistent logging across endpoints
4. **Authorization**: Add authorization policies using standard ASP.NET Core mechanisms
5. **Validation**: Implement validation in your command/query handlers
6. **Error Handling**: Use the provided `ProblemDetailsCustomizer` for consistent error responses
7. **Performance**: Leverage Minimal APIs for high-performance scenarios
8. **Documentation**: Use OpenAPI attributes for enhanced API documentation
