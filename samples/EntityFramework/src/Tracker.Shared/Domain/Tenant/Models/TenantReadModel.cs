using System;
using System.Collections.Generic;

using Arbiter.CommandQuery.Definitions;

namespace Tracker.Domain.Models;

[Equatable]
public partial class TenantReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    #endregion

    public override string ToString()
        => Name;

    public static IEnumerable<string> SearchFields()
        => [nameof(Name), nameof(Description)];

    public static string SortField()
        => nameof(Name);

}
