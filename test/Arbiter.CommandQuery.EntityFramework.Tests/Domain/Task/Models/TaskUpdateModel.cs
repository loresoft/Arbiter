namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TaskUpdateModel
    : ITrackUpdated, ITrackConcurrency, IHaveTenant<int>
{
    #region Generated Properties
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

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

}
