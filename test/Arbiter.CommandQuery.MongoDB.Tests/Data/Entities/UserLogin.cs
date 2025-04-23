using Arbiter.CommandQuery.Definitions;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

public class UserLogin : MongoEntity, IHaveIdentifier<string>, ITrackCreated, ITrackUpdated
{
    public string? EmailAddress { get; set; }

    public string? UserId { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsSuccessful { get; set; }

    public string? FailureMessage { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
