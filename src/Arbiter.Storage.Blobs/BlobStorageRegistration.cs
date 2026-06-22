using Azure.Storage.Blobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Storage.Blobs;

internal sealed record BlobStorageRegistration(
    object? ServiceKey,
    string OptionsName
);
