using System;
using System.Collections.Generic;

using MessagePack;

namespace Tracker.Domain.Models;

[Equatable]
[MessagePackObject(true)]
public partial class UserCreateModel
    : EntityCreateModel
{
    #region Generated Properties
    public string DisplayName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public bool IsDeleted { get; set; }

    #endregion

}
