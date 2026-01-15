using System;
using System.Collections.Generic;

using MessagePack;

namespace Tracker.Domain.Models;

[Equatable]
[MessagePackObject(true)]
public partial class TenantUpdateModel
    : EntityUpdateModel
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    #endregion

}
