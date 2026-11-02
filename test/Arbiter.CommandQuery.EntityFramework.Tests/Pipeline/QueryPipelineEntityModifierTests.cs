using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.Notifications;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Pipeline;

public class QueryPipelineEntityModifierTests
{
    [Test]
    public async Task ApplyModifiersRunsDefaultModifiersInPriorityOrder()
    {
        var services = new ServiceCollection();
        services.AddQueryPipeline();
        services.AddScoped<IEntityModifier<Sample>>(_ => new SampleModifier("C", 30));
        services.AddScoped<IEntityModifier<Sample>>(_ => new SampleModifier("A", 10));
        services.AddScoped<IEntityModifier<Sample>>(_ => new SampleModifier("B", 20));
        services.AddKeyedScoped<IEntityModifier<Sample>>("named", (_, _) => new SampleModifier("N", 0));

        await using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IQueryPipeline>();

        using var dataContext = CreateDataContext();
        var entity = new Sample();

        await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Created);

        entity.Applied.Should().Equal("A", "B", "C");
    }

    [Test]
    public async Task ApplyModifiersRunsOnlyKeyedModifiersForPipelineName()
    {
        var services = new ServiceCollection();
        services.AddQueryPipeline();
        services.AddScoped<IEntityModifier<Sample>>(_ => new SampleModifier("A", 10));
        services.AddKeyedScoped<IEntityModifier<Sample>>("named", (_, _) => new SampleModifier("N2", 2));
        services.AddKeyedScoped<IEntityModifier<Sample>>("named", (_, _) => new SampleModifier("N1", 1));
        services.AddKeyedScoped<IEntityModifier<Sample>>("other", (_, _) => new SampleModifier("O", 0));

        await using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IQueryPipeline>();

        using var dataContext = CreateDataContext();
        var entity = new Sample();

        await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Created, "named");

        entity.Applied.Should().Equal("N1", "N2");
    }

    [Test]
    public async Task ApplyModifiersPassesContextOperationAndPrincipal()
    {
        var services = new ServiceCollection();
        services.AddQueryPipeline();
        services.AddScoped<IEntityModifier<Sample>>(_ => new SampleModifier("A", 10));

        await using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IQueryPipeline>();

        using var dataContext = CreateDataContext();
        var principal = new SamplePrincipal(MockPrincipal.Default, DateTimeOffset.UtcNow, "test");
        var entity = new Sample();

        await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Deleted, principal);

        entity.Context.Should().BeSameAs(dataContext);
        entity.Operation.Should().Be(EntityChangeOperation.Deleted);
        entity.Principal.Should().BeSameAs(principal);
    }

    [Test]
    public async Task ApplyModifiersWithNoModifiersLeavesEntityUnchanged()
    {
        var services = new ServiceCollection();
        services.AddQueryPipeline();

        await using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IQueryPipeline>();

        using var dataContext = CreateDataContext();
        var entity = new Sample();

        await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Updated, "missing");

        entity.Applied.Should().BeEmpty();
    }

    private static DbContext CreateDataContext()
    {
        var options = new DbContextOptionsBuilder().Options;
        return new DbContext(options);
    }

    public sealed class Sample
    {
        public List<string> Applied { get; } = [];

        public DbContext? Context { get; set; }

        public EntityChangeOperation? Operation { get; set; }

        public IRequestPrincipal? Principal { get; set; }
    }

    private sealed class SampleModifier(string name, int priority) : IEntityModifier<Sample>
    {
        public int Priority => priority;

        public ValueTask Apply(
            Sample entity,
            DbContext context,
            EntityChangeOperation operation,
            IRequestPrincipal? principal = null,
            CancellationToken cancellationToken = default)
        {
            entity.Applied.Add(name);
            entity.Context = context;
            entity.Operation = operation;
            entity.Principal = principal;

            return ValueTask.CompletedTask;
        }
    }

    private sealed class SamplePrincipal(ClaimsPrincipal? principal, DateTimeOffset activated, string? activatedBy) : IRequestPrincipal
    {
        public DateTimeOffset Activated { get; } = activated;

        public string? ActivatedBy { get; } = activatedBy;

        public ClaimsPrincipal? Principal { get; private set; } = principal;

        public void ApplyPrincipal(ClaimsPrincipal? principal) => Principal = principal;
    }
}
