using Azure.Core;
using Azure.Storage.Blobs.Models;

namespace Arbiter.Storage.Blobs;

/// <summary>
/// Represents configuration options for Azure Blob Storage clients and containers.
/// </summary>
public class BlobStorageOptions
{
    /// <summary>
    /// Gets or sets the dependency injection key used to register the Blob Storage client.
    /// </summary>
    public object? ServiceKey { get; set; }

    /// <summary>
    /// Gets or sets a Blob Storage connection string, service URI, connection string name, or configuration key.
    /// When <see cref="Credential" /> is provided, this value is the Blob Storage service URI.
    /// </summary>
    public string NameOrConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="TokenCredential" /> used for identity-based authentication.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the suffix appended to container names when formatting container names.
    /// </summary>
    public string? NameSuffix { get; set; }

    /// <summary>
    /// Gets or sets the containers to initialize and register clients for, keyed by container name
    /// with the public access level applied during initialization.
    /// </summary>
    public IDictionary<string, PublicAccessType> Containers { get; set; }
        = new Dictionary<string, PublicAccessType>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets the suffix appended to container names when formatting container names.
    /// </summary>
    /// <param name="nameSuffix">The name suffix to append.</param>
    /// <returns>The current <see cref="BlobStorageOptions" /> instance for chaining.</returns>
    public BlobStorageOptions WithNameSuffix(string? nameSuffix)
    {
        NameSuffix = nameSuffix;
        return this;
    }

    /// <summary>
    /// Adds a container to initialize and register a client for.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns>The current <see cref="BlobStorageOptions" /> instance for chaining.</returns>
    public BlobStorageOptions AddContainer(string containerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        Containers[containerName] = PublicAccessType.None;
        return this;
    }

    /// <summary>
    /// Adds a container to initialize and register a client for, with a public access level.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="publicAccessType">The public access level to apply when the container is created.</param>
    /// <returns>The current <see cref="BlobStorageOptions" /> instance for chaining.</returns>
    public BlobStorageOptions AddContainer(string containerName, PublicAccessType publicAccessType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        Containers[containerName] = publicAccessType;

        return this;
    }

    /// <summary>
    /// Formats a container name by appending the configured suffix when one is provided.
    /// </summary>
    /// <param name="baseName">The base container name.</param>
    /// <returns>The formatted container name.</returns>
    public string FormatName(string baseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        return string.IsNullOrWhiteSpace(NameSuffix) ? baseName : $"{baseName}-{NameSuffix}";
    }

}
