using Arbiter.CommandQuery.Definitions;

using MongoDB.Abstracts;
using MongoDB.Bson.Serialization.Attributes;

namespace Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

public class Priority : MongoEntity, IHaveIdentifier<string>, IHaveKey, ITrackCreated, ITrackUpdated
{
    [BsonGuidRepresentation(global::MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid Key { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
