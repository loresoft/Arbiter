using System.Collections.Concurrent;
using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.Notifications;

using Microsoft.EntityFrameworkCore;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Pipeline;

public sealed record AuditModifierInvocation(
    string Modifier,
    string Username,
    EntityChangeOperation Operation,
    EntityState State,
    string? ActivatedBy,
    ClaimsPrincipal? Principal);

public sealed class AuditModifierRecorder
{
    public const string MarkerPrefix = "modifier-";
    public const string PipelineName = "audit-modifier-pipeline";
    public const string ChildSuffix = "-child";

    private readonly ConcurrentQueue<AuditModifierInvocation> _invocations = new();

    public static string CreateMarker() => $"{MarkerPrefix}{Guid.NewGuid():N}";

    public static bool IsMarked(Entities.Audit entity)
    {
        return entity.Username?.StartsWith(MarkerPrefix, StringComparison.Ordinal) == true
            && !entity.Username.EndsWith(ChildSuffix, StringComparison.Ordinal);
    }

    public void Record(
        string modifier,
        Entities.Audit entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal)
    {
        var state = context.Entry(entity).State;

        var invocation = new AuditModifierInvocation(
            Modifier: modifier,
            Username: entity.Username,
            Operation: operation,
            State: state,
            ActivatedBy: principal?.ActivatedBy,
            Principal: principal?.Principal);

        _invocations.Enqueue(invocation);
    }

    public IReadOnlyList<AuditModifierInvocation> GetInvocations(string username)
    {
        return _invocations
            .Where(i => i.Username == username)
            .ToList();
    }
}

// registered first, but runs after AuditFirstModifier because of a higher priority value
public sealed class AuditDefaultModifier(AuditModifierRecorder recorder) : IEntityModifier<Entities.Audit>
{
    public const string Name = "Default";

    public int Priority => 10;

    public ValueTask Apply(
        Entities.Audit entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (!AuditModifierRecorder.IsMarked(entity))
            return ValueTask.CompletedTask;

        recorder.Record(Name, entity, context, operation, principal);

        if (operation == EntityChangeOperation.Deleted)
            return ValueTask.CompletedTask;

        entity.Content = $"{entity.Content} [{operation}]";

        if (operation != EntityChangeOperation.Created)
            return ValueTask.CompletedTask;

        var activated = principal?.Activated ?? DateTimeOffset.UtcNow;
        var activatedBy = principal?.ActivatedBy;

        // related entity saved in the same SaveChanges as the entity being created
        var child = new Entities.Audit
        {
            Date = entity.Date,
            Content = "Child",
            Username = entity.Username + AuditModifierRecorder.ChildSuffix,
            Created = activated,
            CreatedBy = activatedBy,
            Updated = activated,
            UpdatedBy = activatedBy,
        };

        context.Set<Entities.Audit>().Add(child);

        return ValueTask.CompletedTask;
    }
}

public sealed class AuditFirstModifier(AuditModifierRecorder recorder) : IEntityModifier<Entities.Audit>
{
    public const string Name = "First";

    public int Priority => 1;

    public ValueTask Apply(
        Entities.Audit entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (AuditModifierRecorder.IsMarked(entity))
            recorder.Record(Name, entity, context, operation, principal);

        return ValueTask.CompletedTask;
    }
}

public sealed class AuditNamedModifier(AuditModifierRecorder recorder) : IEntityModifier<Entities.Audit>
{
    public const string Name = "Named";

    public int Priority => 0;

    public ValueTask Apply(
        Entities.Audit entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (!AuditModifierRecorder.IsMarked(entity))
            return ValueTask.CompletedTask;

        recorder.Record(Name, entity, context, operation, principal);

        if (operation != EntityChangeOperation.Deleted)
            entity.Content = $"{entity.Content} [{Name}]";

        return ValueTask.CompletedTask;
    }
}
