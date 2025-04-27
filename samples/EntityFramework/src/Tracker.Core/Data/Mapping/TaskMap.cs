using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tracker.Data.Mapping;

public partial class TaskMap
    : IEntityTypeConfiguration<Tracker.Data.Entities.Task>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Tracker.Data.Entities.Task> builder)
    {
        #region Generated Configure
        // table
        builder.ToTable("Task", "dbo");

        // key
        builder.HasKey(t => t.Id);

        // properties
        builder.Property(t => t.Id)
            .IsRequired()
            .HasColumnName("Id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasColumnName("Title")
            .HasColumnType("nvarchar(255)")
            .HasMaxLength(255);

        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.DueDate)
            .HasColumnName("DueDate")
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.CompleteDate)
            .HasColumnName("CompleteDate")
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasColumnName("IsDeleted")
            .HasColumnType("bit")
            .HasDefaultValue(false);

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasColumnName("TenantId")
            .HasColumnType("int");

        builder.Property(t => t.StatusId)
            .IsRequired()
            .HasColumnName("StatusId")
            .HasColumnType("int");

        builder.Property(t => t.PriorityId)
            .HasColumnName("PriorityId")
            .HasColumnType("int");

        builder.Property(t => t.AssignedId)
            .HasColumnName("AssignedId")
            .HasColumnType("int");

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
        builder.HasOne(t => t.Priority)
            .WithMany(t => t.Tasks)
            .HasForeignKey(d => d.PriorityId)
            .HasConstraintName("FK_Task_Priority_PriorityId");

        builder.HasOne(t => t.Status)
            .WithMany(t => t.Tasks)
            .HasForeignKey(d => d.StatusId)
            .HasConstraintName("FK_Task_Status_StatusId");

        builder.HasOne(t => t.Tenant)
            .WithMany(t => t.Tasks)
            .HasForeignKey(d => d.TenantId)
            .HasConstraintName("FK_Task_Tenant_TenantId");

        builder.HasOne(t => t.AssignedUser)
            .WithMany(t => t.AssignedTasks)
            .HasForeignKey(d => d.AssignedId)
            .HasConstraintName("FK_Task_User_AssignedId");

        #endregion
    }

    #region Generated Constants
    public readonly struct Table
    {
        public const string Schema = "dbo";
        public const string Name = "Task";
    }

    public readonly struct Columns
    {
        public const string Id = "Id";
        public const string Title = "Title";
        public const string Description = "Description";
        public const string StartDate = "StartDate";
        public const string DueDate = "DueDate";
        public const string CompleteDate = "CompleteDate";
        public const string IsDeleted = "IsDeleted";
        public const string TenantId = "TenantId";
        public const string StatusId = "StatusId";
        public const string PriorityId = "PriorityId";
        public const string AssignedId = "AssignedId";
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string Updated = "Updated";
        public const string UpdatedBy = "UpdatedBy";
        public const string RowVersion = "RowVersion";
    }
    #endregion
}
