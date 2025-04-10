# Arbiter

Mediator pattern and CQRS implementation in .NET


## Mediator

A lightweight and extensible implementation of the Mediator pattern for .NET applications, designed for clean, modular architectures like Vertical Slice Architecture and CQRS.

### Installation

The Arbiter library is available on nuget.org via package name `Arbiter`.

To install Arbiter, run the following command in the Package Manager Console

```powershell
Install-Package Arbiter
```

OR

```shell
dotnet add package Arbiter
```

### Features

- Request with response handling
- Notifications (Events)
- Pipeline Behaviors, like middleware 
- Dependence Injection based, no reflection


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
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new Pong { Message = request.Message + " Pong" });
    }
}
```

### Define Pipeline Behavior

```csharp
public class PingBehavior : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> Get(string? message, CancellationToken cancellationToken = default)
    {
        var request = new Ping { Message = message };
        var response = await _mediator.Send<Ping, Pong>(request, cancellationToken);

        return Ok(response);
    }
}
```
