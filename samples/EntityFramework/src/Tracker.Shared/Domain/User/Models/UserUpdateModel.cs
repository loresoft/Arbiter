using System;
using System.Collections.Generic;

namespace Tracker.Domain.Models;

[Equatable]
public partial class UserUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    public string DisplayName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public bool IsDeleted { get; set; }

    #endregion

}
