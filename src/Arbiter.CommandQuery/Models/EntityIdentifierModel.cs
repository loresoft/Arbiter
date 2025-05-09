using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// An identifier base model <see langword="class"/>
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <seealso cref="IHaveIdentifier{TKey}" />
public class EntityIdentifierModel<TKey> : IHaveIdentifier<TKey>
{
    /// <inheritdoc />
    [NotNull]
    [JsonPropertyName("id")]
    [JsonPropertyOrder(-9999)]
    public TKey Id { get; set; } = default!;
}
