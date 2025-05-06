#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

namespace Tracker.WebService.Domain.Models;

public partial class TaskUpdateModel
    : Arbiter.CommandQuery.Models.EntityUpdateModel, IHaveTenant<string>, ITrackDeleted
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
}
