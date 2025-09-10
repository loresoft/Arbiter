using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class TaskUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    [Required]
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
