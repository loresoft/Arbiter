using Arbiter.Components.Services;

namespace Arbiter.Components.Tests.Services;

/// <summary>
/// An <see cref="IPermissionResolver"/> that returns a configured result and records the requests it receives.
/// </summary>
internal sealed class FakePermissionResolver : IPermissionResolver
{
    public Task<bool> Result { get; set; } = Task.FromResult(true);

    public List<(string Action, string Subject, string? Field)> Requests { get; } = [];

    public ValueTask<bool> HasPermission(string action, string subject, string? field)
    {
        Requests.Add((action, subject, field));
        return new ValueTask<bool>(Result);
    }
}
