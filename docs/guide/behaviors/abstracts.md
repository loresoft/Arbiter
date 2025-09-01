# Behavior Abstracts

Abstract base classes and interfaces for implementing pipeline behaviors in the Arbiter framework.

## IPipelineBehavior Interface

The foundational interface that defines the contract for all pipeline behaviors.

```csharp
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    ValueTask<TResponse?> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
```

### Interface Features

- **Generic Design**: Works with any request/response type combination
- **Asynchronous**: Uses `ValueTask<T>` for optimal performance
- **Cancellation Support**: Built-in cancellation token support
- **Pipeline Chaining**: The `next` delegate allows chaining multiple behaviors

## PipelineBehaviorBase

The primary abstract base class for implementing custom pipeline behaviors. It provides essential infrastructure including logging, timing, and standardized error handling.

```csharp
public abstract partial class PipelineBehaviorBase<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
```

### Base Class Features

- **Built-in Logging**: Automatic trace logging with execution timing
- **Error Handling**: Consistent exception handling and logging
- **Performance Monitoring**: Automatic timing of behavior execution
- **Logger Access**: Protected `Logger` property for custom logging needs

### Implementation Pattern

When inheriting from `PipelineBehaviorBase`, you only need to implement the `Process` method:

```csharp
protected abstract ValueTask<TResponse?> Process(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken);
```

### Example Implementation

```csharp
public class CustomBehavior<TRequest, TResponse>
    : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    public CustomBehavior(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }

    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Pre-processing logic
        Logger.LogInformation("Processing {RequestType}", typeof(TRequest).Name);
        
        // Call next behavior or handler
        var response = await next().ConfigureAwait(false);
        
        // Post-processing logic
        Logger.LogInformation("Completed {RequestType}", typeof(TRequest).Name);
        
        return response;
    }
}
```

### Best Practices

1. **Constructor Pattern**: Always accept `ILoggerFactory` and call the base constructor
2. **Null Checks**: The base class handles null checking for request and next delegate
3. **Exception Handling**: Let the base class handle timing and logging; focus on business logic
4. **Async/Await**: Use `ConfigureAwait(false)` when calling the next delegate
5. **Resource Management**: Dispose of any resources in your behavior implementation
