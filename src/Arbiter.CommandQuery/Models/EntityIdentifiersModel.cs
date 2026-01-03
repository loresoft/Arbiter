using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// An identifiers base model<see langword="class"/>
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
[MessagePackObject]
public partial class EntityIdentifiersModel<TKey>
{
    /// <summary>
    /// Gets or sets the list of identifiers.
    /// </summary>
    /// <value>
    /// The list of identifiers.
    /// </value>
    [Key(0)]
    [NotNull]
    [JsonPropertyName("ids")]
    public IReadOnlyCollection<TKey> Ids { get; set; } = null!;
}
