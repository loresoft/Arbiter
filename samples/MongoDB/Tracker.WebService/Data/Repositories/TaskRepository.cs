using MongoDB.Abstracts;
using MongoDB.Driver;

namespace Tracker.WebService.Data.Repositories;

[RegisterSingleton]
public class TaskRepository(IMongoDatabase mongoDatabase) : MongoEntityRepository<Entities.Task>(mongoDatabase)
{
    protected override void EnsureIndexes(IMongoCollection<Entities.Task> mongoCollection)
    {
        base.EnsureIndexes(mongoCollection);

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Entities.Task>(
                Builders<Entities.Task>.IndexKeys.Ascending(s => s.StatusId)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Entities.Task>(
                Builders<Entities.Task>.IndexKeys.Ascending(s => s.PriorityId)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Entities.Task>(
                Builders<Entities.Task>.IndexKeys.Ascending(s => s.AssignedId)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Entities.Task>(
                Builders<Entities.Task>.IndexKeys.Ascending(s => s.TenantId)
            )
        );

        mongoCollection.Indexes.CreateOne(
            new CreateIndexModel<Entities.Task>(
                Builders<Entities.Task>.IndexKeys.Ascending(s => s.IsDeleted)
            )
        );

    }

}
