using MongoDB.Abstracts;
using MongoDB.Driver;

using Tracker.WebService.Data.Entities;

namespace Tracker.WebService.Data.Repositories;

[RegisterSingleton]
public class PriorityRepository(IMongoDatabase mongoDatabase) : MongoEntityRepository<Priority>(mongoDatabase)
{
    protected override void EnsureIndexes(IMongoCollection<Priority> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Priority>(
                Builders<Priority>.IndexKeys.Ascending(s => s.Name)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Priority>(
                Builders<Priority>.IndexKeys.Ascending(s => s.IsActive)
            )
        );
    }

}
