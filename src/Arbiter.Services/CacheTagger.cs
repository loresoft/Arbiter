using System.Collections.Concurrent;

namespace Arbiter.Services;

/// <summary>
/// A service to tag models with cache tags.
/// </summary>
public static class CacheTagger
{
    private static readonly ConcurrentDictionary<Type, string?> _typeTags = new();

    /// <summary>
    /// Sets the tag for a model type.
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    /// <param name="tag">The tag to use for the model</param>
    public static void SetTag<TModel>(string? tag)
    {
        _typeTags.TryAdd(typeof(TModel), tag);
    }

    /// <summary>
    /// Sets the tag for a model type using the full name of the entity type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <typeparam name="TEntity">The type of the entity use as tag</typeparam>
    public static void SetTag<TModel, TEntity>()
    {
        _typeTags.TryAdd(typeof(TModel), typeof(TEntity).FullName);
    }

    /// <summary>
    /// Gets the tag for a model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <returns>The tag for the model type</returns>
    public static string? GetTag<TModel>()
    {
        var type = typeof(TModel);
        if (_typeTags.TryGetValue(type, out var tag))
            return tag;

        return type.FullName;
    }

    /// <summary>
    /// Gets all tags currently registered.
    /// </summary>
    /// <returns>A distinct collection of all registered tags.</returns>
    public static IReadOnlySet<string> GetTags()
    {
        // _typeTags.Values may contain duplicates or null/empty values
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in _typeTags.Values)
        {
            if (!string.IsNullOrEmpty(tag))
                set.Add(tag);
        }

        return set;
    }


    /// <summary>
    /// Generates a cache key for a model type and a value.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="bucket">The cache bucket for the key</param>
    /// <param name="value">The value to use as part of the key</param>
    /// <param name="delimiter">The cache key part delimiter</param>
    /// <returns>The cache key for the model type</returns>
    public static string GetKey<TModel, TValue>(string bucket, TValue value, string delimiter = ".")
    {
        _typeTags.TryGetValue(typeof(TModel), out var tag);
        tag ??= typeof(TModel).FullName;

        return $"{tag}{delimiter}{bucket}{delimiter}{value}";
    }

    /// <summary>
    /// Default cache key bucket names
    /// </summary>
    public static class Buckets
    {
        /// <summary>The cache key bucket for an entity identifier</summary>
        public const string Identifier = "id";
        /// <summary>The cache key bucket for a list of identifiers</summary>
        public const string Identifiers = "ids";
        /// <summary>The cache key bucket for a paged list of items</summary>
        public const string Paged = "page";
        /// <summary>The cache key bucket for a continuation list of items</summary>
        public const string Continuation = "continue";
        /// <summary>The cache key bucket for a list of items</summary>
        public const string List = "list";
        /// <summary>The cache key bucket for an entity alternate key</summary>
        public const string Key = "key";
    }
}
