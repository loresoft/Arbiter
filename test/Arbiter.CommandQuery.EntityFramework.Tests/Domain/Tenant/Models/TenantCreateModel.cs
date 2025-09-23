namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class TenantCreateModel
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated
{
    #region Generated Properties
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    #endregion

}
