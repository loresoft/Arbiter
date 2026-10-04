using Arbiter.Services;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.ValueGeneration;

/// <summary>
/// An Entity Framework Core <see cref="ValueGenerator{TValue}"/> that generates time-ordered
/// <see cref="long"/> keys using a <see cref="Snowflake"/> id generator.
/// </summary>
/// <remarks>
/// Generated values are permanent (not temporary) and sort chronologically, making them well suited
/// to clustered database keys.
/// </remarks>
public class SnowflakeGenerator : ValueGenerator<long>
{
    private Snowflake? _snowflake;

    /// <inheritdoc/>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Generates the next Snowflake id for the specified entity entry.
    /// </summary>
    /// <param name="entry">The entity entry the value is being generated for.</param>
    /// <returns>A positive, time-ordered 63-bit id.</returns>
    /// <remarks>
    /// Uses the <see cref="Snowflake"/> registered in the application service provider when available,
    /// otherwise <see cref="Snowflake.Default"/>. The resolved instance is cached after first use.
    /// </remarks>
    public override long Next(EntityEntry entry)
    {
        var snowflake = Volatile.Read(ref _snowflake);
        if (snowflake == null)
        {
            snowflake = ResolveSnowflake(entry);
            Volatile.Write(ref _snowflake, snowflake);
        }

        return snowflake.NextId();
    }

    private static Snowflake ResolveSnowflake(EntityEntry entry)
    {
        var options = entry.Context.GetService<IDbContextOptions>();
        var serviceProvider = options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;

        return serviceProvider?.GetService<Snowflake>() ?? Snowflake.Default;
    }
}
