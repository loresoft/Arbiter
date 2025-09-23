using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping;

public partial class UserLoginMap
    : IEntityTypeConfiguration<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserLogin>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserLogin> builder)
    {
        #region Generated Configure
        // table
        builder.ToTable("UserLogin", "dbo");

        // key
        builder.HasKey(t => t.Id);

        // properties
        builder.Property(t => t.Id)
            .IsRequired()
            .HasColumnName("Id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.EmailAddress)
            .IsRequired()
            .HasColumnName("EmailAddress")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.UserId)
            .HasColumnName("UserId")
            .HasColumnType("int");

        builder.Property(t => t.UserAgent)
            .HasColumnName("UserAgent")
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Browser)
            .HasColumnName("Browser")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.OperatingSystem)
            .HasColumnName("OperatingSystem")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.DeviceFamily)
            .HasColumnName("DeviceFamily")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.DeviceBrand)
            .HasColumnName("DeviceBrand")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.DeviceModel)
            .HasColumnName("DeviceModel")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

        builder.Property(t => t.IpAddress)
            .HasColumnName("IpAddress")
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50);

        builder.Property(t => t.IsSuccessful)
            .IsRequired()
            .HasColumnName("IsSuccessful")
            .HasColumnType("bit")
            .HasDefaultValue(false);

        builder.Property(t => t.FailureMessage)
            .HasColumnName("FailureMessage")
            .HasColumnType("nvarchar(256)")
            .HasMaxLength(256);

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
        builder.HasOne(t => t.User)
            .WithMany(t => t.UserLogins)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_UserLogin_User_UserId");

        #endregion
    }

    #region Generated Constants
    public readonly struct Table
    {
        public const string Schema = "dbo";
        public const string Name = "UserLogin";
    }

    public readonly struct Columns
    {
        public const string Id = "Id";
        public const string EmailAddress = "EmailAddress";
        public const string UserId = "UserId";
        public const string UserAgent = "UserAgent";
        public const string Browser = "Browser";
        public const string OperatingSystem = "OperatingSystem";
        public const string DeviceFamily = "DeviceFamily";
        public const string DeviceBrand = "DeviceBrand";
        public const string DeviceModel = "DeviceModel";
        public const string IpAddress = "IpAddress";
        public const string IsSuccessful = "IsSuccessful";
        public const string FailureMessage = "FailureMessage";
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string Updated = "Updated";
        public const string UpdatedBy = "UpdatedBy";
        public const string RowVersion = "RowVersion";
    }
    #endregion
}
