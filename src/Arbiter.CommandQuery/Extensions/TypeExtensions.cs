using System.Net;

using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Models;
using Arbiter.Services;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// <see cref="Type"/> extensions methods
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified type implements an interface.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if type implements the interface; otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException">Only interfaces can be implemented.</exception>
    public static bool Implements<TInterface>(this Type type)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(type);

        var interfaceType = typeof(TInterface);

        if (!interfaceType.IsInterface)
            throw new InvalidOperationException("Only interfaces can be implemented.");

        return interfaceType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets a portable fully qualified name for a Type, suitable for serialization headers.
    /// Removes Version, Culture, and PublicKeyToken from the assembly qualified name.
    /// Supports nested generic types with minimal allocations.
    /// </summary>
    public static string GetPortableName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var handler = new ValueStringBuilder(stackalloc char[256]);
        try
        {
            AppendTypeName(ref handler, type);
            return handler.ToString();
        }
        finally
        {
            handler.Dispose();
        }
    }

    private static void AppendTypeName(ref ValueStringBuilder builder, Type type)
    {
        // Handle generic type definitions
        if (!type.IsGenericType)
        {
            // Non-generic type
            builder.Append(type.FullName ?? type.Name);
            builder.Append(", ");

            AppendAssemblyName(ref builder, type.Assembly.FullName);
            return;
        }

        var genericTypeDef = type.GetGenericTypeDefinition();
        var fullName = genericTypeDef.FullName ?? genericTypeDef.Name;

        builder.Append(fullName);

        // Append generic arguments
        builder.Append('[');
        var genericArgs = type.GetGenericArguments();
        for (int i = 0; i < genericArgs.Length; i++)
        {
            if (i > 0)
                builder.Append(',');

            builder.Append('[');
            AppendTypeName(ref builder, genericArgs[i]);
            builder.Append(']');
        }
        builder.Append(']');

        // Append assembly name (portable version)
        builder.Append(", ");
        AppendAssemblyName(ref builder, genericTypeDef.Assembly.FullName);
    }

    private static void AppendAssemblyName(ref ValueStringBuilder builder, string? assemblyFullName)
    {
        if (assemblyFullName == null)
            return;

        var span = assemblyFullName.AsSpan();

        // Find the first comma (separates name from version/culture/token)
        var commaIndex = span.IndexOf(',');
        if (commaIndex > 0)
            builder.Append(span[..commaIndex]);
        else
            builder.Append(span);
    }
}
