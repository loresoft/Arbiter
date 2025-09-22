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
/// Extensions for adding MongoDB support to the command/query pipeline.
/// </summary>
public static class DomainServiceExtensions
{
    /// <summary>
    /// Registers entity queries with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
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
    /// Registers a custom entity identifier query handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity identifier</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom query handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityIdentifierQuery<TKey, TReadModel, THandler>(this IServiceCollection services)
        where TReadModel : class
        where THandler : class, IRequestHandler<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, THandler>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity identifiers query handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity identifiers</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom query handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityIdentifiersQuery<TKey, TReadModel, THandler>(this IServiceCollection services)
        where TReadModel : class
        where THandler : class, IRequestHandler<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, THandler>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity paged query handler in the service collection.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom query handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityPagedQuery<TReadModel, THandler>(this IServiceCollection services)
        where TReadModel : class
        where THandler : class, IRequestHandler<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, THandler>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity select query handler in the service collection.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom query handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntitySelectQuery<TReadModel, THandler>(this IServiceCollection services)
        where TReadModel : class
        where THandler : class, IRequestHandler<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, THandler>();

        return services;
    }


    /// <summary>
    /// Registers entity create, update and delete commands with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCommands<TRepository, TEntity, TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>()
            .AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>()
            .AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>()
            .AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>();

        return services;
    }


    /// <summary>
    /// Registers entity create command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TCreateModel, TEntity>();

        services.AddEntityCreateCommand<TKey, TReadModel, TCreateModel, EntityCreateCommandHandler<TRepository, TEntity, TKey, TCreateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity create command handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model</typeparam>
    /// <typeparam name="THandler">The type of the custom command handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCreateCommand<TKey, TReadModel, TCreateModel, THandler>(this IServiceCollection services)
        where TCreateModel : class
        where THandler : class, IRequestHandler<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, THandler>();

        services.AddEntityCreateBehaviors<TKey, TReadModel, TCreateModel>();

        return services;
    }


    /// <summary>
    /// Registers entity update command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TUpdateModel, TEntity>();

        // allow query for update models
        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TUpdateModel>, TUpdateModel>, EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();
        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TUpdateModel>, IReadOnlyCollection<TUpdateModel>>, EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();

        services.AddEntityUpdateCommand<TKey, TReadModel, TUpdateModel, EntityUpdateCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity update command handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model</typeparam>
    /// <typeparam name="THandler">The type of the custom command handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpdateCommand<TKey, TReadModel, TUpdateModel, THandler>(this IServiceCollection services)
        where TUpdateModel : class
        where THandler : class, IRequestHandler<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, THandler>();

        services.AddEntityUpdateBehaviors<TKey, TReadModel, TUpdateModel>();

        return services;
    }


    /// <summary>
    /// Registers entity patch command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        services.AddEntityPatchCommand<TKey, TReadModel, EntityPatchCommandHandler<TRepository, TEntity, TKey, TReadModel>>();

        services.AddEntityPatchBehaviors<TKey, TEntity, TReadModel>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity patch command handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom command handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityPatchCommand<TKey, TReadModel, THandler>(this IServiceCollection services)
        where THandler : class, IRequestHandler<EntityPatchCommand<TKey, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityPatchCommand<TKey, TReadModel>, TReadModel>, THandler>();

        return services;
    }


    /// <summary>
    /// Registers entity delete command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TRepository">The type of the MongoDB repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TRepository : IMongoRepository<TEntity, TKey>
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        services.AddEntityDeleteCommand<TKey, TReadModel, EntityDeleteCommandHandler<TRepository, TEntity, TKey, TReadModel>>();
        services.AddEntityDeleteBehaviors<TKey, TEntity, TReadModel>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity delete command handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom command handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityDeleteCommand<TKey, TReadModel, THandler>(this IServiceCollection services)
        where THandler : class, IRequestHandler<EntityDeleteCommand<TKey, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, THandler>();

        return services;
    }
}
