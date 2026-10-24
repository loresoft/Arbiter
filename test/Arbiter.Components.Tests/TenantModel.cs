using Arbiter.CommandQuery.Definitions;

namespace Arbiter.Components.Tests;

/// <summary>
/// A searchable model identified by an integer key, used to test the entity input components.
/// </summary>
public class TenantModel : IHaveIdentifier<int>, ISupportSearch
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public static IEnumerable<string> SearchFields() => [nameof(Name)];

    public static string SortField() => nameof(Name);

    public override string ToString() => Name ?? base.ToString()!;
}

/// <summary>
/// Holds the bound value for the entity input component tests.
/// </summary>
public class TenantHolder
{
    public int TenantId { get; set; }
}

/// <summary>
/// An update model holding the bound value, used to test the default permission subject.
/// </summary>
public class TenantUpdateModel
{
    public int TenantId { get; set; }
}
