using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Arbiter.Mediation.Infrastructure;

/// <summary>
/// Provides a registry for mediator handlers with thread-safe registration and caching.
/// </summary>
public static class MediatorRegistry
{
    private static readonly ConcurrentDictionary<Type, IMediatorHandler> _registrations = new();
    private static volatile FrozenDictionary<Type, IMediatorHandler>? _frozenCache;

    /// <summary>
    /// Gets the total number of items currently stored in the cache or registered in the mediator registry.
    /// </summary>
    /// <remarks>If the frozen cache is available, this property returns the number of items in the cache;
    /// otherwise, it returns the number of registered items. Use this property to determine the current size of the
    /// registry without directly accessing the underlying collections.</remarks>
    public static int Count => _frozenCache?.Count ?? _registrations.Count;

    /// <summary>
    /// Registers handlers. Invalidates frozen cache if it exists.
    /// </summary>
    public static void Register(IEnumerable<KeyValuePair<Type, IMediatorHandler>> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        // Invalidate frozen cache - forces rebuild on next access
        Interlocked.Exchange(ref _frozenCache, value: null);

        // Add handlers (thread-safe)
        foreach (var (type, handler) in handlers)
            _registrations.TryAdd(type, handler);
    }


    /// <summary>
    /// Gets the registered handler for the specified request type or adds one if it does not exist.
    /// </summary>
    /// <param name="requestType">The request type key.</param>
    /// <param name="handlerFactory">Factory used to create a handler when no registration exists.</param>
    /// <returns>The existing or newly created handler.</returns>
    public static IMediatorHandler GetOrAdd(Type requestType, Func<Type, IMediatorHandler> handlerFactory)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        ArgumentNullException.ThrowIfNull(handlerFactory);

        return _registrations.GetOrAdd(requestType, type =>
        {
            Interlocked.Exchange(ref _frozenCache, value: null);
            return handlerFactory(type);
        });
    }

    /// <summary>
    /// Attempts to retrieve the handler associated with the specified request type.
    /// </summary>
    /// <remarks>This method caches handler lookups to improve performance on subsequent calls. If the cache
    /// is not initialized, the method falls back to searching the registrations dictionary.</remarks>
    /// <param name="requestType">The type of the request for which to retrieve the handler. This parameter cannot be null.</param>
    /// <param name="handler">When this method returns, contains the handler associated with the specified request type, if found; otherwise,
    /// null. This parameter is passed uninitialized.</param>
    /// <returns>true if a handler for the specified request type is found; otherwise, false.</returns>
    public static bool TryGetHandler(Type requestType, out IMediatorHandler? handler)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        var cache = GetFrozenCache();
        if (cache is not null)
            return cache.TryGetValue(requestType, out handler);

        return _registrations.TryGetValue(requestType, out handler);
    }


    /// <summary>
    /// Gets frozen cache, rebuilding if needed.
    /// </summary>
    public static FrozenDictionary<Type, IMediatorHandler> GetFrozenCache()
    {
        var cache = _frozenCache;
        if (cache is not null)
            return cache;

        // Rebuild
        var newCache = _registrations.ToFrozenDictionary();

        // Atomic update
        var original = Interlocked.CompareExchange(ref _frozenCache, newCache, null);

        return original ?? newCache;
    }

    /// <summary>
    /// Gets a read-only dictionary containing the current registrations of mediator handlers.
    /// </summary>
    /// <remarks>The returned dictionary provides a snapshot of the registered mediator handlers. If the
    /// internal cache is not yet frozen, the method returns the live registrations, which may change if new handlers
    /// are registered. Use this method to inspect or enumerate the available mediator handlers without modifying the
    /// registration state.</remarks>
    /// <returns>An IReadOnlyDictionary that maps each handler type to its corresponding IMediatorHandler instance. If the
    /// registration cache is not initialized, returns the current registrations from the underlying collection.</returns>
    public static IReadOnlyDictionary<Type, IMediatorHandler> GetRegistrations()
    {
        return _frozenCache ?? (IReadOnlyDictionary<Type, IMediatorHandler>)_registrations;
    }



    /// <summary>
    /// Removes a handler (for testing/hot reload).
    /// </summary>
    public static bool Unregister(Type requestType)
    {
        Interlocked.Exchange(ref _frozenCache, value: null);
        return _registrations.TryRemove(requestType, out _);
    }

    /// <summary>
    /// Clears all handlers (for testing).
    /// </summary>
    public static void Clear()
    {
        Interlocked.Exchange(ref _frozenCache, null);
        _registrations.Clear();
    }

}
