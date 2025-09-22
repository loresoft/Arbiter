using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.EntityFramework.Tests.Constants;
using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;
using Arbiter.CommandQuery.Filters;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Acceptance;

public class PriorityTests : DatabaseTestBase
{
    [Test]
    public async Task EntityIdentifierQuery()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var identifierQuery = new EntityIdentifierQuery<int, PriorityReadModel>(MockPrincipal.Default, PriorityConstants.Normal);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Id.Should().Be(PriorityConstants.Normal);
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
            PriorityConstants.Normal,
            PriorityConstants.High
        };

        var identifierQuery = new EntityIdentifiersQuery<int, PriorityReadModel>(MockPrincipal.Default, identifiers);
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
            PriorityConstants.Normal,
            PriorityConstants.High
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

    [Test]
    public async Task EntityQueryDescriptionNull()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = SortDirections.Descending } },
            Filter = new EntityFilter { Name = "Description", Operator = FilterOperators.IsNull }
        };
        var listQuery = new EntityPagedQuery<PriorityReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();
    }

    [Test]
    public async Task EntityQueryDescriptionNotNull()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = SortDirections.Descending } },
            Filter = new EntityFilter { Name = "Description", Operator = FilterOperators.IsNotNull }
        };
        var listQuery = new EntityPagedQuery<PriorityReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();
    }


    [Test]
    public async Task EntityQueryMultipleFilters()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = SortDirections.Descending } },
            Filter = new EntityFilter
            {
                Filters = new List<EntityFilter> {
                    new EntityFilter { Name = "Description", Operator = FilterOperators.IsNull },
                    new EntityFilter { Name = "Name", Operator = FilterOperators.Equal, Value = "High" }
                }
            }
        };
        var listQuery = new EntityPagedQuery<PriorityReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();
    }
}
