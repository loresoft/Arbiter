namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class Event
    : IHaveIdentifier<long>, ITrackCreated, ITrackUpdated
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }
}
