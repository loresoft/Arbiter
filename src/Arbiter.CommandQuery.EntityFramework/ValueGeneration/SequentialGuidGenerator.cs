using Arbiter.CommandQuery.Extensions;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Arbiter.CommandQuery.EntityFramework.ValueGeneration;

/// <summary>
/// Generates sequential <see cref="Guid"/> values reordered for optimal SQL Server index performance.
/// </summary>
public class SequentialGuidGenerator : ValueGenerator<Guid>
{
    /// <inheritdoc/>
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc/>
    public override Guid Next(EntityEntry entry) => Guid.NewSqlGuid();
}
