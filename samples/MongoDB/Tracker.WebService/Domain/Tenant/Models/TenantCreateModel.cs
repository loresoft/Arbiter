#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Tracker.WebService.Domain.Models;

public partial class TenantCreateModel
    : Arbiter.CommandQuery.Models.EntityCreateModel<string>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
