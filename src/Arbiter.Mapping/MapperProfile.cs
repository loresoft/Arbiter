#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException

using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Mapping;

/// <summary>
/// Abstract base class for mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// </summary>
/// <remarks>
/// <para>
/// When a derived class is annotated with <see cref="GenerateMapperAttribute"/>, the source generator
/// implements <see cref="IMapper{TSource, TDestination}"/> on the generated partial class, providing
/// <c>Map</c> and <c>ProjectTo</c> methods. The generated code maps properties that share a common
/// name and compatible type between source and destination, and applies any custom expressions
/// defined in <see cref="ConfigureMapping"/>.
/// </para>
/// </remarks>
/// <typeparam name="TSource">Source type to map from.</typeparam>
/// <typeparam name="TDestination">Destination type to map to.</typeparam>
/// <example>
/// <code>
/// [GenerateMapper]
/// public partial class OrderMapper : MapperProfile&lt;OrderEntity, OrderModel&gt;
/// {
///     protected override void ConfigureMapping(MappingBuilder&lt;OrderEntity, OrderModel&gt; mapping)
///     {
///         mapping.Property(d => d.Total).From(s => s.Price * s.Quantity);
///         mapping.Property(d => d.InternalNotes).Ignore();
///     }
/// }
/// </code>
/// </example>
public abstract class MapperProfile<TSource, TDestination> : IMapper<TSource, TDestination>
{
    /// <inheritdoc />
    /// <exception cref="NotImplementedException">
    /// Thrown when the derived class is not annotated with <see cref="GenerateMapperAttribute"/>.
    /// </exception>
    [return: NotNullIfNotNull(nameof(source))]
    public virtual TDestination? Map(TSource? source)
    {
        throw new NotImplementedException(
            $"Mapping from '{typeof(TSource).Name}' to '{typeof(TDestination).Name}' is not implemented. " +
            $"Apply the [GenerateMapper] attribute to '{GetType().Name}' to generate the mapping implementation.");
    }

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">
    /// Thrown when the derived class is not annotated with <see cref="GenerateMapperAttribute"/>.
    /// </exception>
    public virtual void Map(TSource source, TDestination destination)
    {
        throw new NotImplementedException(
            $"Mapping from '{typeof(TSource).Name}' to '{typeof(TDestination).Name}' is not implemented. " +
            $"Apply the [GenerateMapper] attribute to '{GetType().Name}' to generate the mapping implementation.");
    }

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">
    /// Thrown when the derived class is not annotated with <see cref="GenerateMapperAttribute"/>.
    /// </exception>
    public virtual IQueryable<TDestination> ProjectTo(IQueryable<TSource> source)
    {
        throw new NotImplementedException(
            $"Projection from '{typeof(TSource).Name}' to '{typeof(TDestination).Name}' is not implemented. " +
            $"Apply the [GenerateMapper] attribute to '{GetType().Name}' to generate the projection implementation.");
    }

    /// <summary>
    /// Override to configure custom property mappings between source and destination types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is never executed at runtime. The source generator reads its body at compile time
    /// to discover custom mapping instructions (property sources, constant values, and ignored properties)
    /// and incorporates them into the generated <c>Map</c> and <c>ProjectTo</c> implementations.
    /// </para>
    /// <para>
    /// Because the method body is only parsed as syntax, it must contain only
    /// <see cref="MappingBuilder{TSource, TDestination}"/> configuration calls.
    /// Arbitrary runtime logic such as conditionals, loops, or service calls is not supported
    /// and will be silently ignored by the generator.
    /// </para>
    /// </remarks>
    /// <param name="mapping">The <see cref="MappingBuilder{TSource, TDestination}"/> used to configure property mappings.</param>
    protected virtual void ConfigureMapping(MappingBuilder<TSource, TDestination> mapping)
    {

    }
}
