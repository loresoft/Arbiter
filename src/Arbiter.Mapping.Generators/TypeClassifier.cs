using Arbiter.Mapping.Generators.Models;

using Microsoft.CodeAnalysis;

namespace Arbiter.Mapping.Generators;

/// <summary>
/// Classifies type symbols for nested deep clone mapping.
/// </summary>
internal static class TypeClassifier
{
    /// <summary>
    /// Determines whether the type is a class that can be deep cloned as a nested object.
    /// </summary>
    /// <param name="type">The type symbol to inspect.</param>
    /// <returns><see langword="true"/> if the type is a non-abstract, non-system class; otherwise <see langword="false"/>.</returns>
    public static bool IsComplex(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named)
            return false;

        if (named.TypeKind != TypeKind.Class || named.IsAbstract || named.IsStatic)
            return false;

        if (named.SpecialType != SpecialType.None)
            return false;

        var ns = named.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (string.Equals(ns, "System", StringComparison.Ordinal) || ns.StartsWith("System.", StringComparison.Ordinal))
            return false;

        return true;
    }

    /// <summary>
    /// Gets the element type of a source collection (any type implementing <c>IEnumerable&lt;T&gt;</c>, excluding <see cref="string"/>).
    /// </summary>
    /// <param name="type">The source type symbol.</param>
    /// <returns>The element type, or <see langword="null"/> if the type is not a collection.</returns>
    public static ITypeSymbol? GetSourceElementType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
            return null;

        if (type is IArrayTypeSymbol array)
            return array.Rank == 1 ? array.ElementType : null;

        if (type is INamedTypeSymbol named && IsGeneric(named, "System.Collections.Generic.IEnumerable<T>"))
            return named.TypeArguments[0];

        foreach (var iface in type.AllInterfaces)
        {
            if (IsGeneric(iface, "System.Collections.Generic.IEnumerable<T>"))
                return iface.TypeArguments[0];
        }

        return null;
    }

    /// <summary>
    /// Gets the element type and collection kind for a supported destination collection type.
    /// </summary>
    /// <param name="type">The destination type symbol.</param>
    /// <param name="kind">The collection kind to create.</param>
    /// <returns>The element type, or <see langword="null"/> if the destination collection type is not supported.</returns>
    public static ITypeSymbol? GetDestinationElementType(ITypeSymbol type, out CollectionKind kind)
    {
        kind = CollectionKind.None;

        if (type is IArrayTypeSymbol array)
        {
            if (array.Rank != 1)
                return null;

            kind = CollectionKind.Array;
            return array.ElementType;
        }

        if (type is not INamedTypeSymbol named || named.TypeArguments.Length != 1)
            return null;

        var definition = named.OriginalDefinition.ToDisplayString();
        switch (definition)
        {
            case "System.Collections.Generic.List<T>":
            case "System.Collections.Generic.IList<T>":
            case "System.Collections.Generic.ICollection<T>":
            case "System.Collections.Generic.IEnumerable<T>":
            case "System.Collections.Generic.IReadOnlyList<T>":
            case "System.Collections.Generic.IReadOnlyCollection<T>":
                kind = CollectionKind.List;
                return named.TypeArguments[0];
            case "System.Collections.Generic.HashSet<T>":
            case "System.Collections.Generic.ISet<T>":
                kind = CollectionKind.HashSet;
                return named.TypeArguments[0];
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the key and value types of a source dictionary (any type implementing
    /// <c>IDictionary&lt;TKey, TValue&gt;</c> or <c>IReadOnlyDictionary&lt;TKey, TValue&gt;</c>).
    /// </summary>
    /// <param name="type">The source type symbol.</param>
    /// <param name="keyType">The dictionary key type.</param>
    /// <returns>The dictionary value type, or <see langword="null"/> if the type is not a dictionary.</returns>
    public static ITypeSymbol? GetSourceDictionaryTypes(ITypeSymbol type, out ITypeSymbol? keyType)
    {
        keyType = null;

        if (type is INamedTypeSymbol named && IsDictionaryInterface(named))
        {
            keyType = named.TypeArguments[0];
            return named.TypeArguments[1];
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (IsDictionaryInterface(iface))
            {
                keyType = iface.TypeArguments[0];
                return iface.TypeArguments[1];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the key and value types of a supported destination dictionary type.
    /// </summary>
    /// <param name="type">The destination type symbol.</param>
    /// <param name="keyType">The dictionary key type.</param>
    /// <returns>The dictionary value type, or <see langword="null"/> if the destination type is not supported.</returns>
    public static ITypeSymbol? GetDestinationDictionaryTypes(ITypeSymbol type, out ITypeSymbol? keyType)
    {
        keyType = null;

        if (type is not INamedTypeSymbol named || named.TypeArguments.Length != 2)
            return null;

        var definition = named.OriginalDefinition.ToDisplayString();
        switch (definition)
        {
            case "System.Collections.Generic.Dictionary<TKey, TValue>":
            case "System.Collections.Generic.IDictionary<TKey, TValue>":
            case "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>":
                keyType = named.TypeArguments[0];
                return named.TypeArguments[1];
            default:
                return null;
        }
    }

    private static bool IsDictionaryInterface(INamedTypeSymbol type)
    {
        return IsGeneric(type, "System.Collections.Generic.IDictionary<TKey, TValue>")
            || IsGeneric(type, "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>");
    }

    private static bool IsGeneric(INamedTypeSymbol type, string definition)
    {
        return type.IsGenericType
            && string.Equals(type.OriginalDefinition.ToDisplayString(), definition, StringComparison.Ordinal);
    }
}
