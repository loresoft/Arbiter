using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.Components;

/// <summary>
/// Derives the name and display label of a model type, for example <c>PurchaseOrder</c> and
/// <c>Purchase Order</c> for a <c>PurchaseOrderReadModel</c>.
/// </summary>
/// <remarks>
/// The <c>Read</c>, <c>List</c>, <c>Create</c>, <c>Update</c> or <c>Export</c> model suffix is removed from the
/// type name. A type name without one of those suffixes is used as is, and a <see cref="Nullable{T}"/> type uses
/// the name of its underlying type. Results are cached per type and are safe to read from any thread.
/// </remarks>
public static partial class ModelTypeName
{
    private static readonly ConcurrentDictionary<Type, string> _names = new();
    private static readonly ConcurrentDictionary<Type, string> _labels = new();

    [GeneratedRegex(@"^(?<name>\w+)(Read|List|Create|Update|Export)Model$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ModelTypeRegex();

    /// <summary>
    /// Gets the name of the model type without spaces, for example <c>PurchaseOrder</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    /// <returns>The type name with the model suffix removed</returns>
    public static string GetName<TModel>()
        => GetName(typeof(TModel));

    /// <summary>
    /// Gets the name of the model type without spaces, for example <c>PurchaseOrder</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <param name="modelType">The model type; a <see cref="Nullable{T}"/> type uses its underlying type</param>
    /// <returns>
    /// The type name with the model suffix removed, or <see langword="null"/> when <paramref name="modelType"/> is
    /// <see langword="null"/>
    /// </returns>
    [return: NotNullIfNotNull(nameof(modelType))]
    public static string? GetName(Type? modelType)
    {
        if (modelType is null)
            return null;

        return _names.GetOrAdd(modelType, static type => CreateName(type));
    }

    /// <summary>
    /// Gets the human readable name of the model type, for example <c>Purchase Order</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    /// <returns>The type name with the model suffix removed and split into words</returns>
    public static string GetLabel<TModel>()
        => GetLabel(typeof(TModel));

    /// <summary>
    /// Gets the human readable name of the model type, for example <c>Purchase Order</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <param name="modelType">The model type; a <see cref="Nullable{T}"/> type uses its underlying type</param>
    /// <returns>
    /// The type name with the model suffix removed and split into words, or <see langword="null"/> when
    /// <paramref name="modelType"/> is <see langword="null"/>
    /// </returns>
    [return: NotNullIfNotNull(nameof(modelType))]
    public static string? GetLabel(Type? modelType)
    {
        if (modelType is null)
            return null;

        return _labels.GetOrAdd(modelType, static type => GetName(type).ToTitle());
    }

    private static string CreateName(Type modelType)
    {
        var type = Nullable.GetUnderlyingType(modelType) ?? modelType;
        return ModelTypeRegex().Replace(type.Name, "${name}");
    }
}
