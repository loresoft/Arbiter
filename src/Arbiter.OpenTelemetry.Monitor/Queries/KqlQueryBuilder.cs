using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Services;

namespace Arbiter.OpenTelemetry.Monitor.Queries;

/// <summary>
/// Builds KQL queries for Azure Monitor telemetry tables.
/// </summary>
public class KqlQueryBuilder
{
    private static readonly Dictionary<FilterOperators, Action<StringBuilder, EntityFilter>> _filterWriters = [];

    static KqlQueryBuilder()
    {
        _filterWriters.TryAdd(FilterOperators.Contains, WriteStringFilter);
        _filterWriters.TryAdd(FilterOperators.NotContains, WriteStringFilter);
        _filterWriters.TryAdd(FilterOperators.StartsWith, WriteStringFilter);
        _filterWriters.TryAdd(FilterOperators.NotStartsWith, WriteStringFilter);
        _filterWriters.TryAdd(FilterOperators.EndsWith, WriteStringFilter);
        _filterWriters.TryAdd(FilterOperators.NotEndsWith, WriteStringFilter);

        _filterWriters.TryAdd(FilterOperators.Equal, WriteStandardFilter);
        _filterWriters.TryAdd(FilterOperators.NotEqual, WriteStandardFilter);
        _filterWriters.TryAdd(FilterOperators.GreaterThan, WriteStandardFilter);
        _filterWriters.TryAdd(FilterOperators.GreaterThanOrEqual, WriteStandardFilter);
        _filterWriters.TryAdd(FilterOperators.LessThan, WriteStandardFilter);
        _filterWriters.TryAdd(FilterOperators.LessThanOrEqual, WriteStandardFilter);

        _filterWriters.TryAdd(FilterOperators.IsNull, WriteNullFilter);
        _filterWriters.TryAdd(FilterOperators.IsNotNull, WriteNullFilter);
    }

    private readonly IReadOnlyList<string> _sourceTables;

    /// <summary>
    /// Initializes a new instance of the <see cref="KqlQueryBuilder"/> class.
    /// </summary>
    /// <param name="sourceTables">The telemetry source tables to union for query data.</param>
    public KqlQueryBuilder(IEnumerable<string> sourceTables)
    {
        ArgumentNullException.ThrowIfNull(sourceTables);

        var tables = sourceTables.ToArray();
        if (tables.Length == 0)
            throw new ArgumentException("At least one source table is required.", nameof(sourceTables));

        if (tables.Any(table => !HasValidPropertyName(table)))
            throw new ArgumentException("Source table names must be valid KQL identifiers.", nameof(sourceTables));

        _sourceTables = tables;
    }

    /// <summary>
    /// Builds a KQL query from the specified filter and paging settings.
    /// </summary>
    /// <param name="filter">The optional entity filter to apply.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="cursorTime">The optional pagination cursor timestamp.</param>
    /// <returns>The generated KQL query.</returns>
    public string Build(EntityFilter? filter, int pageSize = 50, DateTimeOffset? cursorTime = null)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be positive.");

        return StringBuilder.Pool.Use(builder =>
        {
            builder.Append("union ");
            for (var index = 0; index < _sourceTables.Count; index++)
            {
                if (index > 0)
                    builder.Append(", ");

                builder.Append(_sourceTables[index]);
            }

            if (cursorTime.HasValue)
            {
                builder.AppendLine();
                builder.Append("| where TimeGenerated <= datetime(");
                builder.Append(cursorTime.Value.ToString("O", CultureInfo.InvariantCulture));
                builder.Append(')');
            }

            if (IsWritableFilter(filter))
            {
                builder.AppendLine();
                builder.Append("| where ");

                Visit(builder, filter);
            }

            builder.AppendLine();
            builder.Append("| top ");
            builder.Append((pageSize + 1).ToString(CultureInfo.InvariantCulture));
            builder.Append(" by TimeGenerated desc");

            return builder.ToString();
        });
    }


    private static void Visit(StringBuilder builder, EntityFilter? entityFilter)
    {
        if (!IsWritableFilter(entityFilter))
            return;

        if (entityFilter.IsGroup())
            WriteGroup(builder, entityFilter);
        else if (entityFilter is EntityFilter filter)
            WriteExpression(builder, filter);
    }

    private static void WriteGroup(StringBuilder builder, EntityFilter entityFilter)
    {
        if (!IsWritableFilter(entityFilter))
            return;

        var filters = entityFilter.Filters?
            .Where(IsWritableFilter)
            .ToArray();

        if (filters == null || filters.Length == 0)
            return;

        if (filters.Length == 1)
        {
            Visit(builder, filters[0]);
            return;
        }

        var wroteFirst = false;

        builder.Append('(');
        foreach (var filter in filters)
        {
            if (!filter.IsValid())
                continue;

            if (wroteFirst)
            {
                if (entityFilter.Logic == FilterLogic.Or)
                    builder.Append(" or ");
                else
                    builder.Append(" and ");
            }

            Visit(builder, filter);
            wroteFirst = true;
        }
        builder.Append(')');
    }

    private static void WriteExpression(StringBuilder builder, EntityFilter filter)
    {
        // Field required for expression
        if (!IsWritableFilter(filter))
            return;

        // default comparison equal
        var comparison = filter.Operator ?? FilterOperators.Equal;

        if (_filterWriters.TryGetValue(comparison, out var action))
            action(builder, filter);
        else
            WriteStandardFilter(builder, filter);
    }


    private static void WriteStringFilter(StringBuilder builder, EntityFilter filter)
    {
        // Field required for expression
        if (!HasValidPropertyName(filter.Name))
            return;

        WriteNameExpression(builder, filter);

        switch (filter.Operator)
        {
            case FilterOperators.Contains:
                builder.Append(" has ");
                break;
            case FilterOperators.NotContains:
                builder.Append(" !has ");
                break;
            case FilterOperators.StartsWith:
                builder.Append(" startswith ");
                break;
            case FilterOperators.NotStartsWith:
                builder.Append(" !startswith ");
                break;
            case FilterOperators.EndsWith:
                builder.Append(" endswith ");
                break;
            case FilterOperators.NotEndsWith:
                builder.Append(" !endswith ");
                break;
            default:
                builder.Append(" == ");
                break;
        }

        WriteValueExpression(builder, filter.Value);
    }

    private static void WriteStandardFilter(StringBuilder builder, EntityFilter filter)
    {
        // Field required for expression
        if (!HasValidPropertyName(filter.Name))
            return;

        WriteNameExpression(builder, filter);

        switch (filter.Operator)
        {
            case FilterOperators.Equal:
                builder.Append(" == ");
                break;
            case FilterOperators.NotEqual:
                builder.Append(" != ");
                break;
            case FilterOperators.GreaterThan:
                builder.Append(" > ");
                break;
            case FilterOperators.GreaterThanOrEqual:
                builder.Append(" >= ");
                break;
            case FilterOperators.LessThan:
                builder.Append(" < ");
                break;
            case FilterOperators.LessThanOrEqual:
                builder.Append(" <= ");
                break;
            default:
                builder.Append(" == ");
                break;
        }

        WriteValueExpression(builder, filter.Value);
    }

    private static void WriteNullFilter(StringBuilder builder, EntityFilter filter)
    {
        // Field required for expression
        if (!HasValidPropertyName(filter.Name))
            return;

        if (filter.Operator == FilterOperators.IsNull)
            builder.Append("isnull");
        else if (filter.Operator == FilterOperators.IsNotNull)
            builder.Append("isnotnull");
        else
            return;

        builder.Append('(');
        WriteNameExpression(builder, filter);
        builder.Append(')');
    }

    private static void WriteNameExpression(StringBuilder builder, EntityFilter filter)
    {
        var requreString = RequiresStringExpression(filter);
        if (requreString)
            builder.Append("tostring(");

        builder.Append(filter.Name);

        if (filter.Key.HasValue())
        {
            var key = EscapeIndexerKey(filter.Key);

            builder.Append("[\"");
            builder.Append(key);
            builder.Append("\"]");
        }

        if (requreString)
            builder.Append(')');
    }

    private static void WriteValueExpression(StringBuilder builder, object? value)
    {
        if (value == null)
            return;

        switch (value)
        {
            case string stringValue:
                var escapedValue = EscapeStringValue(stringValue);
                builder.Append('\'').Append(escapedValue).Append('\'');
                break;
            case char charValue:
                var escapedCharValue = EscapeStringValue(charValue.ToString());
                builder.Append('\'').Append(escapedCharValue).Append('\'');
                break;
            case byte or sbyte or short or ushort or int or uint or long or ulong or decimal:
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                break;
            case float floatValue:
                builder.Append(floatValue.ToString("R", CultureInfo.InvariantCulture));
                break;
            case double doubleValue:
                builder.Append(doubleValue.ToString("R", CultureInfo.InvariantCulture));
                break;
            case bool boolValue:
                builder.Append(boolValue ? "true" : "false");
                break;
            case DateTimeOffset dateTimeValue:
                builder.Append("datetime(").Append(dateTimeValue.ToString("O", CultureInfo.InvariantCulture)).Append(')');
                break;
            case DateTime dateValue:
                builder.Append("datetime(").Append(dateValue.ToString("O", CultureInfo.InvariantCulture)).Append(')');
                break;
            case Guid guidValue:
                builder.Append("guid(").Append(guidValue.ToString("D", CultureInfo.InvariantCulture)).Append(')');
                break;
            case TimeSpan timeSpanValue:
                builder.Append("time(").Append(timeSpanValue.ToString("c", CultureInfo.InvariantCulture)).Append(')');
                break;
            default:
                var defaultValue = EscapeStringValue(value.ToString());
                builder.Append('\'').Append(defaultValue).Append('\'');
                break;
        }
    }


    private static string? EscapeStringValue(string? value)
    {
        return value?
            .Replace("'", "''", StringComparison.Ordinal);
    }

    private static string? EscapeIndexerKey(string? value)
    {
        return value?
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static bool HasValidPropertyName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || !char.IsLetter(name[0]))
            return false;

        for (var index = 1; index < name.Length; index++)
        {
            var character = name[index];

            if (!char.IsLetterOrDigit(character) && character != '_')
                return false;
        }

        return true;
    }


    private static bool IsWritableFilter([NotNullWhen(true)] EntityFilter? filter)
    {
        if (filter?.IsValid() != true)
            return false;

        if (filter.IsGroup())
            return filter.Filters?.Any(IsWritableFilter) == true;

        return HasValidPropertyName(filter.Name);
    }

    private static bool RequiresStringExpression(EntityFilter filter)
    {
        return filter.Value is string or char || filter.Operator switch
        {
            FilterOperators.Contains
                or FilterOperators.NotContains
                or FilterOperators.StartsWith
                or FilterOperators.NotStartsWith
                or FilterOperators.EndsWith
                or FilterOperators.NotEndsWith => true,
            _ => false,
        };
    }
}
