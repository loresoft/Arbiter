using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping;

public partial class UserRoleMap
    : IEntityTypeConfiguration<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserRole>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserRole> builder)
    {
        #region Generated Configure
        // table
        builder.ToTable("UserRole", "dbo");

        // key
        builder.HasKey(t => new { t.UserId, t.RoleId });

        // properties
        builder.Property(t => t.UserId)
            .IsRequired()
            .HasColumnName("UserId")
            .HasColumnType("int");

        builder.Property(t => t.RoleId)
            .IsRequired()
            .HasColumnName("RoleId")
            .HasColumnType("int");

        // relationships
        builder.HasOne(t => t.Role)
            .WithMany(t => t.UserRoles)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("FK_UserRole_Role_RoleId");

        builder.HasOne(t => t.User)
            .WithMany(t => t.UserRoles)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_UserRole_User_UserId");

        #endregion
    }

    #region Generated Constants
    public readonly struct Table
    {
        public const string Schema = "dbo";
        public const string Name = "UserRole";
    }

    public readonly struct Columns
    {
        public const string UserId = "UserId";
        public const string RoleId = "RoleId";
    }
    #endregion
}
