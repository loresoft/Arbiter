using MongoDB.Abstracts;
using MongoDB.Driver;

using Tracker.WebService.Data.Entities;

namespace Tracker.WebService.Data.Repositories;

[RegisterSingleton]
public class UserRepository(IMongoDatabase mongoDatabase) : MongoEntityRepository<User>(mongoDatabase)
{
    protected override void EnsureIndexes(IMongoCollection<User> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(s => s.EmailAddress),
                new CreateIndexOptions { Unique = true }
            )
        );
    }
}
