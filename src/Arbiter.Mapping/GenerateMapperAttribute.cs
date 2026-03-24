using System.Diagnostics;

namespace Arbiter.Mapping;

/// <summary>
/// Marks a <see cref="MapperProfile{TSource, TDestination}"/> derived class for source generation of mapping implementations.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a partial class that inherits from <see cref="MapperProfile{TSource, TDestination}"/>, the source
/// generator emits overrides for <c>Map</c> and <c>ProjectTo</c> at compile time. The generated code maps
/// properties that share a common name and compatible type between source and destination automatically.
/// </para>
/// <para>
/// To customize the generated mapping, override <see cref="MapperProfile{TSource, TDestination}.ConfigureMapping"/>
/// and use the <see cref="MappingBuilder{TSource, TDestination}"/> to define custom source expressions,
/// constant values, or ignored properties.
/// </para>
/// <para>
/// The attribute is conditional on the <c>ARBITER_GENERATOR</c> symbol and is omitted from the compiled assembly.
/// </para>
/// </remarks>
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
[Conditional("ARBITER_GENERATOR")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateMapperAttribute : Attribute
{
}
