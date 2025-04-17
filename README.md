# Arbiter

Mediator pattern and Command Query Responsibility Segregation (CQRS) implementation in .NET

| Library                                                                     | Package                                                                                                                                                                                  | Description                                                       |
| :-------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------- |
| [Arbiter.Mediation](#arbitermediation)                                      | [![Arbiter.Mediation](https://img.shields.io/nuget/v/Arbiter.Mediation.svg)](https://www.nuget.org/packages/Arbiter.Mediation/)                                                          | Lightweight and extensible implementation of the Mediator pattern |
| [Arbiter.CommandQuery](#arbitercommandquery)                                | [![Arbiter.CommandQuery](https://img.shields.io/nuget/v/Arbiter.CommandQuery.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery/)                                                 | Base package for Commands, Queries and Behaviours                 |
| [Arbiter.CommandQuery.EntityFramework](#arbitercommandqueryentityframework) | [![Arbiter.CommandQuery.EntityFramework](https://img.shields.io/nuget/v/Arbiter.CommandQuery.EntityFramework.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.EntityFramework/) | Entity Framework Core handlers for the base Commands and Queries  |
| [Arbiter.CommandQuery.MongoDB](#arbitercommandquerymongodb)                 | [![Arbiter.CommandQuery.MongoDB](https://img.shields.io/nuget/v/Arbiter.CommandQuery.MongoDB.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.MongoDB/)                         | Mongo DB handlers for the base Commands and Queries               |
| [Arbiter.CommandQuery.Endpoints](#arbitercommandqueryendpoints)             | [![Arbiter.CommandQuery.Endpoints](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Endpoints.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Endpoints/)                   | Minimal API endpoints for base Commands and Queries               |

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
- Dependence Injection based

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
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register Arbiter services
        services.AddArbiter();

        // Register handlers
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        // Optionally register pipeline behaviors, supports multiple behaviors
        services.AddTransient<IPipelineBehavior<Ping, Pong>, PingBehavior>();
    }
}
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
