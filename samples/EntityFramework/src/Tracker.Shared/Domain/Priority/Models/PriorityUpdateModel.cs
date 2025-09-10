using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class PriorityUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    #endregion

}
