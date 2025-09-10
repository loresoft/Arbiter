using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class TenantCreateModel
    : EntityCreateModel
{
    #region Generated Properties
    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    #endregion

}
