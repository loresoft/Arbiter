using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Tracker.Data;

public partial class TrackerContext : DbContext
{
    public TrackerContext(DbContextOptions<TrackerContext> options)
        : base(options)
    {
    }

    #region Generated Properties
    public virtual DbSet<Tracker.Data.Entities.Priority> Priorities { get; set; } = null!;

    public virtual DbSet<Tracker.Data.Entities.Status> Statuses { get; set; } = null!;

    public virtual DbSet<Tracker.Data.Entities.Task> Tasks { get; set; } = null!;

    public virtual DbSet<Tracker.Data.Entities.Tenant> Tenants { get; set; } = null!;

    public virtual DbSet<Tracker.Data.Entities.User> Users { get; set; } = null!;

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Generated Configuration
        modelBuilder.ApplyConfiguration(new Tracker.Data.Mapping.PriorityMap());
        modelBuilder.ApplyConfiguration(new Tracker.Data.Mapping.StatusMap());
        modelBuilder.ApplyConfiguration(new Tracker.Data.Mapping.TaskMap());
        modelBuilder.ApplyConfiguration(new Tracker.Data.Mapping.TenantMap());
        modelBuilder.ApplyConfiguration(new Tracker.Data.Mapping.UserMap());
        #endregion
    }
}
