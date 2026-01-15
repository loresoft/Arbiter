using System;
using System.Collections.Generic;

using MessagePack;

namespace Tracker.Domain.Models;

[Equatable]
[MessagePackObject(true)]
public partial class TaskCreateModel
    : EntityCreateModel
{
    #region Generated Properties
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompleteDate { get; set; }

    public bool IsDeleted { get; set; }

    public int TenantId { get; set; }

    public int StatusId { get; set; }

    public int? PriorityId { get; set; }

    public int? AssignedId { get; set; }

    #endregion

}
