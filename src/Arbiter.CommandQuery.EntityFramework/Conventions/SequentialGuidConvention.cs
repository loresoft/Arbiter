using Arbiter.CommandQuery.EntityFramework.ValueGeneration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Arbiter.CommandQuery.EntityFramework.Conventions;

/// <summary>
/// A model finalizing convention that configures all <see cref="Guid"/> primary key properties
/// to use <see cref="ValueGenerated.OnAdd"/> with the <see cref="SequentialGuidGenerator"/>.
/// </summary>
/// <remarks>
/// <para>
/// Register this convention by overriding
/// <see cref="DbContext.ConfigureConventions(ModelConfigurationBuilder)"/> in your <c>DbContext</c>:
/// </para>
/// <code>
/// protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
/// {
///     configurationBuilder.Conventions.Add(_ =&gt; new SequentialGuidConvention());
/// }
/// </code>
/// </remarks>
public class SequentialGuidConvention : IModelFinalizingConvention
{
    /// <inheritdoc/>
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is null)
                continue;

            foreach (var property in primaryKey.Properties)
            {
                if (property.ClrType != typeof(Guid))
                    continue;

                property.Builder.ValueGenerated(ValueGenerated.OnAdd);
                property.Builder.HasValueGenerator(typeof(SequentialGuidGenerator));
            }
        }
    }
}
