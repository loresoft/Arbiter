using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data;

public partial class TrackerContext : DbContext
{
    public TrackerContext(DbContextOptions<TrackerContext> options)
        : base(options)
    {
    }

    #region Generated Properties
    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Audit> Audits { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Priority> Priorities { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Role> Roles { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Status> Statuses { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.TaskExtended> TaskExtendeds { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Task> Tasks { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Tenant> Tenants { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserLogin> UserLogins { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.UserRole> UserRoles { get; set; } = null!;

    public virtual DbSet<Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.User> Users { get; set; } = null!;

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Generated Configuration
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.AuditMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.PriorityMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.RoleMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.StatusMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.TaskExtendedMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.TaskMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.TenantMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.UserLoginMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.UserMap());
        modelBuilder.ApplyConfiguration(new Arbiter.CommandQuery.EntityFramework.Tests.Data.Mapping.UserRoleMap());
        #endregion
    }
}
