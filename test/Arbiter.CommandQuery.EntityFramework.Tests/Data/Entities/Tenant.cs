namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class Tenant
    : IHaveIdentifier<int>, IHaveKey, ITrackCreated, ITrackUpdated
{
    public Tenant()
    {
        #region Generated Constructor
        Tasks = new HashSet<Task>();
        #endregion
    }

    #region Generated Properties
    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    public virtual ICollection<Task> Tasks { get; set; }

    #endregion

}
