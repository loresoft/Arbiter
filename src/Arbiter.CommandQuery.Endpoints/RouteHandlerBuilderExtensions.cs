using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Extensions for <see cref="RouteHandlerBuilder"/> to add common metadata.
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds common metadata to the route handler builder.
    /// </summary>
    /// <param name="builder">The route handler builder to update</param>
    /// <param name="entityName">The entity name to use as a metadata tag</param>
    /// <returns>The route handler builder</returns>
    public static RouteHandlerBuilder WithEntityMetadata(this RouteHandlerBuilder builder, string entityName)
    {
        return builder
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithTags(entityName);
    }
}
