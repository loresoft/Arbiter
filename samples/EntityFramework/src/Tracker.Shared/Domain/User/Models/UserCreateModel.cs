using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class UserCreateModel
    : EntityCreateModel
{
    #region Generated Properties
    [Required]
    public string DisplayName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = null!;

    public bool IsDeleted { get; set; }

    #endregion

}
