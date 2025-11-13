namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class PriorityReadModel
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated, ITrackConcurrency, ISupportSearch
{
    #region Generated Properties
    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    public static IEnumerable<string> SearchFields() => [nameof(Name), nameof(Description)];

    public static string SortField() => nameof(DisplayOrder);
}
