using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Handlers;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery.EntityFramework;

/// <summary>
/// Extensions for adding Entity Framework Core support to the command/query pipeline.
/// </summary>
public static class DomainServiceExtensions
{
    /// <summary>
    /// Registers entity queries with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityQueries<TContext, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        // standard queries
        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, EntityIdentifierQueryHandler<TContext, TEntity, TKey, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, EntityIdentifiersQueryHandler<TContext, TEntity, TKey, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, EntityPagedQueryHandler<TContext, TEntity, TReadModel>>();
        services.TryAddTransient<IRequestHandler<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, EntitySelectQueryHandler<TContext, TEntity, TReadModel>>();

        services.AddEntityQueryBehaviors<TKey, TReadModel>();

        return services;
    }


    /// <summary>
    /// Registers a custom entity identifier query handler in the service collection.
    /// </summary>
    /// <typeparam name="TTKey">The key type for the entity identifier</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="THandler">The type of the custom query handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityIdentifierQuery<TTKey, TReadModel, THandler>(this IServiceCollection services)
        where TReadModel : class
        where THandler : class, IRequestHandler<EntityIdentifierQuery<TTKey, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TTKey, TReadModel>, TReadModel>, THandler>();

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
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCommands<TContext, TEntity, TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddEntityCreateCommand<TContext, TEntity, TKey, TReadModel, TCreateModel>()
            .AddEntityUpdateCommand<TContext, TEntity, TKey, TReadModel, TUpdateModel>()
            .AddEntityUpsertCommand<TContext, TEntity, TKey, TReadModel, TUpdateModel>()
            .AddEntityPatchCommand<TContext, TEntity, TKey, TReadModel>()
            .AddEntityDeleteCommand<TContext, TEntity, TKey, TReadModel>();

        return services;
    }


    /// <summary>
    /// Registers entity create command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCreateCommand<TContext, TEntity, TKey, TReadModel, TCreateModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TCreateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TCreateModel, TEntity>();

        services.AddEntityCreateCommand<TKey, TReadModel, TCreateModel, EntityCreateCommandHandler<TContext, TEntity, TKey, TCreateModel, TReadModel>>();

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
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpdateCommand<TContext, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TUpdateModel, TEntity>();

        // allow query for update models
        services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TUpdateModel>, TUpdateModel>, EntityIdentifierQueryHandler<TContext, TEntity, TKey, TUpdateModel>>();
        services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TUpdateModel>, IReadOnlyCollection<TUpdateModel>>, EntityIdentifiersQueryHandler<TContext, TEntity, TKey, TUpdateModel>>();

        services.AddEntityUpdateCommand<TKey, TReadModel, TUpdateModel, EntityUpdateCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>>();

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
    /// Registers entity update or insert command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpsertCommand<TContext, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();
        CacheTagger.SetTag<TUpdateModel, TEntity>();

        services.AddEntityUpsertCommand<TKey, TReadModel, TUpdateModel, EntityUpsertCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Registers a custom entity upsert command handler in the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model</typeparam>
    /// <typeparam name="THandler">The type of the custom command handler.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpsertCommand<TKey, TReadModel, TUpdateModel, THandler>(this IServiceCollection services)
        where TUpdateModel : class
        where THandler : class, IRequestHandler<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IRequestHandler<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, THandler>();

        services.AddEntityUpsertBehaviors<TKey, TReadModel, TUpdateModel>();

        return services;
    }


    /// <summary>
    /// Registers entity patch command with pipeline behaviors in the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityPatchCommand<TContext, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        services.AddEntityPatchCommand<TKey, TReadModel, EntityPatchCommandHandler<TContext, TEntity, TKey, TReadModel>>();

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
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
    /// <typeparam name="TKey">The key type for the data context entity</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityDeleteCommand<TContext, TEntity, TKey, TReadModel>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        CacheTagger.SetTag<TReadModel, TEntity>();

        services.AddEntityDeleteCommand<TKey, TReadModel, EntityDeleteCommandHandler<TContext, TEntity, TKey, TReadModel>>();
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
