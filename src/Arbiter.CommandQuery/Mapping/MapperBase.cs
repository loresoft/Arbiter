using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Mapping;

/// <summary>
/// Provides a base implementation for mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// This class compiles mapping expressions at construction time for optimal runtime performance.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
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
{
    private readonly Action<TSource, TDestination>? _compiledMapper;
    private readonly Func<TSource, TDestination> _compiledFactory;
    private readonly Expression<Func<TSource, TDestination>> _compiledProjection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapperBase{TSource, TDestination}"/> class.
    /// Creates and compiles the mapping expressions for optimal runtime performance.
    /// </summary>
    [RequiresUnreferencedCode("Expression compilation requires unreferenced code for AOT scenarios")]
    [RequiresDynamicCode("Expression compilation requires dynamic code generation")]
    protected MapperBase()
    {
        var (factory, mapper, projection) = CreateMappers();

        _compiledFactory = factory;
        _compiledMapper = mapper;
        _compiledProjection = projection;
    }


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
        if (source is null)
            return default;

        return _compiledFactory(source)!;
    }

    /// <summary>
    /// Maps values from a source object to an existing destination object by updating the destination's properties.
    /// </summary>
    /// <param name="source">The source object to map from. Cannot be null.</param>
    /// <param name="destination">The destination object to map to. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mapping expression defined in <see cref="CreateMapping"/> does not support mapping to existing objects.
    /// This occurs when the expression uses constructor parameters, record types, or other patterns that don't generate settable property assignments.
    /// </exception>
    /// <remarks>
    /// This method updates the properties of an existing destination object rather than creating a new instance.
    /// It requires the mapping expression to use object initializer syntax with settable properties.
    /// </remarks>
    public void Map(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        if (_compiledMapper is null)
        {
            throw new InvalidOperationException(
                $"Cannot map to existing instance of '{typeof(TDestination).Name}'. The mapping expression must use object initializer " +
                $"syntax with settable properties (e.g., 'new {typeof(TDestination).Name} {{ Property = value }}'). This operation is not " +
                $"supported for record types or mappings that use constructor parameters only.");
        }

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
    /// Creates the compiled mapping delegates and expression from the mapping definition.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - Factory function for creating new destination instances
    /// - Mapper action for updating existing destination instances
    /// - Projection expression for queryable operations
    /// </returns>
    [RequiresUnreferencedCode("Expression compilation and member access requires unreferenced code for AOT scenarios")]
    [RequiresDynamicCode("Expression compilation requires dynamic code generation")]
    private (Func<TSource, TDestination>, Action<TSource, TDestination>?, Expression<Func<TSource, TDestination>>) CreateMappers()
    {
        // Get the mapping expression defined by the derived class
        var mappingExpression = CreateMapping();

        // Rewrite the expression to add null checks for nested property access
        var rewrittenExpression = RewriteNestedPropertyAccess(mappingExpression);

        // Compile the expression into a factory function for creating new objects
        var factory = rewrittenExpression.Compile();

        // Create mapper that assigns to existing object with optimized expression building
        // We need new parameters for the mapper action (source, destination)
        var sourceParam = Expression.Parameter(typeof(TSource), "source");
        var destinationParam = Expression.Parameter(typeof(TDestination), "destination");

        // Extract individual property assignments from the mapping expression
        // This transforms "new Dest { Prop = src.Value }" into "dest.Prop = src.Value" assignments
        if (!TryExtractAssignments(rewrittenExpression, sourceParam, destinationParam, out var assignments))
        {
            // If assignments could not be extracted (unsupported mapping expression),
            return (factory, null, rewrittenExpression);
        }

        if (assignments.Count == 0)
        {
            // If no assignments were extracted, create an empty action that does nothing
            // This can happen with simple constructor-only mappings
            var emptyAction = Expression.Lambda<Action<TSource, TDestination>>(Expression.Empty(), sourceParam, destinationParam).Compile();
            return (factory, emptyAction, rewrittenExpression);
        }

        // Create the expression body - either a single assignment or a block of assignments
        var block = assignments.Count == 1
            ? assignments[0]                    // Single assignment doesn't need a block wrapper
            : Expression.Block(assignments);    // Multiple assignments need to be wrapped in a block

        // Create the lambda expression for the mapper action and compile it
        var mapperLambda = Expression.Lambda<Action<TSource, TDestination>>(block, sourceParam, destinationParam);
        var mapper = mapperLambda.Compile();

        return (factory, mapper, rewrittenExpression);
    }

    /// <summary>
    /// Rewrites the mapping expression to add null checks for nested property access.
    /// Transforms expressions like 'source.Distributor.Name' into 'source.Distributor != null ? source.Distributor.Name : default'.
    /// </summary>
    /// <param name="expression">The original mapping expression.</param>
    /// <returns>The rewritten expression with null checks injected.</returns>
    [RequiresUnreferencedCode("Expression manipulation requires unreferenced code for AOT scenarios")]
    private static Expression<Func<TSource, TDestination>> RewriteNestedPropertyAccess(Expression<Func<TSource, TDestination>> expression)
    {
        var visitor = new NullCheckInjector();
        var rewrittenBody = visitor.Visit(expression.Body);
        return Expression.Lambda<Func<TSource, TDestination>>(rewrittenBody, expression.Parameters);
    }

    /// <summary>
    /// Attempts to extract assignment expressions from a mapping expression for use in updating existing destination objects.
    /// </summary>
    /// <param name="mappingExpression">The original mapping expression that creates a new destination instance.</param>
    /// <param name="sourceParam">The parameter expression representing the source object.</param>
    /// <param name="destinationParam">The parameter expression representing the destination object to update.</param>
    /// <param name="assignments">When successful, contains a list of assignment expressions that can be used to update an existing destination object.</param>
    /// <returns>
    /// <see langword="true"/> if the assignments were successfully extracted; otherwise, <see langword="false"/>.
    /// Returns <see langword="false"/> when the mapping expression body is not a <see cref="MemberInitExpression"/>.
    /// </returns>
    /// <remarks>
    /// This method handles <see cref="MemberInitExpression"/> (object initializer syntax) to extract property assignments.
    /// Only object initializer syntax is supported (e.g., <c>new Destination { Property = value }</c>).
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Lots of comments")]
    [RequiresUnreferencedCode("Member access and expression manipulation requires unreferenced code for AOT scenarios")]
    private static bool TryExtractAssignments(
        Expression<Func<TSource, TDestination>> mappingExpression,
        ParameterExpression sourceParam,
        ParameterExpression destinationParam,
        out List<Expression> assignments)
    {
        // Handle object initializer syntax: new Dest { Prop1 = value1, Prop2 = value2 }
        if (mappingExpression.Body is not MemberInitExpression memberInitExpression)
        {
            assignments = [];
            return false;
        }

        // Pre-allocate capacity for better performance
        assignments = new List<Expression>(memberInitExpression.Bindings.Count);

        // Create a visitor that will replace the original source parameter with our new one
        // This is needed because we're changing from "source => new Dest { ... }" to "source, dest => dest.Prop = ..."
        var parameterReplacer = new ParameterReplacer(mappingExpression.Parameters[0], sourceParam);

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

        return true;
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

    /// <summary>
    /// An expression visitor that injects null checks for nested property access.
    /// Transforms expressions like 'source.Distributor.Name' into conditional expressions
    /// that check for null at each level of nesting.
    /// Skips rewriting if the expression already contains explicit null checks.
    /// </summary>
    private sealed class NullCheckInjector : ExpressionVisitor
    {
        private bool _insideConditionalTest;

        /// <summary>
        /// Visits a conditional expression and skips rewriting the test condition to avoid interfering with existing null checks.
        /// </summary>
        /// <param name="node">The conditional expression to visit.</param>
        /// <returns>The visited conditional expression.</returns>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            // Check if this conditional expression is already performing a null check
            if (IsNullCheckPattern(node))
            {
                // This is already a null check pattern (e.g., x != null ? x.Property : default)
                // Don't rewrite it, just return as-is
                return node;
            }

            // Visit the test expression without injecting null checks
            // (the test is often where null checks are performed)
            var previousInsideTest = _insideConditionalTest;
            _insideConditionalTest = true;
            var test = Visit(node.Test);
            _insideConditionalTest = previousInsideTest;

            // Visit the true and false branches normally
            var ifTrue = Visit(node.IfTrue);
            var ifFalse = Visit(node.IfFalse);

            return node.Update(test, ifTrue, ifFalse);
        }

        /// <summary>
        /// Visits a member access expression and injects null checks for nested property access.
        /// </summary>
        /// <param name="node">The member access expression to visit.</param>
        /// <returns>
        /// A conditional expression with null checks if the member access is nested,
        /// otherwise the original expression after visiting its components.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            // If we're inside a conditional test expression, don't inject null checks
            // as we're likely already checking for null
            if (_insideConditionalTest)
            {
                return base.VisitMember(node);
            }

            // First, visit the expression to ensure any nested member accesses are also processed
            var visitedExpression = Visit(node.Expression);
            
            // If the expression is null (static member) or not a member access, no null check needed
            if (visitedExpression is null || visitedExpression is not MemberExpression)
                return node.Update(visitedExpression);

            // Collect the chain of member accesses (e.g., source.Distributor.Partner.Name)
            var memberChain = new List<MemberExpression>();
            var current = node;
            
            while (current.Expression is MemberExpression memberExpression)
            {
                memberChain.Add(current);
                current = memberExpression;
            }
            
            // If there's no chain (direct property access), no null check needed
            if (memberChain.Count == 0)
                return node.Update(visitedExpression);

            // Only inject null checks for reference types
            var rootExpression = current.Expression;
            if (rootExpression is null || !current.Type.IsClass)
                return node.Update(visitedExpression);

            // Build the null check condition from the bottom up
            // For source.Distributor.Partner.Name, we need:
            // source.Distributor != null && source.Distributor.Partner != null
            Expression? condition = null;
            Expression rebuiltExpression = rootExpression;

            // Start from the root and build up the condition
            for (int i = memberChain.Count - 1; i >= 0; i--)
            {
                var member = memberChain[i];
                rebuiltExpression = Expression.MakeMemberAccess(rebuiltExpression, member.Member);

                // Only add null checks for reference types
                if (member.Expression!.Type.IsClass)
                {
                    var nullCheck = Expression.NotEqual(
                        member.Expression,
                        Expression.Constant(null, member.Expression.Type));

                    condition = condition is null
                        ? nullCheck
                        : Expression.AndAlso(condition, nullCheck);
                }
            }

            // If no condition was built, return the original expression
            if (condition is null)
                return node.Update(visitedExpression);

            // Create the conditional expression: condition ? value : default
            var defaultValue = Expression.Default(node.Type);
            return Expression.Condition(condition, rebuiltExpression, defaultValue);
        }

        /// <summary>
        /// Determines if a conditional expression represents a null check pattern.
        /// Recognizes patterns like: x != null ? x.Property : default
        /// </summary>
        /// <param name="node">The conditional expression to check.</param>
        /// <returns>True if the expression is a null check pattern; otherwise, false.</returns>
        private static bool IsNullCheckPattern(ConditionalExpression node)
        {
            // Pattern 1: x != null ? ... : default
            // Pattern 2: x == null ? default : ...
            if (node.Test is BinaryExpression binaryTest)
            {
                var isNotEqual = binaryTest.NodeType == ExpressionType.NotEqual;
                var isEqual = binaryTest.NodeType == ExpressionType.Equal;

                if (!isNotEqual && !isEqual)
                    return false;

                // Check if one side is a null constant
                var isLeftNull = binaryTest.Left is ConstantExpression leftConst && leftConst.Value is null;
                var isRightNull = binaryTest.Right is ConstantExpression rightConst && rightConst.Value is null;

                if (!isLeftNull && !isRightNull)
                    return false;

                // The other side should be a member access or parameter
                var checkedExpression = isLeftNull ? binaryTest.Right : binaryTest.Left;
                
                // Verify the checked expression is used in one of the branches
                if (isNotEqual)
                {
                    // x != null ? x.Property : default
                    return ContainsMemberAccess(node.IfTrue, checkedExpression);
                }
                else
                {
                    // x == null ? default : x.Property
                    return ContainsMemberAccess(node.IfFalse, checkedExpression);
                }
            }

            // Pattern 3: AndAlso/OrElse with null checks
            if (node.Test.NodeType == ExpressionType.AndAlso || node.Test.NodeType == ExpressionType.OrElse)
            {
                return ContainsNullCheck(node.Test);
            }

            return false;
        }

        /// <summary>
        /// Checks if an expression contains a member access based on the specified expression.
        /// </summary>
        private static bool ContainsMemberAccess(Expression expression, Expression baseExpression)
        {
            if (expression is MemberExpression memberExpr)
            {
                // Check if this member expression or any parent matches
                var current = memberExpr;
                while (current != null)
                {
                    if (ExpressionEquals(current.Expression, baseExpression))
                        return true;

                    current = current.Expression as MemberExpression;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a binary expression contains null checks (x != null or x == null).
        /// </summary>
        private static bool ContainsNullCheck(Expression expression)
        {
            if (expression is BinaryExpression binary)
            {
                var isNullCheck = (binary.NodeType == ExpressionType.NotEqual || binary.NodeType == ExpressionType.Equal) &&
                                  (IsNullConstant(binary.Left) || IsNullConstant(binary.Right));

                if (isNullCheck)
                    return true;

                // Recursively check left and right for AndAlso/OrElse chains
                if (binary.NodeType == ExpressionType.AndAlso || binary.NodeType == ExpressionType.OrElse)
                {
                    return ContainsNullCheck(binary.Left) || ContainsNullCheck(binary.Right);
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an expression is a null constant.
        /// </summary>
        private static bool IsNullConstant(Expression expression)
        {
            return expression is ConstantExpression constant && constant.Value is null;
        }

        /// <summary>
        /// Compares two expressions for structural equality.
        /// </summary>
        private static bool ExpressionEquals(Expression? expr1, Expression? expr2)
        {
            if (ReferenceEquals(expr1, expr2))
                return true;

            if (expr1 is null || expr2 is null)
                return false;

            if (expr1.NodeType != expr2.NodeType || expr1.Type != expr2.Type)
                return false;

            // For parameters, compare by name and type
            if (expr1 is ParameterExpression param1 && expr2 is ParameterExpression param2)
                return param1.Name == param2.Name && param1.Type == param2.Type;

            // For member access, compare member and expression
            if (expr1 is MemberExpression member1 && expr2 is MemberExpression member2)
                return member1.Member == member2.Member && ExpressionEquals(member1.Expression, member2.Expression);

            return false;
        }
    }
}
