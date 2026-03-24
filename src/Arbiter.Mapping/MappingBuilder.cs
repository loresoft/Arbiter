using System.Linq.Expressions;

namespace Arbiter.Mapping;

/// <summary>
/// Provides a fluent API for configuring property mappings between a source and destination type.
/// </summary>
/// <remarks>
/// <para>
/// This class is never used at runtime. The source generator parses calls to its members as syntax
/// at compile time to discover custom mapping instructions and emit strongly-typed mapping code.
/// </para>
/// </remarks>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public class MappingBuilder<TSource, TDestination>
{
    /// <summary>
    /// Begins configuration for a specific destination property.
    /// </summary>
    /// <typeparam name="TMember">The type of the destination property.</typeparam>
    /// <param name="destinationMember">The expression that identifies the destination property.</param>
    /// <returns>A <see cref="PropertyBuilder{TSource, TDestination, TMember}"/> to configure the property mapping.</returns>
    public PropertyBuilder<TSource, TDestination, TMember> Property<TMember>(Expression<Func<TDestination, TMember>> destinationMember)
    {
        return new PropertyBuilder<TSource, TDestination, TMember>(destinationMember);
    }
}
