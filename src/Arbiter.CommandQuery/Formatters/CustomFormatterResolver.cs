using MessagePack;
using MessagePack.Formatters;

namespace Arbiter.CommandQuery.Formatters;

/// <summary>
/// Resolves custom MessagePack formatters used by the application.
/// </summary>
public sealed class CustomFormatterResolver : IFormatterResolver
{
    /// <summary>
    /// The singleton instance that can be used.
    /// </summary>
    public static readonly CustomFormatterResolver Instance = new();

    private CustomFormatterResolver()
    {
    }

    /// <summary>
    /// Gets a formatter for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve a formatter for.</typeparam>
    /// <returns>The resolved formatter, or <see langword="null"/> when no formatter is available.</returns>
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    /// <summary>
    /// Caches resolved formatters per type for efficient repeated lookup.
    /// </summary>
    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            if (typeof(T) == typeof(object))
            {
                Formatter = (IMessagePackFormatter<T>)(object)new TypePreservingFormatter();
            }
        }
    }
}
