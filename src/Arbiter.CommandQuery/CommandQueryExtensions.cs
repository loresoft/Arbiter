using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Options;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;
using Arbiter.Mapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
    /// principal reader, mapper, tenant resolver, and <see cref="EnvironmentOptions"/> bound from configuration.
    /// </remarks>
    public static IServiceCollection AddCommandQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediator();
        services.AddEnvironmentOptions();

        services.TryAddSingleton<IPrincipalReader, PrincipalReader>();
        services.TryAddSingleton<IMapper, ServiceProviderMapper>();
        services.TryAddSingleton(typeof(ITenantResolver<>), typeof(TenantResolver<>));

        return services;
    }

    /// <summary>
    /// Adds <see cref="EnvironmentOptions"/> bound from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">
    /// An optional delegate used to configure the <see cref="EnvironmentOptions"/>. The delegate runs as a post-configure
    /// action, so any values it sets override configured values regardless of registration order.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// The options are bound from the root of <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>, so each
    /// property of <see cref="EnvironmentOptions"/> is set as a top level key in <c>appsettings.json</c>
    /// (for Blazor WebAssembly, <c>wwwroot/appsettings.json</c>). Values from other configuration sources, such as
    /// environment variables, are bound the same way.
    /// </para>
    /// <para>
    /// This method can be called multiple times; the configuration binding is only registered once.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>Configure the options in <c>appsettings.json</c>:</para>
    /// <code language="json">
    /// {
    ///   "ApplicationName": "Tracker",
    ///   "CompanyName": "LoreSoft",
    ///   "ProjectName": "Arbiter",
    ///   "EnvironmentName": "Production",
    ///   "BaseAddress": "https://tracker.example.com/",
    ///   "DefaultCacheTime": "00:10:00"
    /// }
    /// </code>
    /// <para>Register the options, optionally overriding configured values in code:</para>
    /// <code language="csharp">
    /// builder.Services.AddEnvironmentOptions(options => options.EnvironmentName = "Staging");
    /// </code>
    /// </example>
    public static IServiceCollection AddEnvironmentOptions(this IServiceCollection services, Action<EnvironmentOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();

        // register the binding once and run the delegate as post-configure, so it overrides configuration in any call order
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<EnvironmentOptions>, EnvironmentOptionsSetup>());

        if (configure != null)
            services.PostConfigure(configure);

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

        services.AddEntityHybridCacheQueryBehavior();
        services.AddEntityHybridCacheExpireBehavior();

        return services;
    }

    /// <summary>
    /// Adds the hybrid cache query behavior to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityHybridCacheQueryBehavior(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(HybridCacheQueryBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds the hybrid cache expiration behavior to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEntityHybridCacheExpireBehavior(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(HybridCacheExpireBehavior<,>));

        return services;
    }


    /// <summary>
    /// Adds MessagePack options to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional action to configure the MessagePack serializer options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <seealso cref="MessagePackDefaults"/>
    public static IServiceCollection AddMessagePackOptions(this IServiceCollection services, Action<MessagePack.MessagePackSerializerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure MessagePack options
        var options = MessagePackDefaults.DefaultSerializerOptions;
        configure?.Invoke(options);

        // MessagePack Serializer Options Registration
        services.TryAddSingleton(options);

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
