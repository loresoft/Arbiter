using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class AuditReadModel
    : Arbiter.CommandQuery.Models.EntityCreateModel<int>
{
    #region Generated Properties
    public DateTime Date { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string Content { get; set; } = null!;

    public string Username { get; set; } = null!;

    #endregion

}
