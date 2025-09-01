---
title: ASP.NET Core Controller
description: Base controller classes for implementing CQRS patterns in ASP.NET Core MVC applications with built-in CRUD operations, pagination, and CSV export functionality.
---

# ASP.NET Core Controller

The **Arbiter.CommandQuery.Mvc** package provides a set of base controller classes that simplify the implementation of CQRS (Command Query Responsibility Segregation) patterns in ASP.NET Core MVC applications. These controllers leverage the mediator pattern to handle commands and queries for entity operations.

## Overview

This package extends the core Arbiter.CommandQuery functionality by providing ready-to-use controller base classes that handle common CRUD operations, pagination, filtering, and CSV export functionality. The controllers follow RESTful conventions and provide consistent API endpoints for entity management.

## Key Features

- **Base Controller Classes**: Ready-to-use controllers for common CQRS operations
- **RESTful API Endpoints**: Standard HTTP methods for CRUD operations
- **Pagination Support**: Built-in support for paged queries
- **CSV Export**: Export functionality for entity data
- **JSON Patch Support**: Partial updates using JSON Patch
- **Exception Handling**: JSON-formatted error responses

## Controller Base Classes

### MediatorControllerBase

The foundation controller that provides access to the mediator pattern.

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class MediatorControllerBase : ControllerBase
{
    protected MediatorControllerBase(IMediator mediator)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public IMediator Mediator { get; }
}
```

### EntityQueryControllerBase

Provides read-only operations for entities including:

- **GET /api/[controller]/{id}** - Retrieve a single entity by ID
- **GET /api/[controller]** - Query entities with filtering and sorting
- **POST /api/[controller]/query** - Query entities using request body
- **GET /api/[controller]/page** - Paginated entity retrieval
- **POST /api/[controller]/page** - Paginated queries using request body

```csharp
public abstract class EntityQueryControllerBase<TKey, TListModel, TReadModel> 
    : MediatorControllerBase
{
    // Implementation provides query endpoints
}
```

### EntityCommandControllerBase

Extends query functionality with full CRUD operations:

- **GET /api/[controller]/{id}/update** - Get update model for entity
- **POST /api/[controller]** - Create new entity
- **POST /api/[controller]/{id}** - Upsert (create or update) entity
- **PUT /api/[controller]/{id}** - Update existing entity
- **PATCH /api/[controller]/{id}** - Partial update using JSON Patch
- **DELETE /api/[controller]/{id}** - Delete entity

```csharp
public abstract class EntityCommandControllerBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityQueryControllerBase<TKey, TListModel, TReadModel>
{
    // Implementation provides CRUD endpoints
}
```

### Export Controllers

For CSV export functionality:

#### EntityExportQueryControllerBase

Adds CSV export to query operations:

- **POST /api/[controller]/export** - Export query results as CSV
- **GET /api/[controller]/export** - Export with query parameters as CSV

#### EntityExportCommandControllerBase

Combines command operations with CSV export functionality.

## Usage Example

Here's how to create a controller for managing products:

```csharp
[Route("api/[controller]")]
public class ProductController : EntityCommandControllerBase<int, ProductListModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductController(IMediator mediator) : base(mediator)
    {
    }
}
```

This provides a complete REST API for product management:

```http
GET    /api/product/123           # Get product by ID
GET    /api/product               # List products with filtering
POST   /api/product/query         # Query products
GET    /api/product/page          # Paginated products
POST   /api/product/page          # Paginated query
GET    /api/product/123/update    # Get update model
POST   /api/product               # Create product
POST   /api/product/123           # Upsert product
PUT    /api/product/123           # Update product
PATCH  /api/product/123           # Partial update
DELETE /api/product/123           # Delete product
```

## CSV Export Example

For controllers that need CSV export functionality:

```csharp
[Route("api/[controller]")]
public class ProductController : EntityExportCommandControllerBase<int, ProductListModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductController(IMediator mediator) : base(mediator)
    {
    }
}
```

This adds export endpoints:

```http
POST /api/product/export?fileName=products.csv    # Export with query
GET  /api/product/export?encodedQuery=...&fileName=products.csv
```

## Middleware Components

### JsonExceptionMiddleware

Provides centralized exception handling with JSON-formatted error responses:

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseMiddleware<JsonExceptionMiddleware>();
    // Other middleware...
}
```

Features:

- Converts exceptions to RFC 7807 Problem Details format
- Includes trace IDs for debugging
- Hides sensitive information in production
- Handles `DomainException` with custom status codes

### BaseAddressResolver

Resolves base addresses for API endpoints with multiple fallback options:

```csharp
services.AddScoped<IBaseAddressResolver, BaseAddressResolver>();
```

Resolution order:

1. Blazor NavigationManager (if available)
2. HTTP context (current request)
3. Configuration ("BaseAddress" key)

## HTTP Status Codes

The controllers follow standard HTTP status code conventions:

- **200 OK** - Successful operation
- **400 Bad Request** - Validation errors
- **404 Not Found** - Entity not found (handled by underlying queries)
- **500 Internal Server Error** - Server errors

## JSON Patch Support

The `PATCH` endpoints support JSON Patch operations for partial updates:

```json
[
  { "op": "replace", "path": "/name", "value": "New Product Name" },
  { "op": "remove", "path": "/description" }
]
```

## Query Parameters

### Filtering and Sorting

The controllers support query parameters for filtering and sorting:

```http
GET /api/product?q=category:electronics&sort=name
GET /api/product/page?q=price>100&sort=name&page=2&size=25
```

### Pagination

Pagination parameters:

- `page` - Page number (1-based)
- `size` - Page size (default: 20)
- `q` - Query/filter expression
- `sort` - Sort expression

## Dependencies

The package requires:

- **Arbiter.CommandQuery** - Core CQRS functionality
- **Microsoft.AspNetCore.App** - ASP.NET Core framework
- **SystemTextJsonPatch** - JSON Patch support (for PATCH operations)

## Configuration

Add the controllers to your dependency injection:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddScoped<IBaseAddressResolver, BaseAddressResolver>();
    
    // Add your specific controllers
    services.AddScoped<ProductController>();
}
```

## Best Practices

1. **Generic Type Parameters**: Use meaningful model types for better API documentation
2. **Authorization**: Add authorization attributes to controllers as needed
3. **Validation**: Implement validation in your command/query handlers
4. **Error Handling**: Use the provided JsonExceptionMiddleware for consistent error responses
5. **Export Models**: Ensure export models implement `ISupportWriter<T>` for CSV functionality
