using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tracker.Data.Mapping;

public partial class UserMap
    : IEntityTypeConfiguration<Tracker.Data.Entities.User>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Tracker.Data.Entities.User> builder)
    {
        #region Generated Configure
        // table
        builder.ToTable("User", "dbo");

        // key
        builder.HasKey(t => t.Id);

        // properties
        builder.Property(t => t.Id)
            .IsRequired()
            .HasColumnName("Id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.DisplayName)
            .IsRequired()
            .HasColumnName("DisplayName")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.EmailAddress)
            .IsRequired()
            .HasColumnName("EmailAddress")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasColumnName("IsDeleted")
            .HasColumnType("bit")
            .HasDefaultValue(false);

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
        public const string Name = "User";
    }

    public readonly struct Columns
    {
        public const string Id = "Id";
        public const string DisplayName = "DisplayName";
        public const string EmailAddress = "EmailAddress";
        public const string IsDeleted = "IsDeleted";
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string Updated = "Updated";
        public const string UpdatedBy = "UpdatedBy";
        public const string RowVersion = "RowVersion";
    }
    #endregion
}
