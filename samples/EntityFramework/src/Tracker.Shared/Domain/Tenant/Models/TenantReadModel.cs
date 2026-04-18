using System;
using System.Collections.Generic;

using Arbiter.CommandQuery.Definitions;

using MessagePack;

namespace Tracker.Domain.Models;

[Equatable]
[MessagePackObject(true)]
public partial class TenantReadModel
    : EntityReadModel, ISupportSearch
{
    #region Generated Properties
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    #endregion

    public int TaskCount { get; set; }

    [SequenceEquality]
    public List<string>? Tasks { get; set; }

    public override string ToString()
        => Name;

    public static IEnumerable<string> SearchFields()
        => [nameof(Name), nameof(Description)];

    public static string SortField()
        => nameof(Name);

}
