namespace Arbiter.Components.Services;

/// <summary>
/// The default <see cref="IPermissionResolver"/> that grants permission for every request.
/// </summary>
/// <remarks>
/// Registered by <see cref="ComponentServiceExtensions.AddArbiterComponents"/> so components always have a resolver to
/// inject. Replace it by calling <see cref="ComponentServiceExtensions.AddPermissionResolver{TResolver}"/>.
/// </remarks>
public sealed class DefaultPermissionResolver : IPermissionResolver
{
    /// <inheritdoc />
    /// <returns>A completed task that resolves to <see langword="true"/>.</returns>
    public ValueTask<bool> HasPermission(string action, string subject, string? field)
        => ValueTask.FromResult(true);
}
