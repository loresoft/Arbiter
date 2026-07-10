---
name: arbiter-mediation
description: Use when wiring or extending Arbiter.Mediation — defining IRequest / IRequestHandler, INotification / INotificationHandler, IPipelineBehavior, or calling services.AddMediator(). Also use when the user asks about Arbiter's mediator pattern, request/response, notifications/events, or pipeline middleware.
---

# Arbiter.Mediation

Lightweight mediator: requests with typed responses, notifications, and pipeline behaviors. No reflection at the hot path, AOT-friendly.

## Install

```bash
dotnet add package Arbiter.Mediation
```

## Register

```csharp
using Arbiter.Mediation;

// Singleton mediator by default; pass ServiceLifetime to change
services.AddMediator();

// Handlers and behaviors are registered separately
services.AddTransient<IRequestHandler<Ping, Pong>, PingHandler>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

`AddMediator(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)`.

## Canonical pattern — request/response

```csharp
public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}

public class Pong
{
    public string? Message { get; set; }
}

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        return new Pong { Message = $"{request.Message} Pong" };
    }
}

// Send
var pong = await mediator.Send(new Ping { Message = "Hello" }, cancellationToken);
```

## Notifications (events)

```csharp
public class UserCreated : INotification
{
    public int UserId { get; init; }
}

public class SendWelcomeEmail : INotificationHandler<UserCreated>
{
    public ValueTask Handle(UserCreated notification, CancellationToken cancellationToken)
    {
        // ...
        return ValueTask.CompletedTask;
    }
}

services.AddTransient<INotificationHandler<UserCreated>, SendWelcomeEmail>();

await mediator.Publish(new UserCreated { UserId = 42 }, cancellationToken);
```

## Pipeline behavior (middleware)

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async ValueTask<TResponse?> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next(cancellationToken);
        _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## Notes

- Handler return type is `ValueTask<TResponse?>` (nullable). `Send` returns `TResponse?`.
- Behaviors run in registration order, wrapping the next handler.
- For CQRS abstractions on top of this (entity CRUD, paging, filtering), see `arbiter-commandquery`.
- For OpenTelemetry, see `arbiter-opentelemetry` — sources are `MediatorTelemetry.SourceName` / `MediatorTelemetry.MeterName`.

## Reference

- Guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/mediation.md
- Source: `src/Arbiter.Mediation/`
