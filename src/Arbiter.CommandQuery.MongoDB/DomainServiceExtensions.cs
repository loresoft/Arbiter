using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB;

/// <summary>
/// Provides extension methods for registering domain services, including queries and commands, for MongoDB repositories.
/// </summary>
public static class DomainServiceExtensions
{
    /// <summary>
    /// Registers query handlers for entity-related queries in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The service collection to add the query handlers to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityQueries<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        // standard queries
        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, EntityPagedQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, EntitySelectQueryHandler<TRepository, TEntity, TKey, TReadModel>>();

        services.AddEntityQueryBehaviors<TKey, TReadModel>();

        return services;
    }

    /// <summary>
    /// Registers command handlers for entity-related commands in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The service collection to add the command handlers to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityCommands<TRepository, TEntity, TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TCreateModel, TEntity>();
        CacheTagger.SetTag<TUpdateModel, TEntity>();

        services
            .AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>()
            .AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>()
            .AddEntityUpsertCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>()
            .AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>()
            .AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>();

        return services;
    }

    /// <summary>
    /// Registers a create command handler for entities in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <param name="services">The service collection to add the create command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // standard crud commands
        services.TryAddTransient<IRequestHandler<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, EntityCreateCommandHandler<TRepository, TEntity, TKey, TCreateModel, TReadModel>>();

        services.AddEntityCreateBehaviors<TKey, TReadModel, TCreateModel>();

        return services;
    }

    /// <summary>
    /// Registers an update command handler for entities in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The service collection to add the update command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // allow query for update models
        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TUpdateModel>, TUpdateModel>, EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();
        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TUpdateModel>, IReadOnlyCollection<TUpdateModel>>, EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();

        // standard crud commands
        services.TryAddTransient<IRequestHandler<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityUpdateCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>>();

        services.AddEntityUpdateBehaviors<TKey, TReadModel, TUpdateModel>();

        return services;
    }

    /// <summary>
    /// Registers an upsert command handler for entities in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The service collection to add the upsert command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityUpsertCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // standard crud commands
        services.TryAddTransient<IRequestHandler<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityUpsertCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>>();

        services.AddEntityUpsertBehaviors<TKey, TReadModel, TUpdateModel>();

        return services;
    }

    /// <summary>
    /// Registers a patch command handler for entities in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The service collection to add the patch command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // standard crud commands
        services.TryAddTransient<IRequestHandler<EntityPatchCommand<TKey, TReadModel>, TReadModel>, EntityPatchCommandHandler<TRepository, TEntity, TKey, TReadModel>>();

        services.AddEntityPatchBehaviors<TKey, TEntity, TReadModel>();

        return services;
    }

    /// <summary>
    /// Registers a delete command handler for entities in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The service collection to add the delete command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // standard crud commands
        services.TryAddTransient<IRequestHandler<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, EntityDeleteCommandHandler<TRepository, TEntity, TKey, TReadModel>>();

        services.AddEntityDeleteBehaviors<TKey, TEntity, TReadModel>();

        return services;
    }
}
