namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class RoleUpdateModel
    : ITrackUpdated, ITrackConcurrency
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

}
