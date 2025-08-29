using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Mapping;

/// <summary>
/// Provides a base implementation for mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// This class compiles mapping expressions at construction time for optimal runtime performance.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to. Must be a reference type with a parameterless constructor.</typeparam>
/// <remarks>
/// For best performance, register implementations of <see cref="MapperBase{TSource, TDestination}"/> as singletons in your dependency injection container.
/// The mapping expressions are compiled once at construction and reused for all mapping operations, making singleton registration ideal.
/// </remarks>
/// <example>
/// Here's an example implementation for mapping from a User entity to a UserDto:
/// <code>
/// public class UserMapper : MapperBase&lt;User, UserDto&gt;
/// {
///     protected override Expression&lt;Func&lt;User, UserDto&gt;&gt; CreateMapping()
///     {
///         // Object initializer syntax - recommended approach
///         return user =&gt; new UserDto
///         {
///             Id = user.Id,
///             FullName = user.FirstName + " " + user.LastName,
///             Email = user.Email,
///             IsActive = user.Status == UserStatus.Active,
///             CreatedDate = user.CreatedAt.Date,
///             Department = user.Department != null ? user.Department.Name : null, // Explicit null check
///             OrderCount = user.Orders.Count(), // Use Count() for query translation
///             TotalOrderAmount = user.Orders.Sum(o =&gt; o.Amount) // Use Sum() for aggregates
///         };
///     }
/// }
/// </code>
/// </example>
public abstract class MapperBase<TSource, TDestination> : IMapper<TSource, TDestination>
    where TDestination : class, new()
{
    private readonly Action<TSource, TDestination> _compiledMapper;
    private readonly Func<TSource, TDestination> _compiledFactory;
    private readonly Expression<Func<TSource, TDestination>> _compiledProjection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapperBase{TSource, TDestination}"/> class.
    /// Creates and compiles the mapping expressions for optimal runtime performance.
    /// </summary>
    protected MapperBase()
    {
        var (factory, mapper, projection) = CreateMappers();

        _compiledFactory = factory;
        _compiledMapper = mapper;
        _compiledProjection = projection;
    }


    /// <summary>
    /// When overridden in a derived class, creates the mapping expression that defines how to transform
    /// from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <returns>An expression that represents the mapping logic from source to destination.</returns>
    /// <remarks>
    /// This method should return an expression that creates a new instance of <typeparamref name="TDestination"/>
    /// and assigns values from the source object. The expression can use member initialization or constructor calls.
    /// <para/>
    /// The expression can include:
    /// <list type="bullet">
    /// <item>Simple property mapping: <c>Name = entity.Name</c></item>
    /// <item>String concatenation: <c>FullName = entity.FirstName + " " + entity.LastName</c></item>
    /// <item>Method calls: <c>UpperName = entity.Name.ToUpper()</c></item>
    /// <item>Conditional logic: <c>IsValid = entity.Status == EntityStatus.Active</c></item>
    /// <item>
    /// <b>Null checks:</b> Since null-conditional operators (e.g., <c>Phone = entity.Contact?.PhoneNumber</c>) are not supported
    /// in expressions, use explicit null checks instead, such as <c>Phone = entity.Contact != null ? entity.Contact.PhoneNumber : null</c>.
    /// </item>
    /// <item>
    /// <b>Aggregates:</b> When projecting aggregate values from collections (such as counts, sums, averages, etc.), use the appropriate
    /// LINQ extension methods (e.g., <c>Count()</c>, <c>Sum()</c>, <c>Average()</c>) instead of collection properties. For example, use
    /// <c>ItemCount = entity.Items.Count()</c> or <c>TotalAmount = entity.Orders.Sum(o =&gt; o.Amount)</c>. This ensures compatibility
    /// with <see cref="ProjectTo"/> and allows the mapping expression to be translated to SQL or other query providers.
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Here's an example implementation for mapping from a User entity to a UserDto:
    /// <code>
    /// public class UserMapper : MapperBase&lt;User, UserDto&gt;
    /// {
    ///     protected override Expression&lt;Func&lt;User, UserDto&gt;&gt; CreateMapping()
    ///     {
    ///         // Object initializer syntax - recommended approach
    ///         return user =&gt; new UserDto
    ///         {
    ///             Id = user.Id,
    ///             FullName = user.FirstName + " " + user.LastName,
    ///             Email = user.Email,
    ///             IsActive = user.Status == UserStatus.Active,
    ///             CreatedDate = user.CreatedAt.Date,
    ///             Department = user.Department != null ? user.Department.Name : null, // Explicit null check
    ///             OrderCount = user.Orders.Count(), // Use Count() for query translation
    ///             TotalOrderAmount = user.Orders.Sum(o =&gt; o.Amount) // Use Sum() for aggregates
    ///         };
    ///     }
    /// }
    /// </code>
    /// </example>
    protected abstract Expression<Func<TSource, TDestination>> CreateMapping();


    /// <summary>
    /// Maps a source object to a new destination object.
    /// </summary>
    /// <param name="source">The source object to map from. Can be null.</param>
    /// <returns>
    /// A new instance of <typeparamref name="TDestination"/> with values mapped from the source,
    /// or null if the source is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map(TSource? source)
    {
        if (source == null)
            return null;

        return _compiledFactory(source);
    }

    /// <summary>
    /// Maps values from a source object to an existing destination object.
    /// </summary>
    /// <param name="source">The source object to map from. Cannot be null.</param>
    /// <param name="destination">The destination object to map to. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    public void Map(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        _compiledMapper(source, destination);
    }

    /// <summary>
    /// Projects a queryable of source objects to a queryable of destination objects.
    /// This method preserves the queryable nature, allowing for deferred execution and translation to SQL or other query languages.
    /// </summary>
    /// <param name="source">The source queryable to project from.</param>
    /// <returns>A queryable of destination objects that represents the projected query.</returns>
    /// <remarks>
    /// This method is particularly useful with Entity Framework and other ORM frameworks as it allows
    /// the mapping expression to be translated to SQL, avoiding loading unnecessary data into memory.
    /// </remarks>
    public IQueryable<TDestination> ProjectTo(IQueryable<TSource> source)
    {
        return source.Select(_compiledProjection);
    }


    /// <summary>
    /// Creates the compiled mapping delegates and expression from the mapping definition.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - Factory function for creating new destination instances
    /// - Mapper action for updating existing destination instances
    /// - Projection expression for queryable operations
    /// </returns>
    private (Func<TSource, TDestination>, Action<TSource, TDestination>, Expression<Func<TSource, TDestination>>) CreateMappers()
    {
        // Get the mapping expression defined by the derived class
        var mappingExpression = CreateMapping();

        // Compile the expression into a factory function for creating new objects
        var factory = mappingExpression.Compile();

        // Create mapper that assigns to existing object with optimized expression building
        // We need new parameters for the mapper action (source, destination)
        var sourceParam = Expression.Parameter(typeof(TSource), "source");
        var destinationParam = Expression.Parameter(typeof(TDestination), "destination");

        // Extract individual property assignments from the mapping expression
        // This transforms "new Dest { Prop = src.Value }" into "dest.Prop = src.Value" assignments
        var assignments = ExtractAssignments(mappingExpression, sourceParam, destinationParam);

        if (assignments.Count == 0)
        {
            // If no assignments were extracted, create an empty action that does nothing
            // This can happen with simple constructor-only mappings
            var emptyAction = Expression.Lambda<Action<TSource, TDestination>>(Expression.Empty(), sourceParam, destinationParam).Compile();
            return (factory, emptyAction, mappingExpression);
        }

        // Create the expression body - either a single assignment or a block of assignments
        var block = assignments.Count == 1
            ? assignments[0]                    // Single assignment doesn't need a block wrapper
            : Expression.Block(assignments);    // Multiple assignments need to be wrapped in a block

        // Create the lambda expression for the mapper action and compile it
        var mapperLambda = Expression.Lambda<Action<TSource, TDestination>>(block, sourceParam, destinationParam);

        return (factory, mapperLambda.Compile(), mappingExpression);
    }

    /// <summary>
    /// Extracts assignment expressions from a mapping expression for use in updating existing destination objects.
    /// </summary>
    /// <param name="mappingExpression">The original mapping expression that creates a new destination instance.</param>
    /// <param name="sourceParam">The parameter expression representing the source object.</param>
    /// <param name="destinationParam">The parameter expression representing the destination object to update.</param>
    /// <returns>A list of assignment expressions that can be used to update an existing destination object.</returns>
    /// <remarks>
    /// This method handles both <see cref="MemberInitExpression"/> (object initializer syntax) and
    /// <see cref="NewExpression"/> (constructor with named parameters) to extract property assignments.
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Lots of comments")]
    private static List<Expression> ExtractAssignments(
        Expression<Func<TSource, TDestination>> mappingExpression,
        ParameterExpression sourceParam,
        ParameterExpression destinationParam)
    {
        var assignments = new List<Expression>();

        // Create a visitor that will replace the original source parameter with our new one
        // This is needed because we're changing from "source => new Dest { ... }" to "source, dest => dest.Prop = ..."
        var parameterReplacer = new ParameterReplacer(mappingExpression.Parameters[0], sourceParam);

        // Analyze the body of the mapping expression to extract assignments
        switch (mappingExpression.Body)
        {
            case MemberInitExpression memberInitExpression:
                // Handle object initializer syntax: new Dest { Prop1 = value1, Prop2 = value2 }

                // Pre-allocate capacity for better performance
                assignments.Capacity = memberInitExpression.Bindings.Count;

                // Process each property binding in the member initializer
                for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
                {
                    MemberBinding? binding = memberInitExpression.Bindings[i];

                    // We only handle member assignments (Prop = value), not other binding types
                    if (binding is not MemberAssignment assignment)
                        continue;

                    // Replace the source parameter in the value expression
                    var value = parameterReplacer.Visit(assignment.Expression);

                    // Create member access on the destination parameter (dest.Prop)
                    var memberAccess = Expression.MakeMemberAccess(destinationParam, assignment.Member);

                    // Create assignment expression: dest.Prop = value
                    assignments.Add(Expression.Assign(memberAccess, value));
                }
                break;

            case NewExpression newExpression when newExpression.Arguments.Count > 0 && newExpression.Members != null:
                // Handle constructor with named parameters: new Dest(prop1: value1, prop2: value2)

                // Pre-allocate capacity for better performance
                assignments.Capacity = newExpression.Arguments.Count;

                // Process each constructor argument
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    // Get the member (property/field) that corresponds to this constructor parameter
                    var member = newExpression.Members[i];

                    // Replace the source parameter in the argument expression
                    var value = parameterReplacer.Visit(newExpression.Arguments[i]);

                    // Create member access on the destination parameter (dest.Member)
                    var memberAccess = Expression.MakeMemberAccess(destinationParam, member);

                    // Create assignment expression: dest.Member = value
                    assignments.Add(Expression.Assign(memberAccess, value));
                }
                break;
        }

        return assignments;
    }

    /// <summary>
    /// An expression visitor that replaces parameter references in expressions.
    /// Used to replace the original source parameter with a new parameter when building assignment expressions.
    /// </summary>
    /// <param name="oldParameter">The original parameter to replace.</param>
    /// <param name="newParameter">The new parameter to use as replacement.</param>
    private sealed class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        : ExpressionVisitor
    {
        /// <summary>
        /// Visits a parameter expression and replaces it if it matches the target parameter.
        /// </summary>
        /// <param name="node">The parameter expression to visit.</param>
        /// <returns>The new parameter if it matches the old parameter, otherwise the original parameter.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
            => ReferenceEquals(node, oldParameter) ? newParameter : node;
    }
}
