namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class Task
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated, IHaveTenant<int>
{
    public Task()
    {
        #region Generated Constructor
        #endregion
    }

    #region Generated Properties
    public int Id { get; set; }

    public int StatusId { get; set; }

    public int? PriorityId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompleteDate { get; set; }

    public int? AssignedId { get; set; }

    public int TenantId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    public virtual User? AssignedUser { get; set; }

    public virtual Priority? Priority { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual TaskExtended TaskExtended { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;

    #endregion

}
