using Arbiter.CommandQuery.Definitions;

using MongoDB.Abstracts;

namespace Tracker.WebService.Data.Entities;

public class Task : MongoEntity, IHaveIdentifier<string>, IHaveTenant<string>, ITrackCreated, ITrackUpdated
{
    public string? StatusId { get; set; }

    public string? PriorityId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompleteDate { get; set; }

    public string? AssignedId { get; set; }

    public string TenantId { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

}
