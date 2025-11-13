namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TaskReadModel
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated, ITrackConcurrency, IHaveTenant<int>
{
    #region Generated Properties
    public int Id { get; set; }

    public Guid Key { get; set; }

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

    public string StatusName { get; set; } = null!;

    public string? PriorityName { get; set; }

    public string? AssignedName { get; set; }

    public string TenantName { get; set; } = null!;
}
