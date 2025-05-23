using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// An identifiers base model<see langword="class"/>
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class EntityIdentifiersModel<TKey>
{
    /// <summary>
    /// Gets or sets the list of identifiers.
    /// </summary>
    /// <value>
    /// The list of identifiers.
    /// </value>
    [NotNull]
    [JsonPropertyName("ids")]
    public IReadOnlyCollection<TKey> Ids { get; set; } = null!;
}
