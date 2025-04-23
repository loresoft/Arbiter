using Arbiter.CommandQuery.Definitions;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

public class Status : MongoEntity, IHaveIdentifier<string>, ITrackCreated, ITrackUpdated
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

}
