using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

using Arbiter.CommandQuery.Definitions;

namespace Tracker.Domain.Models;

[Equatable]
public partial class TaskReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompleteDate { get; set; }

    public bool IsDeleted { get; set; }

    public int TenantId { get; set; }

    public int StatusId { get; set; }

    public int? PriorityId { get; set; }

    public int? AssignedId { get; set; }

    #endregion

    [Column("Tenant.Name")]
    public string? TenantName { get; set; }

    [Column("Status.Name")]
    public string? StatusName { get; set; }

    [Column("Priority.Name")]
    public string? PriorityName { get; set; }

    public override string ToString()
        => Title;

    public static IEnumerable<string> SearchFields()
        => [nameof(Title), nameof(Description)];

    public static string SortField()
        => nameof(Title);
}
