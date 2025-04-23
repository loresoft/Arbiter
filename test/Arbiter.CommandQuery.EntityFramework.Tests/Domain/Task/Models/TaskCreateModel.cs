using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TaskCreateModel
    : Arbiter.CommandQuery.Models.EntityReadModel<int>, IHaveTenant<int>
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

}
