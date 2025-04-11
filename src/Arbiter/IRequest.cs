namespace Arbiter;

/// <summary>
/// An <see langword="interface"/> to represent a request
/// </summary>
public interface IRequest;

/// <summary>
/// An <see langword="interface"/> to represent a request with a response
/// </summary>
/// <typeparam name="TResponse">The type of response from request</typeparam>
public interface IRequest<out TResponse> : IRequest;
