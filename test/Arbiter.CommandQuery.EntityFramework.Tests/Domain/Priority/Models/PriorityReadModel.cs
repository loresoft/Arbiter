namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

public partial class PriorityReadModel
    : Arbiter.CommandQuery.Models.EntityCreateModel<int>, ISupportSearch
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    #endregion

    public static IEnumerable<string> SearchFields() => [nameof(Name), nameof(Description)];

    public static string SortField() => nameof(DisplayOrder);
}
