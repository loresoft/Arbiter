using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TaskReadModel
    : Arbiter.CommandQuery.Models.EntityCreateModel<int>, IHaveTenant<int>
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

    #endregion

    public string StatusName { get; set; } = null!;

    public string? PriorityName { get; set; }

    public string? AssignedName { get; set; }

    public string TenantName { get; set; } = null!;
}
