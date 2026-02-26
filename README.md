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
- **Blazor Ready**: First-class Dispatcher support for Blazor Auto, Server Interactive, and WebAssembly render modes

## Table of Contents

- [Quick Start](#quick-start)
- [Packages](#packages)
- [Core Libraries](#core-libraries)
  - [Arbiter.Mediation](#arbitermediation)
  - [Arbiter.CommandQuery](#arbitercommandquery)
  - [Arbiter.Communication](#arbitercommunication)
- [Data Providers](#data-providers)
- [Web Integration](#web-integration)
- [Blazor Dispatcher](#blazor-dispatcher)
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
    public async ValueTask<User?> Handle(GetUserQuery request, CancellationToken cancellationToken)
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

### Blazor Dispatcher Packages

| Library                                                       | Package                                                                                                                                                             | Description                                                                        |
| :------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :--------------------------------------------------------------------------------- |
| [Arbiter.Dispatcher.Server](#arbiterdispatcherserver)         | [![Arbiter.Dispatcher.Server](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Server.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Server/)             | ASP.NET Core endpoint that receives dispatcher messages from Blazor WASM clients   |
| [Arbiter.Dispatcher.Client](#arbiterdispatcherclient)         | [![Arbiter.Dispatcher.Client](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Client.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Client/)             | Client-side dispatcher for Blazor: JSON/MessagePack (WASM) and ServerDispatcher    |

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
    public async ValueTask<Pong?> Handle(
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
- **Smart Behaviors**: Hybrid caching, auditing, validation, and soft delete support
- **Auto Mapping**: Built-in view model to data model mapping
- **Enhanced Querying**: Powerful filter, sort, and pagination support with type-safe operators
- **Multi-tenancy Ready**: Built-in tenant isolation support
- **Unified Query System**: Single `EntityQuery` class handles both paged and non-paged scenarios
- **Flexible Filtering**: Support for complex filter expressions with multiple operators and logic combinations

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
- `EntityIdentifiersQuery<TKey, TReadModel>` - Get multiple entities by IDs
- `EntityPagedQuery<TReadModel>` - Queries both paged and non-paged scenarios with filtering and sorting

**Entity Commands:**

- `EntityCreateCommand<TKey, TCreateModel, TReadModel>` - Create new entities
- `EntityUpdateCommand<TKey, TUpdateModel, TReadModel>` - Update existing entities (includes upsert)
- `EntityPatchCommand<TKey, TReadModel>` - Partial updates to entities
- `EntityDeleteCommand<TKey, TReadModel>` - Delete entities

#### Example Usage

**Query by ID:**

```csharp
var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);
var result = await mediator.Send(query);
```

**Query with Filtering and Sorting:**

```csharp
var filters = new List<EntityFilter>
{
    new EntityFilter { Name = "Status", Operator = FilterOperators.Equal, Value = "Active" },
    new EntityFilter { Name = "Price", Operator = FilterOperators.GreaterThan, Value = 10.00m }
};

var sorts = new List<EntitySort>
{
    new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
};

var query = new EntityQuery
{
    Filter = new EntityFilter { Filters = filters },
    Sort = sorts
};

// no page or page size will return all matches
var command = new EntityPagedQuery<ProductReadModel>(principal, query);
var result = await mediator.Send(command);
```

**Paginated Query:**

```csharp
var entityQuery = new EntityQuery
{
    Filter = new EntityFilter 
    { 
        Name = "Category", 
        Operator = FilterOperators.Equal, 
        Value = "Electronics" 
    },
    Sort = new List<EntitySort>
    {
        new EntitySort 
        { 
            Name = "CreatedDate", 
            Direction = SortDirections.Descending 
        }
    },
    Page = 1,
    PageSize = 20
};

var query = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
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

#### Advanced Filtering and Querying

**Complex Filter Logic:**

```csharp
var complexEntityQuery = new EntityQuery
{
    Filter = new EntityFilter
    {
        Filters = new List<EntityFilter>
        {
            new EntityFilter 
            { 
                Name = "Category", 
                Operator = FilterOperators.In, 
                Value = new[] { "Electronics", "Computers" } 
            },
            new EntityFilter 
            { 
                Name = "Price", 
                Operator = FilterOperators.GreaterThanOrEqual, 
                Value = 100.00m 
            },
            new EntityFilter 
            { 
                Name = "Name", 
                Operator = FilterOperators.Contains, 
                Value = "Gaming" 
            }
        },
        Logic = FilterLogic.And
    },
    Sort = new List<EntitySort>
    {
        new EntitySort { Name = "Price", Direction = SortDirections.Descending },
        new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
    }
};

var query = new EntityPagedQuery<ProductReadModel>(principal, complexEntityQuery);
var result = await mediator.Send(query);
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

## Blazor Dispatcher

The Dispatcher libraries provide a unified `IDispatcher` abstraction for sending commands and queries from Blazor components, with full support for **Blazor Auto render mode**. Components depend only on `IDispatcher`; the correct transport is wired at startup based on the render environment.

| Render mode | Implementation | Transport |
| :--- | :--- | :--- |
| WebAssembly | `MessagePackDispatcher` / `JsonDispatcher` | HTTP POST to `/api/dispatcher/send` |
| Server Interactive | `ServerDispatcher` | Direct `IMediator` call (in-process) |

### Arbiter.Dispatcher.Server

Provides the `DispatcherEndpoint`—a single `POST /api/dispatcher/send` HTTP endpoint that deserializes incoming requests (JSON or MessagePack), resolves the target handler via `IMediator`, and streams the response back to the WASM client.

```bash
dotnet add package Arbiter.Dispatcher.Server
```

#### Server Setup

```csharp
// Program.cs — Blazor host project
builder.Services.AddDispatcherService();

// ...

app.MapDispatcherService().RequireAuthorization();
```

### Arbiter.Dispatcher.Client

Provides `IDispatcher` and its implementations, plus `DispatcherDataService`, `ModelStateManager<TModel>`, `ModelStateLoader<TKey, TModel>`, and `ModelStateEditor<TKey, TReadModel, TUpdateModel>` for Blazor component state management.

```bash
dotnet add package Arbiter.Dispatcher.Client
```

#### Client Setup — WebAssembly

```csharp
// Program.cs — WASM client project
builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });
```

#### Client Setup — Server Interactive

```csharp
// Program.cs — Blazor host project
builder.Services.AddServerDispatcher();
```

#### Client Setup — Blazor Auto render mode

Register both dispatchers so each render environment resolves `IDispatcher` correctly:

```csharp
// Host project: server-side rendering + serves WASM clients
builder.Services.AddServerDispatcher();
builder.Services.AddDispatcherService();

// WASM client project: WebAssembly rendering
builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });
```

#### Sending commands and queries

Inject `IDispatcher` or the higher-level `IDispatcherDataService` into any component or service:

```csharp
// Low-level: IDispatcher
var query = new EntityIdentifierQuery<int, UserReadModel>(principal, userId);
var user = await dispatcher.Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
    query, cancellationToken);

// High-level: IDispatcherDataService
var user = await dataService.Get<int, UserReadModel>(userId);
var page = await dataService.Page<UserReadModel>(entityQuery);
var saved = await dataService.Save<int, UserUpdateModel, UserReadModel>(userId, updateModel);
```

#### Component state management

`ModelStateEditor` manages the full load–edit–save–delete lifecycle for a Blazor edit form:

```csharp
@inject ModelStateEditor<int, UserReadModel, UserUpdateModel> Store
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    Store.OnStateChanged += (_, _) => InvokeAsync(StateHasChanged);

    if (IsCreate)
        Store.New();
    else
        await Store.Load(Id);
}

// Store.IsBusy  — true while a load/save/delete is in progress
// Store.IsDirty — true when the model has unsaved changes
// Store.Model   — the editable update model bound to the form
// Store.Save()  / Store.Delete() / Store.Cancel()

public void Dispose() => Store.OnStateChanged -= HandleStateChanged;
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
- **[Blazor Dispatcher Overview](https://loresoft.github.io/Arbiter/guide/dispatcher/overview.html)** - Dispatcher architecture, WASM and Server Interactive setup
- **[Dispatcher Server](https://loresoft.github.io/Arbiter/guide/dispatcher/server.html)** - Server endpoint configuration, security, and diagnostics
- **[Dispatcher Client](https://loresoft.github.io/Arbiter/guide/dispatcher/client.html)** - Client registration, sending commands and queries
- **[State Management](https://loresoft.github.io/Arbiter/guide/dispatcher/state.html)** - ModelStateManager, ModelStateLoader, ModelStateEditor

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

## Change Log

### Version 2.0

#### Major Breaking Changes and Improvements

##### Breaking Changes

- **Removed `EntitySelectQuery`**: Replaced with enhanced `EntityQuery` that now supports both paged and non-paged results
- **Removed `EntityUpsertCommand`**: Upsert functionality has been unified into `EntityUpdateCommand` with built-in upsert logic
- **Removed `EntityContinuationQuery` and `EntityContinuationResult`**: Functionality now integrated into `EntityQuery`
- **Command/Query Reorganization**:
  - Moved query classes (`EntityIdentifierQuery`, `EntityIdentifiersQuery`, `EntityPagedQuery`) to `Commands` namespace for better organization
  - Renamed base classes for consistency:
    - `EntityIdentifierCommand` → `EntityIdentifierBase`
    - `EntityIdentifiersCommand` → `EntityIdentifiersBase`
    - `EntityModelCommand` → `EntityModelBase`
  - Renamed filter, logic, and sort values for more consistent query building
    - `EntityFilterOperators` → `FilterOperators`
    - `EntityFilterLogic` → `FilterLogic`
    - `EntitySortDirections` → `SortDirections`

##### Architecture Improvements

- **Simplified Query System**:
  - Consolidated multiple query types into a single, more powerful `EntityQuery` class
  - Removed redundant query handlers and behaviors
  - Enhanced filter and sort capabilities with better type safety

- **Enhanced Filtering**:
  - Moved filter logic to dedicated `Queries.FilterLogic` and `Queries.FilterOperators` enums
  - Improved `EntityFilterConverter` with better validation and error handling
  - Enhanced `LinqExpressionBuilder` with more robust query building capabilities

- **Caching Simplification**:
  - Removed `DistributedCacheQueryBehavior` and `MemoryCacheQueryBehavior`
  - Consolidated caching logic into `HybridCacheQueryBehavior` for better performance
  - Removed `IDistributedCacheSerializer` interface in favor of built-in serialization

##### Behavior Consolidation

- **Tenant Behaviors**:
  - Removed `TenantFilterBehaviorBase` and `TenantSelectQueryBehavior`
  - Enhanced `TenantPagedQueryBehavior` to handle all tenant-related query filtering
  
- **Soft Delete Behaviors**:
  - Removed `DeletedFilterBehaviorBase` and `DeletedSelectQueryBehavior`
  - Enhanced `DeletedPagedQueryBehavior` to handle all soft delete scenarios

- **Removed Legacy Behaviors**:
  - `TrackChangeCommandBehavior` - functionality moved to handlers
  - Various base behavior classes that were no longer needed

#### Migration from Version 1.x

**Updating Query Usage:**

```csharp
// Version 1.x - EntitySelectQuery (REMOVED)
var oldQuery = new EntitySelectQuery<ProductReadModel>(principal, filter, sort);

// Version 2.0 - Use EntityPagedQuery with EntityQuery instead
var entityQuery = new EntityQuery
{
    Filter = filter,
    Sort = sorts
};
var newQuery = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
```

**Updating Filter Operators:**

```csharp
// Version 1.x - String operators (DEPRECATED)
var oldFilter = new EntityFilter 
{ 
    Name = "Status", 
    Operator = "eq", 
    Value = "Active" 
};

// Version 2.0 - Enum operators
var newFilter = new EntityFilter 
{ 
    Name = "Status", 
    Operator = FilterOperators.Equal, 
    Value = "Active" 
};
```

**Updating Sort Directions:**

```csharp
// Version 1.x - String directions (DEPRECATED)
var oldSort = new EntitySort 
{ 
    Name = "Name", 
    Direction = "asc" 
};

// Version 2.0 - Enum directions
var newSort = new EntitySort 
{ 
    Name = "Name", 
    Direction = SortDirections.Ascending 
};
```

**Upsert Operations:**

```csharp
// Version 1.x - Separate EntityUpsertCommand (REMOVED)
var oldUpsert = new EntityUpsertCommand<int, ProductUpsertModel, ProductReadModel>(principal, model);

// Version 2.0 - Use EntityUpdateCommand with upsert behavior
var newUpdate = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, id, model, true);
```
