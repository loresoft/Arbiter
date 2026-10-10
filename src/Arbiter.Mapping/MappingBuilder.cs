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

    /// <summary>
    /// Opts a nested type pair into deep clone mapping.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wherever a property of type <typeparamref name="TNestedSource"/> is mapped to a property of type
    /// <typeparamref name="TNestedDestination"/> (at any depth, including collection elements and dictionary values),
    /// the generator emits a new destination instance instead of copying the reference.
    /// </para>
    /// <para>
    /// Nested types that are not configured are assigned directly (reference copy).
    /// When <paramref name="configure"/> is omitted, properties are matched by name.
    /// </para>
    /// </remarks>
    /// <typeparam name="TNestedSource">The nested source type.</typeparam>
    /// <typeparam name="TNestedDestination">The nested destination type.</typeparam>
    /// <param name="configure">Optional configuration for the nested mapping.</param>
    /// <returns>The current <see cref="MappingBuilder{TSource, TDestination}"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// mapping.Map&lt;AddressEntity, AddressModel&gt;(address =>
    /// {
    ///     address.Property(d => d.Zip).From(s => s.PostalCode);
    /// });
    /// </code>
    /// </example>
    public MappingBuilder<TSource, TDestination> Map<TNestedSource, TNestedDestination>(
        Action<MappingBuilder<TNestedSource, TNestedDestination>>? configure = null)
    {
        return this;
    }
}
