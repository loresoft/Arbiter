using System;
using System.Collections.Generic;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class AuditReadModel
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated, ITrackConcurrency
{
    #region Generated Properties
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string Content { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

}
