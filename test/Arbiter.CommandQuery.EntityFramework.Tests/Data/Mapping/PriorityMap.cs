using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping;

public partial class PriorityMap
    : IEntityTypeConfiguration<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Priority>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Priority> builder)
    {
        #region Generated Configure
        // table
        builder.ToTable("Priority", "dbo");

        // key
        builder.HasKey(t => t.Id);

        // properties
        builder.Property(t => t.Id)
            .IsRequired()
            .HasColumnName("Id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasColumnName("Name")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasColumnType("nvarchar(255)")
            .HasMaxLength(255);

        builder.Property(t => t.DisplayOrder)
            .IsRequired()
            .HasColumnName("DisplayOrder")
            .HasColumnType("int")
            .HasDefaultValue(0);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValue(true);

        builder.Property(t => t.Created)
            .IsRequired()
            .HasColumnName("Created")
            .HasColumnType("datetimeoffset")
            .HasDefaultValueSql("(sysutcdatetime())");

        builder.Property(t => t.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100);

        builder.Property(t => t.Updated)
            .IsRequired()
            .HasColumnName("Updated")
            .HasColumnType("datetimeoffset")
            .HasDefaultValueSql("(sysutcdatetime())");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("UpdatedBy")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100);

        builder.Property(t => t.RowVersion)
            .IsRequired()
            .HasConversion<byte[]>()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnName("RowVersion")
            .HasColumnType("rowversion")
            .ValueGeneratedOnAddOrUpdate();

        // relationships
        #endregion
    }

    #region Generated Constants
    public readonly struct Table
    {
        public const string Schema = "dbo";
        public const string Name = "Priority";
    }

    public readonly struct Columns
    {
        public const string Id = "Id";
        public const string Name = "Name";
        public const string Description = "Description";
        public const string DisplayOrder = "DisplayOrder";
        public const string IsActive = "IsActive";
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string Updated = "Updated";
        public const string UpdatedBy = "UpdatedBy";
        public const string RowVersion = "RowVersion";
    }
    #endregion
}
