using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// A command to create a new entity based on the specified <typeparamref name="TCreateModel"/>.
/// <typeparamref name="TReadModel"/> will be the result of the command.
/// </summary>
/// <typeparam name="TCreateModel">The type of the create model.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public record EntityCreateCommand<TCreateModel, TReadModel>
    : EntityModelCommand<TCreateModel, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this command is run for</param>
    /// <param name="model">The create model for this command.</param>
    public EntityCreateCommand(ClaimsPrincipal? principal, [NotNull] TCreateModel model)
        : base(principal, model)
    {
    }

    /// <inheritdoc />
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
