---
title: Quick Start
description: Mediator pattern and Command Query Responsibility Segregation (CQRS) implementation in .NET
---
# Quick Start

Mediator pattern and Command Query Responsibility Segregation (CQRS) implementation in .NET

| Library                                                                     | Package                                                                                                                                                                                  | Description                                                       |
| :-------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------- |
| [Arbiter.Mediation](#arbitermediation)                                      | [![Arbiter.Mediation](https://img.shields.io/nuget/v/Arbiter.Mediation.svg)](https://www.nuget.org/packages/Arbiter.Mediation/)                                                          | Lightweight and extensible implementation of the Mediator pattern |
| [Arbiter.Mediation.OpenTelemetry](#arbitermediationopentelemetry)           | [![Arbiter.Mediation.OpenTelemetry](https://img.shields.io/nuget/v/Arbiter.Mediation.OpenTelemetry.svg)](https://www.nuget.org/packages/Arbiter.Mediation.OpenTelemetry/)                | OpenTelemetry support for Arbiter.Mediation library               |
| [Arbiter.CommandQuery](#arbitercommandquery)                                | [![Arbiter.CommandQuery](https://img.shields.io/nuget/v/Arbiter.CommandQuery.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery/)                                                 | Base package for Commands, Queries and Behaviors                  |
| [Arbiter.CommandQuery.EntityFramework](#arbitercommandqueryentityframework) | [![Arbiter.CommandQuery.EntityFramework](https://img.shields.io/nuget/v/Arbiter.CommandQuery.EntityFramework.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.EntityFramework/) | Entity Framework Core handlers for the base Commands and Queries  |
| [Arbiter.CommandQuery.MongoDB](#arbitercommandquerymongodb)                 | [![Arbiter.CommandQuery.MongoDB](https://img.shields.io/nuget/v/Arbiter.CommandQuery.MongoDB.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.MongoDB/)                         | Mongo DB handlers for the base Commands and Queries               |
| [Arbiter.CommandQuery.Endpoints](#arbitercommandqueryendpoints)             | [![Arbiter.CommandQuery.Endpoints](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Endpoints.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Endpoints/)                   | Minimal API endpoints for base Commands and Queries               |
| [Arbiter.CommandQuery.Mvc](#arbitercommandquerymvc)                         | [![Arbiter.CommandQuery.Mvc](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Mvc.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Mvc/)                                     | MVC Controllers for base Commands and Queries                     |

## Arbiter.Mediation

A lightweight and extensible implementation of the Mediator pattern for .NET applications, designed for clean, modular architectures like Vertical Slice Architecture and CQRS.

### Mediation Installation

The Arbiter Mediation library is available on nuget.org via package name `Arbiter.Mediation`.

To install Arbiter Mediation, run the following command in the Package Manager Console

```powershell
Install-Package Arbiter.Mediation
```

OR

```shell
dotnet add package Arbiter.Mediation
```

### Mediation Features

- Request with response handling using `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`
- Notifications (Events) using `INotification` and `INotificationHandler<TNotification>`
- Pipeline Behaviors, like middleware, using `IPipelineBehavior<TRequest, TResponse>`
- Dependence Injection based resolution of handlers and behaviors via scoped `IServiceProvider`
- Supports OpenTelemetry tracing and meters via `Arbiter.Mediation.OpenTelemetry`

### Define Request

```csharp
public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}
```

### Implement Handler

```csharp
public class PingHandler : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong> Handle(
        Ping request,
        CancellationToken cancellationToken = default)
    {
        // Simulate some work
        await Task.Delay(100, cancellationToken);

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
```

### Define Pipeline Behavior

```csharp
public class PingBehavior : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong> Handle(
        Ping request,
        RequestHandlerDelegate<Pong> next,
        CancellationToken cancellationToken = default)
    {
        // Do something before the request is handled
        var response = await next(cancellationToken);
        // Do something after the request is handled

        return response;
    }
}
```

### Register Handlers

```csharp
// Register Mediator services
services.AddMediator();

// Register handlers
services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

// Optionally register pipeline behaviors, supports multiple behaviors
services.AddTransient<IPipelineBehavior<Ping, Pong>, PingBehavior>();
```

### Send Request

```csharp
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        string? message = null,
        CancellationToken? cancellationToken = default)
    {
        var request = new Ping { Message = message };
        var response = await _mediator.Send<Ping, Pong>(request, cancellationToken);

        return Ok(response);
    }
}
```

## Arbiter.Mediation.OpenTelemetry

OpenTelemetry support for Arbiter.Mediation library

### OpenTelemetry Installation

```powershell
Install-Package Arbiter.Mediation.OpenTelemetry
```

OR

```shell
dotnet add package Arbiter.Mediation.OpenTelemetry
```

### OpenTelemetry Usage

Register via dependency injection

```c#
services.AddMediatorDiagnostics();

services.AddOpenTelemetry()
  .WithTracing(tracing => tracing
    .AddMediatorInstrumentation()
    .AddConsoleExporter()
  )
  .WithMetrics(metrics => metrics
    .AddMediatorInstrumentation()
    .AddConsoleExporter()
  );
```

## Arbiter.CommandQuery

Command Query Responsibility Segregation (CQRS) framework based on mediator pattern

### CommandQuery Installation

The Arbiter Command Query library is available on nuget.org via package name `Arbiter.CommandQuery`.

To install Arbiter Command Query, run the following command in the Package Manager Console

```powershell
Install-Package Arbiter.CommandQuery
```

OR

```shell
dotnet add package Arbiter.CommandQuery
```

### CommandQuery Features

- Base commands and queries for common CRUD operations
- Generics base handlers for implementing common CRUD operations
- Common behaviors for audit, caching, soft delete, multi-tenant
- View model to data modal mapping
- Entity Framework Core handlers for common CRUD operations
- MongoDB handlers for common CRUD operations

### CommandQuery Usage

Register Command Query services via dependency injection

```csharp
services.AddCommandQuery();
```

### Query By ID

```csharp
// sample user claims
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));

var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);

// Send the query to the mediator for execution
var result = await mediator.Send(query);
```

### Query By Filter

```csharp
var filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" };
var sort = new EntitySort { Name = "Name", Direction = "asc" };

var query = new EntitySelectQuery<ProductReadModel>(principal, filter, sort);

// Send the query to the mediator for execution
var result = await mediator.Send(query);
```

### Update Command

```csharp
var id = 123; // The ID of the entity to update
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product",
    Description = "Updated description of the product",
    Price = 29.99m
};

var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, id, updateModel);

// Send the command to the mediator for execution
var result = await mediator.Send(command);
```

## Arbiter.CommandQuery.EntityFramework

Entity Framework Core handlers for the base Commands and Queries

### EntityFramework Installation

```powershell
Install-Package Arbiter.CommandQuery.EntityFramework
```

OR

```shell
dotnet add package Arbiter.CommandQuery.EntityFramework
```

### EntityFramework Usage

Register via dependency injection

```csharp
// Add Entity Framework Core services
services.AddDbContext<TrackerContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("TrackerConnection"))
);

// Register Command Query services
services.AddCommandQuery();

// Implement and register IMapper
services.AddSingleton<IMapper, MyMapper>();

// Implement and register IValidator
services.AddSingleton<IValidator, MyValidator>();

// Register Entity Framework Core handlers for each Entity
services.AddEntityQueries<TrackerContext, Product, int, ProductReadModel>();
services.AddEntityCommands<TrackerContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

## Arbiter.CommandQuery.MongoDB

Mongo DB handlers for the base Commands and Queries

### MongoDB Installation

```powershell
Install-Package Arbiter.CommandQuery.MongoDB
```

OR

```shell
dotnet add package Arbiter.CommandQuery.MongoDB
```

### MongoDB Usage

Register via dependency injection

```csharp
// Add MongoDB Repository services
services.AddMongoRepository("Tracker");

// Register Command Query services
services.AddCommandQuery();

// Implement and register IMapper
services.AddSingleton<IMapper, MyMapper>();

// Implement and register IValidator
services.AddSingleton<IValidator, MyValidator>();

// Register MongoDB handlers for each Entity
services.AddEntityQueries<IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
services.AddEntityCommands<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

## Arbiter.CommandQuery.Endpoints

Minimal API endpoints for base Commands and Queries

### Endpoints Installation

```powershell
Install-Package Arbiter.CommandQuery.Endpoints
```

OR

```shell
dotnet add package Arbiter.CommandQuery.Endpoints
```

### Endpoints Usage

Register via dependency injection

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add endpoints services
builder.Services.AddEndpointRoutes();

var app = builder.Build();

// map endpoint routes
app.MapEndpointRoutes();
```

### Example Endpoint

```csharp
public class ProductEndpoint : EntityCommandEndpointBase<int, ProductReadModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Product")
    {
    }
}

// Register endpoint, must support duplicate (IEnumerable) IEndpointRoute registrations
builder.Services.AddSingleton<IEndpointRoute, ProductEndpoint>();
```

## Arbiter.CommandQuery.Mvc

MVC Controllers for base Commands and Queries

### MVC Installation

```powershell
Install-Package Arbiter.CommandQuery.Mvc
```

OR

```shell
dotnet add package Arbiter.CommandQuery.Mvc
```
