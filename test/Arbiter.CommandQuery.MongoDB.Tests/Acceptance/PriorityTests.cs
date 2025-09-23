using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Tests.Constants;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.MongoDB.Tests.Acceptance;

public class PriorityTests : DatabaseTestBase
{
    [Test]
    public async Task EntityIdentifierQuery()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var identifierQuery = new EntityIdentifierQuery<string, PriorityReadModel>(MockPrincipal.Default, PriorityConstants.Normal.Id);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Id.Should().Be(PriorityConstants.Normal.Id);
    }

    [Test]
    public async Task EntityIdentifiersQuery()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var identifiers = new[]
        {
            PriorityConstants.Normal.Id,
            PriorityConstants.High.Id
        };

        var identifierQuery = new EntityIdentifiersQuery<string, PriorityReadModel>(MockPrincipal.Default, identifiers);
        var identifierResults = await mediator.Send(identifierQuery);

        identifierResults.Should().NotBeNull();
        identifierResults.Count.Should().Be(2);
    }

    [Test]
    public async Task EntityQueryIn()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var identifiers = new[]
        {
            PriorityConstants.Normal.Id,
            PriorityConstants.High.Id
        };

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = SortDirections.Descending } },
            Filter = new EntityFilter { Name = "Id", Operator = FilterOperators.In, Value = identifiers }
        };
        var listQuery = new EntityPagedQuery<PriorityReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();
        listResult.Total.Should().Be(2);
    }

}
