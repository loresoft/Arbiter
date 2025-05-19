using Microsoft.AspNetCore.Routing;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Defines an <see langword="interface"/> for adding routes to an ASP.NET Core application.
/// </summary>
public interface IEndpointRoute
{
    /// <summary>
    /// Adds routes to the specified <see cref="IEndpointRouteBuilder"/> instance.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    void AddRoutes(IEndpointRouteBuilder endpoints);
}
