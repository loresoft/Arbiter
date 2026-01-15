using System;
using System.Collections.Generic;

using MessagePack;

namespace Tracker.Domain.Models;

[Equatable]
[MessagePackObject(true)]
public partial class StatusUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    #endregion

}
