using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Validation;

namespace Tracker.Domain.Models;

[Equatable, ValidatableType]
public partial class UserReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    [Required]
    public string DisplayName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = null!;

    public bool IsDeleted { get; set; }

    #endregion

    public override string ToString()
        => $"{DisplayName} <{EmailAddress}>";

    public static IEnumerable<string> SearchFields()
        => [nameof(DisplayName), nameof(EmailAddress)];

    public static string SortField()
        => nameof(DisplayName);

}
