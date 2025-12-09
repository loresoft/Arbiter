using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Mapping;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

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
        }

        bool supportsDeleted = typeof(TReadModel).Implements<ITrackDeleted>();
        if (supportsDeleted)
        {
            services.AddTransient<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DeletedPagedQueryBehavior<TReadModel>>();
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
