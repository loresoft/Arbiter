using System.Linq.Expressions;

namespace Arbiter.Mapping;

/// <summary>
/// Configures how a destination property is mapped from a source type.
/// </summary>
/// <remarks>
/// <para>
/// This class is never used at runtime. The source generator parses calls to its members
/// (<see cref="From{TSourceMember}"/>, <see cref="Value"/>, <see cref="Ignore"/>) as syntax
/// at compile time to discover custom mapping instructions and emit strongly-typed mapping code
/// without runtime reflection.
/// </para>
/// </remarks>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
/// <typeparam name="TMember">The type of the destination property.</typeparam>
public class PropertyBuilder<TSource, TDestination, TMember>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBuilder{TSource, TDestination, TMember}"/> class.
    /// </summary>
    /// <param name="destinationMember">The expression that identifies the destination property.</param>
    public PropertyBuilder(Expression<Func<TDestination, TMember>> destinationMember)
    {
        DestinationExpression = destinationMember;
    }

    /// <summary>
    /// Gets the expression that identifies the destination property.
    /// </summary>
    public Expression<Func<TDestination, TMember>> DestinationExpression { get; }

    /// <summary>
    /// Gets the expression that identifies the source property to map from.
    /// </summary>
    public Expression? SourceExpression { get; private set; }

    /// <summary>
    /// Gets the constant value to assign to the destination property.
    /// </summary>
    public TMember? ConstantValue { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the destination property should be ignored during mapping.
    /// </summary>
    public bool IsIgnored { get; private set; }

    /// <summary>
    /// Configures the destination property to map from the specified source expression.
    /// </summary>
    /// <typeparam name="TSourceMember">The type of the source property.</typeparam>
    /// <param name="sourceExpression">The expression that identifies the source property.</param>
    public void From<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression)
    {
        SourceExpression = sourceExpression;
    }

    /// <summary>
    /// Configures the destination property to use a constant value.
    /// </summary>
    /// <param name="value">The constant value to assign.</param>
    public void Value(TMember value)
    {
        ConstantValue = value;
    }

    /// <summary>
    /// Configures the destination property to be ignored during mapping.
    /// </summary>
    public void Ignore()
    {
        IsIgnored = true;
    }
}
