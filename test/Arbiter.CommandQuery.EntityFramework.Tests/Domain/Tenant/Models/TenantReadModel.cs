using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TenantReadModel
    : Arbiter.CommandQuery.Models.EntityCreateModel<int>
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    #endregion

}
