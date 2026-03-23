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
}
