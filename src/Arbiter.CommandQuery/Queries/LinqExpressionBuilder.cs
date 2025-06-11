using System.Text;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Builds a <see langword="string"/>-based LINQ expression from an <see cref="EntityFilter"/> instance.
/// </summary>
/// <remarks>
/// This class converts an <see cref="EntityFilter"/> (including nested/grouped filters)
/// into a LINQ expression string and a corresponding list of parameter values.
/// </remarks>
/// <example>
/// The following example demonstrates how to use <see cref="LinqExpressionBuilder"/> to build a LINQ expression from an <see cref="EntityFilter"/>:
/// <code>
/// // Create a filter for entities where Status == "Active" and Priority > 1
/// var filter = new EntityFilter
/// {
///     Logic = "and",
///     Filters = new List&lt;EntityFilter&gt;
///     {
///         new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
///         new EntityFilter { Name = "Priority", Operator = "gt", Value = 1 }
///     }
/// };
///
/// // Build the LINQ expression
/// var builder = new LinqExpressionBuilder();
/// builder.Build(filter);
///
/// // Access the generated expression and parameters
/// string expression = builder.Expression; // e.g., "(Status == @0 and Priority > @1)"
/// var parameters = builder.Parameters;    // [ "Active", 1 ]
/// </code>
/// </example>
public class LinqExpressionBuilder
{
    private static readonly Dictionary<string, string> _operatorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { EntityFilterOperators.Equal, "==" },
        { EntityFilterOperators.NotEqual, "!=" },
        { EntityFilterOperators.LessThan, "<" },
        { EntityFilterOperators.LessThanOrEqual, "<=" },
        { EntityFilterOperators.GreaterThan, ">" },
        { EntityFilterOperators.GreaterThanOrEqual, ">=" },
        { "equals", "==" },
        { "not equals", "!=" },
        { "starts with", EntityFilterOperators.StartsWith },
        { "ends with", EntityFilterOperators.EndsWith },
        { "is null", EntityFilterOperators.IsNull },
        { "is empty", EntityFilterOperators.IsNull },
        { "is not null", EntityFilterOperators.IsNotNull },
        { "is not empty", EntityFilterOperators.IsNotNull },
    };

    private readonly StringBuilder _expression = new();
    private readonly List<object?> _values = [];

    /// <summary>
    /// Gets the list of parameter values used in the generated LINQ expression.
    /// </summary>
    public IReadOnlyList<object?> Parameters => _values;

    /// <summary>
    /// Gets the generated LINQ expression string.
    /// </summary>
    public string Expression => _expression.ToString();

    /// <summary>
    /// Builds a string-based LINQ expression from the specified <see cref="EntityFilter"/>.
    /// </summary>
    /// <param name="entityFilter">The entity filter to build the expression from.</param>
    /// <remarks>
    /// This method clears any previous expression and parameters before building a new one.
    /// </remarks>
    public void Build(EntityFilter? entityFilter)
    {
        _expression.Clear();
        _values.Clear();

        if (entityFilter is not null)
            Visit(entityFilter);
    }

    /// <summary>
    /// Visits the provided <see cref="EntityFilter"/> and appends its expression to the builder.
    /// </summary>
    /// <param name="entityFilter">The filter to process.</param>
    private void Visit(EntityFilter entityFilter)
    {
        // If the filter is a group (has nested filters), write the group; otherwise, write a single expression.
        if (!WriteGroup(entityFilter))
            WriteExpression(entityFilter);
    }

    /// <summary>
    /// Writes a single filter expression to the builder.
    /// </summary>
    /// <param name="entityFilter">The filter to write.</param>
    /// <remarks>
    /// Handles translation of operators, negation, and function calls (e.g., StartsWith, EndsWith).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Expression Logic")]
    private void WriteExpression(EntityFilter entityFilter)
    {
        var name = entityFilter.Name;
        if (string.IsNullOrWhiteSpace(name))
            return;

        int index = _values.Count;
        var value = entityFilter.Value;

        // Translate the operator to its LINQ/C# equivalent.
        var operatorValue = string.IsNullOrWhiteSpace(entityFilter.Operator) ? "==" : entityFilter.Operator!;
        if (!_operatorMap.TryGetValue(operatorValue, out var comparison))
            comparison = operatorValue.Trim();

        // Determine if the operator implies negation.
        var negation = comparison.StartsWith('!') || comparison.StartsWith("not", StringComparison.OrdinalIgnoreCase) ? "!" : string.Empty;

        // Handle function-based operators (e.g., StartsWith, EndsWith, Contains).
        if (comparison.EndsWith(EntityFilterOperators.StartsWith, StringComparison.OrdinalIgnoreCase))
        {
            _expression.Append(negation).Append(name).Append(".StartsWith(@").Append(index).Append(')');
            _values.Add(value);
        }
        else if (comparison.EndsWith(EntityFilterOperators.EndsWith, StringComparison.OrdinalIgnoreCase))
        {
            _expression.Append(negation).Append(name).Append(".EndsWith(@").Append(index).Append(')');
            _values.Add(value);
        }
        else if (comparison.EndsWith(EntityFilterOperators.Contains, StringComparison.OrdinalIgnoreCase))
        {
            _expression.Append(negation).Append(name).Append(".Contains(@").Append(index).Append(')');
            _values.Add(value);
        }
        else if (comparison.EndsWith(EntityFilterOperators.IsNull, StringComparison.OrdinalIgnoreCase))
        {
            // Null check
            _expression.Append(name).Append(" == NULL");
        }
        else if (comparison.EndsWith(EntityFilterOperators.IsNotNull, StringComparison.OrdinalIgnoreCase))
        {
            // Not null check
            _expression.Append(name).Append(" != NULL");
        }
        else if (comparison.EndsWith(EntityFilterOperators.In, StringComparison.OrdinalIgnoreCase))
        {
            // "In" operator (e.g., value in collection)
            _expression.Append(negation).Append("it.").Append(name).Append(" in @").Append(index);
            _values.Add(value);
        }
        else if (comparison.EndsWith(EntityFilterOperators.Expression, StringComparison.OrdinalIgnoreCase))
        {
            // Custom expression
            var expression = index == 0 ? name : name.Replace("@0", $"@{index}", StringComparison.OrdinalIgnoreCase);
            _expression.Append(negation).Append(expression);
            _values.Add(value);
        }
        else
        {
            // Default: direct comparison (e.g., ==, !=, <, >, etc.)
            _expression.Append(name).Append(' ').Append(comparison).Append(" @").Append(index);
            _values.Add(value);
        }
    }

    /// <summary>
    /// Writes a group of filters (with logical operator) to the builder.
    /// </summary>
    /// <param name="entityFilter">The group filter to write.</param>
    /// <returns>
    /// <see langword="true"/> if a group was written; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Groups are written in parentheses and combined using the specified logic ("and"/"or").
    /// </remarks>
    private bool WriteGroup(EntityFilter entityFilter)
    {
        var filters = entityFilter.Filters;
        if (filters is null || filters.Count == 0)
            return false;

        // Count valid filters to avoid writing empty groups.
        int validCount = 0;
        foreach (var f in filters)
        {
            if (f.IsValid())
                validCount++;
        }

        if (validCount == 0)
            return false;

        // Determine the logical operator ("and" or "or").
        var logic = string.IsNullOrWhiteSpace(entityFilter.Logic)
            ? EntityFilterLogic.And
            : entityFilter.Logic!;

        _expression.Append('(');
        bool wroteFirst = false;
        foreach (var filter in filters)
        {
            if (!filter.IsValid())
                continue;

            if (wroteFirst)
                _expression.Append(' ').Append(logic).Append(' ');

            Visit(filter);
            wroteFirst = true;
        }
        _expression.Append(')');

        return true;
    }
}
