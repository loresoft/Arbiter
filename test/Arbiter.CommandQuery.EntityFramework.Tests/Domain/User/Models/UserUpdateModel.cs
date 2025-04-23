using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class UserUpdateModel
    : Arbiter.CommandQuery.Models.EntityUpdateModel
{
    #region Generated Properties
    public string EmailAddress { get; set; } = null!;

    public bool IsEmailAddressConfirmed { get; set; }

    public string DisplayName { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? ResetHash { get; set; }

    public string? InviteHash { get; set; }

    public int AccessFailedCount { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

    public bool IsDeleted { get; set; }

    #endregion

}
