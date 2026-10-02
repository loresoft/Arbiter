using Arbiter.CommandQuery.EntityFramework.ValueGeneration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arbiter.CommandQuery.EntityFramework;

/// <summary>
/// Provides extension methods for configuring EF Core model builders with Arbiter conventions.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures the property to use <see cref="SequentialGuidGenerator"/> for automatic
    /// key generation, producing sequential <see cref="Guid"/> values optimized for SQL Server index performance.
    /// </summary>
    /// <param name="builder">The property builder for a <see cref="Guid"/> property.</param>
    /// <returns>The same <paramref name="builder"/> so that additional calls can be chained.</returns>
    public static PropertyBuilder<Guid> UseSequentialGuid(this PropertyBuilder<Guid> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasValueGenerator<SequentialGuidGenerator>();
    }

    /// <summary>
    /// Configures the property to use <see cref="SnowflakeGenerator"/> for automatic
    /// key generation, producing time-ordered 63-bit <see cref="long"/> Snowflake ids well suited to clustered indexes.
    /// </summary>
    /// <remarks>
    /// Ids are generated using <see cref="Arbiter.Services.Snowflake.Default"/>, which uses a randomly assigned
    /// instance id. Uniqueness is only guaranteed per instance id.
    /// </remarks>
    /// <param name="builder">The property builder for a <see cref="long"/> property.</param>
    /// <returns>The same <paramref name="builder"/> so that additional calls can be chained.</returns>
    public static PropertyBuilder<long> UseSnowflake(this PropertyBuilder<long> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasValueGenerator<SnowflakeGenerator>();
    }
}
