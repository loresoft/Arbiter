using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class UserRole
{
    public UserRole()
    {
        #region Generated Constructor
        #endregion
    }

    #region Generated Properties
    public int UserId { get; set; }

    public int RoleId { get; set; }

    #endregion

    #region Generated Relationships
    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    #endregion

}
