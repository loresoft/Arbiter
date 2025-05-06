using MongoDB.Abstracts;
using MongoDB.Driver;

using Tracker.WebService.Data.Entities;

namespace Tracker.WebService.Data.Repositories;

[RegisterSingleton]
public class TenantRepository(IMongoDatabase mongoDatabase) : MongoEntityRepository<Tenant>(mongoDatabase)
{
    protected override void EnsureIndexes(IMongoCollection<Tenant> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Tenant>(
                Builders<Tenant>.IndexKeys.Ascending(s => s.Name)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Tenant>(
                Builders<Tenant>.IndexKeys.Ascending(s => s.IsActive)
            )
        );

    }

}
