using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class TenantReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    [Required]
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
