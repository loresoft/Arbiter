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
    /// <para>
    /// Ids are generated client-side when an entity is added to the context. The generator uses the
    /// <see cref="Arbiter.Services.Snowflake"/> registered in the application service provider when one is available,
    /// otherwise it falls back to <see cref="Arbiter.Services.Snowflake.Default"/>. The resolved instance is cached
    /// after first use.
    /// </para>
    /// <para>
    /// Uniqueness is only guaranteed per instance id. <see cref="Arbiter.Services.Snowflake.Default"/> uses a randomly
    /// assigned instance id unless configured via <see cref="Arbiter.Services.Snowflake.Configure(Arbiter.Services.Snowflake)"/>,
    /// so register a <see cref="Arbiter.Services.Snowflake"/> with an explicit, coordinated instance id when running multiple nodes.
    /// </para>
    /// <para>
    /// The property is also configured with <c>ValueGeneratedNever</c> so EF Core treats the generated ids as permanent
    /// values and so migrations do not configure the column as a database-generated identity (auto-increment) column.
    /// </para>
    /// </remarks>
    /// <param name="builder">The property builder for a <see cref="long"/> property.</param>
    /// <returns>The same <paramref name="builder"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static PropertyBuilder<long> UseSnowflake(this PropertyBuilder<long> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // SnowflakeGenerator produces permanent client-side values. ValueGeneratedNever prevents EF Core from treating
        // them as temporary values and prevents migrations from configuring the key column as identity/auto-increment.
        return builder
            .HasValueGenerator<SnowflakeGenerator>()
            .ValueGeneratedNever();
    }
}
