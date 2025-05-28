// Ignore Spelling: queryable

using System.Linq.Dynamic.Core;
using System.Text;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IQueryable{T}"/> to support dynamic sorting, filtering, and projection.
/// </summary>
/// <remarks>
/// These extensions are commonly used in data access layers to apply dynamic query operations such as sorting and filtering
/// based on user input or API parameters. They also support projecting query results to different types using a mapper.
/// </remarks>
public static class QueryExtensions
{
    /// <summary>
    /// Applies the specified <paramref name="sort"/> to the provided <paramref name="query"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data in the query.</typeparam>
    /// <param name="query">The query to apply the sort to.</param>
    /// <param name="sort">The sort expression to apply.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with the sort applied. If <paramref name="sort"/> is <c>null</c>, the original query is returned.
    /// </returns>
    /// <example>
    /// <code>
    /// var sorted = query.Sort(new EntitySort { Name = "Name", Direction = "asc" });
    /// </code>
    /// </example>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, EntitySort? sort)
    {
        if (sort == null)
            return query;

        return query.Sort([sort]);
    }

    /// <summary>
    /// Applies the specified <paramref name="sorts"/> to the provided <paramref name="query"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data in the query.</typeparam>
    /// <param name="query">The query to apply the sorts to.</param>
    /// <param name="sorts">A collection of sort expressions to apply.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with the sorts applied. If <paramref name="sorts"/> is <c>null</c> or empty, the original query is returned.
    /// </returns>
    /// <example>
    /// <code>
    /// var sorts = new List&lt;EntitySort&gt;
    /// {
    ///     new EntitySort { Name = "Priority", Direction = "desc" },
    ///     new EntitySort { Name = "Name", Direction = "asc" }
    /// };
    /// var sorted = query.Sort(sorts);
    /// </code>
    /// </example>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, IEnumerable<EntitySort>? sorts)
    {
        if (sorts?.Any() != true)
            return query;

        ArgumentNullException.ThrowIfNull(query);

        // Create ordering expression e.g. Field1 asc, Field2 desc
        var builder = new StringBuilder();
        foreach (var sort in sorts)
        {
            if (builder.Length > 0)
                builder.Append(',');

            builder.Append(sort.Name).Append(' ');

            var isDescending = !string.IsNullOrWhiteSpace(sort.Direction)
                && sort.Direction.StartsWith(EntitySortDirections.Descending, StringComparison.OrdinalIgnoreCase);

            builder.Append(isDescending ? EntitySortDirections.Descending : EntitySortDirections.Ascending);
        }

        return query.OrderBy(builder.ToString());
    }

    /// <summary>
    /// Applies the specified <paramref name="filter"/> to the provided <paramref name="query"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data in the query.</typeparam>
    /// <param name="query">The query to apply the filter to.</param>
    /// <param name="filter">The filter expression to apply.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with the filter applied. If <paramref name="filter"/> is <c>null</c> or invalid, the original query is returned.
    /// </returns>
    /// <example>
    /// <code>
    /// var filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" };
    /// var filtered = query.Filter(filter);
    /// </code>
    /// </example>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, EntityFilter? filter)
    {
        if (filter is null)
            return query;

        ArgumentNullException.ThrowIfNull(query);

        var builder = new LinqExpressionBuilder();
        builder.Build(filter);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        // nothing to filter
        if (string.IsNullOrWhiteSpace(predicate))
            return query;

        var config = new ParsingConfig
        {
            UseParameterizedNamesInDynamicQuery = true,
        };

        return query.Where(config, predicate, parameters);
    }

    /// <summary>
    /// Projects the elements of an <see cref="IQueryable{TSource}"/> to a new <see cref="IQueryable{TDestination}"/> using the specified <see cref="IMapper"/>.
    /// </summary>
    /// <remarks>
    /// This method uses the provided <see cref="IMapper"/> to perform the projection, allowing for
    /// transformation of the source queryable's elements into a different type. The projection is performed in a way
    /// that supports deferred execution.
    /// </remarks>
    /// <typeparam name="TSource">The type of the source elements in the queryable.</typeparam>
    /// <typeparam name="TDestination">The type of the destination elements in the projected queryable.</typeparam>
    /// <param name="queryable">The source queryable to project.</param>
    /// <param name="mapper">The mapper used to define the projection rules.</param>
    /// <returns>
    /// An <see cref="IQueryable{TDestination}"/> containing the projected elements.
    /// </returns>
    /// <example>
    /// <code>
    /// var projected = query.ProjectTo&lt;SourceType, DestinationType&gt;(mapper);
    /// </code>
    /// </example>
    public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(this IQueryable<TSource> queryable, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(queryable);
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.ProjectTo<TSource, TDestination>(queryable);
    }
}
