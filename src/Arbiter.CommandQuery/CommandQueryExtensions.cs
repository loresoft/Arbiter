using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Mapping;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;
using Arbiter.CommandQuery.State;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery;

/// <summary>
/// Extension methods for adding command query services to the service collection.
/// </summary>
public static class CommandQueryExtensions
{
    /// <summary>
    /// Adds the command query services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the core command query services including the mediator,
    /// principal reader, mapper, and tenant resolver.
    /// </remarks>
    public static IServiceCollection AddCommandQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediator();

        services.TryAddSingleton<IPrincipalReader, PrincipalReader>();
        services.TryAddSingleton<IMapper, ServiceProviderMapper>();
        services.TryAddSingleton(typeof(ITenantResolver<>), typeof(TenantResolver<>));

        return services;
    }

    /// <summary>
    /// Adds command validation behavior to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers a pipeline behavior that validates commands before they are processed.
    /// It ensures that any command passed through the pipeline adheres to the defined validation rules.
    /// </remarks>
    public static IServiceCollection AddCommandValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidateCommandBehavior<,>));

        return services;
    }


    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client with service provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using the HTTP client instance only.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the remote dispatcher without configuring the HTTP client.
    /// The client must register the <see cref="RemoteDispatcher"/> with the correct <see cref="HttpClient"/> separately.
    /// </remarks>
    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // up to client to register RemoteDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<RemoteDispatcher>());
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }

    /// <summary>
    /// Adds the server dispatcher to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// The server dispatcher uses the mediator pattern to dispatch commands and queries locally.
    /// </remarks>
    public static IServiceCollection AddServerDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IDispatcher, MediatorDispatcher>();
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }


    /// <summary>
    /// Adds the caching query behaviors for <see cref="IMemoryCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers memory cache behaviors for all standard entity query operations:
    /// identifier, identifiers, paged, select, and continuation queries.
    /// </remarks>
    [Obsolete("Use AddEntityMemoryCache() instead. This method will be removed in a future release.")]
    public static IServiceCollection AddEntityQueryMemoryCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, MemoryCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, MemoryCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="IMemoryCache"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers generic memory cache behaviors for all requests that implement <see cref="ICacheResult"/>
    /// </remarks>
    public static IServiceCollection AddEntityMemoryCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MemoryCacheQueryBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="IDistributedCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers distributed cache behaviors for all standard entity query operations:
    /// identifier, identifiers, paged, select, and continuation queries.
    /// </remarks>
    [Obsolete("Use AddEntityDistributedCache() instead. This method will be removed in a future release.")]
    public static IServiceCollection AddEntityQueryDistributedCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, DistributedCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DistributedCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="IDistributedCache"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers generic distributed cache behaviors for all requests that implement <see cref="ICacheResult"/>
    /// </remarks>
    public static IServiceCollection AddEntityDistributedCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DistributedCacheQueryBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds the caching query behaviors for <see cref="HybridCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers hybrid cache behaviors for all standard entity query operations:
    /// identifier, identifiers, paged, select, and continuation queries.
    /// </remarks>
    [Obsolete("Use AddEntityHybridCache() instead. This method will be removed in a future release.")]
    public static IServiceCollection AddEntityHybridCache<TKey, TReadModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, HybridCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, HybridCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, HybridCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
        services.AddTransient<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, HybridCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();

        return services;
    }

    /// <summary>
    /// Adds the caching command and query behaviors for <see cref="HybridCache"/> to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers hybrid cache behaviors for both queries and commands.
    /// Query behaviors provide caching, while command behaviors handle cache expiration.
    /// </remarks>
    [Obsolete("Use AddEntityHybridCache() instead. This method will be removed in a future release.")]
    public static IServiceCollection AddEntityHybridCache<TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityHybridCache<TKey, TReadModel>();

        services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>>();
        services.AddTransient<IPipelineBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, HybridCacheExpireBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>>();

        return services;
    }

    /// <summary>
    /// Adds the hybrid cache behaviors for entity commands and queries to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers generic hybrid cache behaviors for all requests that implement the appropriate interfaces:
    /// <list type="bullet">
    /// <item><description>Query behaviors are registered for requests implementing <see cref="ICacheResult"/> - these provide automatic caching of query results.</description></item>
    /// <item><description>Expire behaviors are registered for requests implementing <see cref="ICacheExpire"/> - these handle automatic cache invalidation for commands.</description></item>
    /// </list>
    /// This is a more flexible alternative to the strongly-typed entity-specific cache registration methods,
    /// allowing any request type to participate in caching by implementing the appropriate marker interfaces.
    /// </remarks>
    public static IServiceCollection AddEntityHybridCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(HybridCacheQueryBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(HybridCacheExpireBehavior<,>));

        return services;
    }


    /// <summary>
    /// Adds the entity query behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method conditionally registers pipeline behaviors based on the interfaces implemented by <typeparamref name="TReadModel"/>:
    /// <list type="bullet">
    /// <item><description>If <typeparamref name="TReadModel"/> implements <see cref="IHaveTenant{TKey}"/>, tenant-based filtering behaviors are added.</description></item>
    /// <item><description>If <typeparamref name="TReadModel"/> implements <see cref="ITrackDeleted"/>, soft delete filtering behaviors are added.</description></item>
    /// </list>
    /// Pipeline behaviors are registered in the order they should execute.
    /// </remarks>
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
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method conditionally registers pipeline behaviors based on the interfaces implemented by <typeparamref name="TCreateModel"/>:
    /// <list type="bullet">
    /// <item><description>If <typeparamref name="TCreateModel"/> implements <see cref="IHaveTenant{TKey}"/>, tenant-based behaviors are added.</description></item>
    /// <item><description>If <typeparamref name="TCreateModel"/> implements <see cref="ITrackCreated"/>, creation tracking behaviors are added.</description></item>
    /// </list>
    /// Validation and change notification behaviors are always added.
    /// Pipeline behaviors are registered in the order they should execute.
    /// </remarks>
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
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method conditionally registers pipeline behaviors based on the interfaces implemented by <typeparamref name="TUpdateModel"/>:
    /// <list type="bullet">
    /// <item><description>If <typeparamref name="TUpdateModel"/> implements <see cref="IHaveTenant{TKey}"/>, tenant-based behaviors are added.</description></item>
    /// <item><description>If <typeparamref name="TUpdateModel"/> implements <see cref="ITrackUpdated"/>, update tracking behaviors are added.</description></item>
    /// </list>
    /// Validation and change notification behaviors are always added.
    /// Pipeline behaviors are registered in the order they should execute.
    /// </remarks>
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
    /// Adds the entity patch behaviors to the service collection.
    /// </summary>
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers change notification behaviors for patch operations.
    /// Pipeline behaviors are registered in the order they should execute.
    /// </remarks>
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
    /// <typeparam name="TKey">The key type for the entity model.</typeparam>
    /// <typeparam name="TEntity">The type of entity being operated on.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers change notification behaviors for delete operations.
    /// Pipeline behaviors are registered in the order they should execute.
    /// </remarks>
    public static IServiceCollection AddEntityDeleteBehaviors<TKey, TEntity, TReadModel>(this IServiceCollection services)
        where TEntity : class, IHaveIdentifier<TKey>, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // pipeline registration, run in order registered
        services.AddTransient<IPipelineBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TEntity, TReadModel>>();

        return services;
    }
}
