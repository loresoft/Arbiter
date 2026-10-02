using Arbiter.Services;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Arbiter.CommandQuery.EntityFramework.ValueGeneration;

/// <summary>
/// An Entity Framework Core <see cref="ValueGenerator{TValue}"/> that generates time-ordered
/// <see cref="long"/> keys using a <see cref="Snowflake"/> id generator.
/// </summary>
/// <remarks>
/// Generated values are permanent (not temporary) and sort chronologically, making them well suited
/// to clustered database keys.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SnowflakeGenerator"/> class.
/// </remarks>
/// <param name="snowflake">
/// The <see cref="Snowflake"/> generator used to create ids. When <see langword="null"/>,
/// <see cref="Snowflake.Default"/> is used.
/// </param>
public class SnowflakeGenerator(Snowflake? snowflake = null) : ValueGenerator<long>
{
    private readonly Snowflake _snowflake = snowflake ?? Snowflake.Default;

    /// <inheritdoc/>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Generates the next Snowflake id for the specified entity entry.
    /// </summary>
    /// <param name="entry">The entity entry the value is being generated for.</param>
    /// <returns>A positive, time-ordered 63-bit id.</returns>
    public override long Next(EntityEntry entry) => _snowflake.NextId();
}
