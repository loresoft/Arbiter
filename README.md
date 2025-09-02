# Arbiter

**A powerful, lightweight, and extensible implementation of the Mediator pattern and Command Query Responsibility Segregation (CQRS) for .NET applications.**

Arbiter is designed for building clean, modular architectures like Vertical Slice Architecture and CQRS. It provides a comprehensive suite of libraries that work together to simplify complex application patterns while maintaining high performance and flexibility.

[![Build Status](https://github.com/loresoft/Arbiter/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Arbiter/actions)
[![License](https://img.shields.io/github/license/loresoft/Arbiter.svg)](LICENSE)

## Key Features

- **Clean Architecture**: Perfect for Vertical Slice Architecture and CQRS patterns
- **High Performance**: Minimal overhead with efficient mediator implementation
- **Extensible**: Pipeline behaviors, custom handlers, and extensive customization options
- **Observable**: Built-in OpenTelemetry support for tracing and metrics
- **Database Agnostic**: Support for Entity Framework Core, MongoDB, and more
- **Web Ready**: Minimal API endpoints and MVC controller support
- **Communication**: Integrated email and SMS messaging capabilities

## Table of Contents

- [Quick Start](#quick-start)
- [Packages](#packages)
- [Core Libraries](#core-libraries)
  - [Arbiter.Mediation](#arbitermediation)
  - [Arbiter.CommandQuery](#arbitercommandquery)
  - [Arbiter.Communication](#arbitercommunication)
- [Data Providers](#data-providers)
- [Web Integration](#web-integration)
- [Documentation](#documentation)
- [Samples](#samples)
- [Contributing](#contributing)
- [License](#license)

## Quick Start

Get started with Arbiter in just a few steps:

### 1. Install the Core Package

```bash
dotnet add package Arbiter.Mediation
```

### 2. Define a Request and Handler

```csharp
// Define your request
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

// Implement the handler
public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async ValueTask<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your business logic here
        return await GetUserFromDatabase(request.UserId);
    }
}
```

### 3. Register Services

```csharp
services.AddMediator();
services.AddTransient<IRequestHandler<GetUserQuery, User>, GetUserHandler>();
```

### 4. Use the Mediator

```csharp
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<User> GetUser(int id)
    {
        return await _mediator.Send(new GetUserQuery { UserId = id });
    }
}
```

## Packages

### Core Libraries Packages

| Library                                        | Package                                                                                                                                     | Description                                                       |
| :--------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------- |
| [Arbiter.Mediation](#arbitermediation)         | [![Arbiter.Mediation](https://img.shields.io/nuget/v/Arbiter.Mediation.svg)](https://www.nuget.org/packages/Arbiter.Mediation/)             | Lightweight and extensible implementation of the Mediator pattern |
| [Arbiter.CommandQuery](#arbitercommandquery)   | [![Arbiter.CommandQuery](https://img.shields.io/nuget/v/Arbiter.CommandQuery.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery/)    | Base package for Commands, Queries and Behaviors                  |
| [Arbiter.Communication](#arbitercommunication) | [![Arbiter.Communication](https://img.shields.io/nuget/v/Arbiter.Communication.svg)](https://www.nuget.org/packages/Arbiter.Communication/) | Message template communication for email and SMS services         |

### Data Providers Packages

| Library                                                                     | Package                                                                                                                                                                                  | Description                                                      |
| :-------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------- |
| [Arbiter.CommandQuery.EntityFramework](#arbitercommandqueryentityframework) | [![Arbiter.CommandQuery.EntityFramework](https://img.shields.io/nuget/v/Arbiter.CommandQuery.EntityFramework.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.EntityFramework/) | Entity Framework Core handlers for the base Commands and Queries |
| [Arbiter.CommandQuery.MongoDB](#arbitercommandquerymongodb)                 | [![Arbiter.CommandQuery.MongoDB](https://img.shields.io/nuget/v/Arbiter.CommandQuery.MongoDB.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.MongoDB/)                         | MongoDB handlers for the base Commands and Queries               |

### Web Integration Packages

| Library                                                         | Package                                                                                                                                                                | Description                                         |
| :-------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- |
| [Arbiter.CommandQuery.Endpoints](#arbitercommandqueryendpoints) | [![Arbiter.CommandQuery.Endpoints](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Endpoints.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Endpoints/) | Minimal API endpoints for base Commands and Queries |
| [Arbiter.CommandQuery.Mvc](#arbitercommandquerymvc)             | [![Arbiter.CommandQuery.Mvc](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Mvc.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Mvc/)                   | MVC Controllers for base Commands and Queries       |

### Observability Packages

| Library                                                           | Package                                                                                                                                                                   | Description                                         |
| :---------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------------------- |
| [Arbiter.Mediation.OpenTelemetry](#arbitermediationopentelemetry) | [![Arbiter.Mediation.OpenTelemetry](https://img.shields.io/nuget/v/Arbiter.Mediation.OpenTelemetry.svg)](https://www.nuget.org/packages/Arbiter.Mediation.OpenTelemetry/) | OpenTelemetry support for Arbiter.Mediation library |

### Communication Providers Packages

| Library                                                     | Package                                                                                                                                                          | Description                                                   |
| :---------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------ |
| [Arbiter.Communication.Azure](#arbitercommunicationazure)   | [![Arbiter.Communication.Azure](https://img.shields.io/nuget/v/Arbiter.Communication.Azure.svg)](https://www.nuget.org/packages/Arbiter.Communication.Azure/)    | Communication implementation for Azure Communication Services |
| [Arbiter.Communication.Twilio](#arbitercommunicationtwilio) | [![Arbiter.Communication.Twilio](https://img.shields.io/nuget/v/Arbiter.Communication.Twilio.svg)](https://www.nuget.org/packages/Arbiter.Communication.Twilio/) | Communication implementation for SendGrid and Twilio          |

## Core Libraries

### Arbiter.Mediation

A lightweight and extensible implementation of the Mediator pattern for .NET applications, designed for clean, modular architectures like Vertical Slice Architecture and CQRS.

#### Mediation Features

- **Request/Response Pattern**: Handle requests with typed responses using `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`
- **Notifications/Events**: Publish events using `INotification` and `INotificationHandler<TNotification>`
- **Pipeline Behaviors**: Middleware-like cross-cutting concerns using `IPipelineBehavior<TRequest, TResponse>`
- **Dependency Injection**: Seamless integration with .NET's DI container
- **High Performance**: Minimal allocations and efficient execution
- **OpenTelemetry Ready**: Built-in observability support

#### Installation

```bash
dotnet add package Arbiter.Mediation
```

#### Basic Usage

**1. Define Request and Response**

```csharp
public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}

public class Pong
{
    public string? Message { get; set; }
}
```

**2. Implement Handler**

```csharp
public class PingHandler : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong> Handle(
        Ping request,
        CancellationToken cancellationToken = default)
    {
        // Simulate async work
        await Task.Delay(100, cancellationToken);

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
```

**3. Define Pipeline Behavior (Optional)**

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        
        var response = await next(cancellationToken);
        
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        
        return response;
    }
}
```

**4. Register Services**

```csharp
// Register Mediator services
services.AddMediator();

// Register handlers
services.AddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

// Register pipeline behaviors (optional)
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

**5. Use in Controllers**

```csharp
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PingController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<Pong>> Get(
        [FromQuery] string? message = null,
        CancellationToken cancellationToken = default)
    {
        var request = new Ping { Message = message ?? "Hello" };
        var response = await _mediator.Send(request, cancellationToken);
        
        return Ok(response);
    }
}
```

### Arbiter.Mediation.OpenTelemetry

Comprehensive observability support for Arbiter.Mediation with OpenTelemetry integration.

#### OpenTelemetry Installation

```bash
dotnet add package Arbiter.Mediation.OpenTelemetry
```

#### OpenTelemetry Features

- **Distributed Tracing**: Automatic tracing of all mediator operations
- **Metrics**: Built-in metrics for request duration, throughput, and errors
- **Activity Enrichment**: Rich contextual information in traces
- **Zero Configuration**: Works out of the box with minimal setup

#### Usage

```csharp
// Register diagnostics
services.AddMediatorDiagnostics();

// Configure OpenTelemetry
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

### Arbiter.CommandQuery

A comprehensive Command Query Responsibility Segregation (CQRS) framework built on top of the mediator pattern.

#### CommandQuery Features

- **CQRS Implementation**: Clear separation between commands and queries
- **Pre-built Operations**: Common CRUD operations out of the box
- **Generic Handlers**: Reusable handlers for typical data operations
- **Smart Behaviors**: Caching, auditing, validation, and soft delete support
- **Auto Mapping**: Built-in view model to data model mapping
- **Advanced Querying**: Filter, sort, and pagination support
- **Multi-tenancy Ready**: Built-in tenant isolation support

#### Package Installation

```bash
dotnet add package Arbiter.CommandQuery
```

#### Service Registration

```csharp
services.AddCommandQuery();
```

#### Built-in Commands and Queries

The library provides several pre-built commands and queries for common operations:

**Entity Queries:**

- `EntityIdentifierQuery<TKey, TReadModel>` - Get entity by ID
- `EntitySelectQuery<TReadModel>` - Query entities with filtering and sorting
- `EntityPagedQuery<TReadModel>` - Paginated entity queries

**Entity Commands:**

- `EntityCreateCommand<TKey, TCreateModel, TReadModel>` - Create new entities
- `EntityUpdateCommand<TKey, TUpdateModel, TReadModel>` - Update existing entities
- `EntityUpsertCommand<TKey, TUpsertModel, TReadModel>` - Create or update entities
- `EntityDeleteCommand<TKey, TReadModel>` - Delete entities

#### Example Usage

**Query by ID:**

```csharp
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
    new Claim(ClaimTypes.Name, "JohnDoe") 
}));

var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);
var result = await mediator.Send(query);
```

**Query with Filtering:**

```csharp
var filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" };
var sort = new EntitySort { Name = "Name", Direction = "asc" };

var query = new EntitySelectQuery<ProductReadModel>(principal, filter, sort);
var result = await mediator.Send(query);
```

**Update Command:**

```csharp
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product",
    Description = "Updated description",
    Price = 29.99m
};

var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, 123, updateModel);
var result = await mediator.Send(command);
```

## Data Providers

### Arbiter.CommandQuery.EntityFramework

Entity Framework Core integration providing ready-to-use handlers for all base commands and queries.

#### CommandQuery EntityFramework Installation

```bash
dotnet add package Arbiter.CommandQuery.EntityFramework
```

#### CommandQuery EntityFramework Features

- **Complete CRUD Operations**: Pre-built handlers for all entity operations
- **Change Tracking**: Automatic audit fields and soft delete support  
- **Optimized Queries**: Efficient EF Core query patterns
- **Transaction Support**: Proper transaction management
- **Bulk Operations**: Support for bulk insert/update operations

#### Setup

```csharp
// Add Entity Framework Core services
services.AddDbContext<TrackerContext>(options =>
    options.UseSqlServer(connectionString)
);

// Register Command Query services
services.AddCommandQuery();

// Register mappers and validators
services.AddSingleton<IMapper, MyMapper>();
services.AddSingleton<IValidator, MyValidator>();

// Register Entity Framework handlers for each entity
services.AddEntityQueries<TrackerContext, Product, int, ProductReadModel>();
services.AddEntityCommands<TrackerContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

### Arbiter.CommandQuery.MongoDB

MongoDB integration providing handlers for all base commands and queries with document database optimizations.

```bash
dotnet add package Arbiter.CommandQuery.MongoDB
```

#### MongoDB Setup

```csharp
// Add MongoDB Repository services
services.AddMongoRepository("Tracker");
services.AddCommandQuery();

// Register mappers and validators
services.AddSingleton<IMapper, MyMapper>();
services.AddSingleton<IValidator, MyValidator>();

// Register MongoDB handlers for each entity
services.AddEntityQueries<IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
services.AddEntityCommands<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

## Web Integration

### Arbiter.CommandQuery.Endpoints

Minimal API endpoints that automatically expose your commands and queries as REST APIs.

```bash
dotnet add package Arbiter.CommandQuery.Endpoints
```

#### Endpoints Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add endpoint services
builder.Services.AddEndpointRoutes();

var app = builder.Build();

// Map endpoint routes
app.MapEndpointRoutes();

app.Run();
```

**Custom Endpoint:**

```csharp
public class ProductEndpoint : EntityCommandEndpointBase<int, ProductReadModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Product")
    {
    }
}

// Register the endpoint
builder.Services.AddSingleton<IEndpointRoute, ProductEndpoint>();
```

### Arbiter.CommandQuery.Mvc

MVC Controllers for base Commands and Queries with full ASP.NET Core integration.

```bash
dotnet add package Arbiter.CommandQuery.Mvc
```

## Communication

### Arbiter.Communication

A flexible message templating system for email and SMS communications with support for multiple providers.

```bash
dotnet add package Arbiter.Communication
```

#### Communication Providers

**Azure Communication Services:**

```bash
dotnet add package Arbiter.Communication.Azure
```

**SendGrid and Twilio:**

```bash
dotnet add package Arbiter.Communication.Twilio
```

## Documentation

- **[Complete Documentation](https://loresoft.github.io/Arbiter/)** - Comprehensive guides and API reference
- **[Quick Start Guide](https://loresoft.github.io/Arbiter/guide/quickStart.html)** - Get up and running quickly
- **[Architecture Patterns](https://loresoft.github.io/Arbiter/guide/patterns.html)** - Best practices and patterns

## Samples

Explore practical examples in the [samples](./samples/) directory:

- **[Entity Framework Sample](./samples/EntityFramework/)** - Complete CRUD operations with EF Core
- **[MongoDB Sample](./samples/MongoDB/)** - Document database implementation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (git checkout -b feature/amazing-feature)
3. Commit your changes (git commit -m 'Add amazing feature')
4. Push to the branch (git push origin feature/amazing-feature)
5. Open a Pull Request

## License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

## Support

If you find this project useful, please consider:

- **Starring** the repository
- **Reporting** issues
- **Contributing** improvements
- **Spreading** the word
