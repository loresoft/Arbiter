using System;
using System.Collections.Generic;

namespace Tracker.Domain.Models;

[Equatable]
public partial class TenantUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    #endregion

}
