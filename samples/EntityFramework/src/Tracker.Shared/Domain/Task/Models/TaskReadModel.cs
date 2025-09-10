using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class TaskReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    [Required]
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
