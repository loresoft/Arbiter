// Ignore Spelling: Upsert

using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery;

/// <summary>
/// Extensions for adding mediator services to the service collection.
/// </summary>
public static class CommandQueryExtensions
{
    /// <summary>
    /// Adds the mediator command query services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCommandQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediator();

        services.TryAddSingleton<IPrincipalReader, PrincipalReader>();
        services.TryAddSingleton<IMapper, ServiceProviderMapper>();

        return services;
    }


    /// <summary>
    /// Adds the remote dispatcher to the service collection.  The client must register the <see cref="RemoteDispatcher"/> with the correct <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // up to client to register RemoteDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<RemoteDispatcher>());
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        return services;
    }

    /// <summary>
    /// Adds the server dispatcher to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddServerDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IDispatcher, MediatorDispatcher>();
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        return services;
    }


    /// <summary>
    /// Adds the caching query behaviors for <see cref="IMemoryCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityQueryMemoryCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, MemoryCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, MemoryCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>, MemoryCacheQueryBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="IDistributedCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityQueryDistributedCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, DistributedCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DistributedCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>, DistributedCacheQueryBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="HybridCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityHybridCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, HybridCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, HybridCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, HybridCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, HybridCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>, HybridCacheQueryBehavior<EntityContinuationQuery<TReadModel>, EntityContinuationResult<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching command behaviors for <see cref="HybridCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityHybridCache<TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityHybridCache<TKey, TReadModel>();

        services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the entity query behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityQueryBehaviors<TKey, TReadModel>(this IServiceCollection services)
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        bool supportsTenant = typeof(TReadModel).Implements<IHaveTenant<TKey>>();
        if (supportsTenant)
        {
            services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, TenantPagedQueryBehavior<TKey, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, TenantSelectQueryBehavior<TKey, TReadModel>>();
        }

        bool supportsDeleted = typeof(TReadModel).Implements<ITrackDeleted>();
        if (supportsDeleted)
        {
            services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DeletedPagedQueryBehavior<TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, DeletedSelectQueryBehavior<TReadModel>>();
        }

        return services;
    }

    /// <summary>
    /// Adds the entity create behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityCreateBehaviors<TKey, TReadModel, TCreateModel>(this IServiceCollection services)
        where TCreateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        var createType = typeof(TCreateModel);
        bool supportsTenant = createType.Implements<IHaveTenant<TKey>>();
        if (supportsTenant)
        {
            services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TCreateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TCreateModel, TReadModel>>();
        }

        bool supportsTracking = createType.Implements<ITrackCreated>();
        if (supportsTracking)
            services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TCreateModel, TReadModel>>();

        services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TCreateModel, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TCreateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the entity update behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpdateBehaviors<TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        var updateType = typeof(TUpdateModel);
        bool supportsTenant = updateType.Implements<IHaveTenant<TKey>>();
        if (supportsTenant)
        {
            services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TUpdateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TUpdateModel, TReadModel>>();
        }

        bool supportsTracking = updateType.Implements<ITrackUpdated>();
        if (supportsTracking)
            services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TUpdateModel, TReadModel>>();

        services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TUpdateModel, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TUpdateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the entity upsert behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityUpsertBehaviors<TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
        where TUpdateModel : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        var updateType = typeof(TUpdateModel);
        bool supportsTenant = updateType.Implements<IHaveTenant<TKey>>();
        if (supportsTenant)
        {
            services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TUpdateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TUpdateModel, TReadModel>>();
        }

        bool supportsTracking = updateType.Implements<ITrackUpdated>();
        if (supportsTracking)
            services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TUpdateModel, TReadModel>>();

        services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TUpdateModel, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TUpdateModel, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the entity patch behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityPatchBehaviors<TKey, TEntity, TReadModel>(this IServiceCollection services)
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        services.AddTransient<IPipelineBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TEntity, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the entity delete behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the model.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityDeleteBehaviors<TKey, TEntity, TReadModel>(this IServiceCollection services)
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        services.AddTransient<IPipelineBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TEntity, TReadModel>>();

        return services;
    }
}
