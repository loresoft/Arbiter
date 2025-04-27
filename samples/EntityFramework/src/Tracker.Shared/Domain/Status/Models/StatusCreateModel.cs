using System;
using System.Collections.Generic;

namespace Tracker.Domain.Models;

[Equatable]
public partial class StatusCreateModel
    : EntityCreateModel
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    #endregion

}
