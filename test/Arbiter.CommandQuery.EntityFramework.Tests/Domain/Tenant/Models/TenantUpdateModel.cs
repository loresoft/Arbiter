namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TenantUpdateModel
    : ITrackUpdated, ITrackConcurrency
{
    #region Generated Properties
    public Guid Key { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

}
