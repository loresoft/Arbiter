using MongoDB.Abstracts;
using MongoDB.Driver;

using Tracker.WebService.Data.Entities;

namespace Tracker.WebService.Data.Repositories;

[RegisterSingleton]
public class StatusRepository(IMongoDatabase mongoDatabase) : MongoEntityRepository<Status>(mongoDatabase)
{
    protected override void EnsureIndexes(IMongoCollection<Status> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Status>(
                Builders<Status>.IndexKeys.Ascending(s => s.Name)
            ),
            new CreateOneIndexOptions()
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Status>(
                Builders<Status>.IndexKeys.Ascending(s => s.IsActive)
            )
        );

    }

}
