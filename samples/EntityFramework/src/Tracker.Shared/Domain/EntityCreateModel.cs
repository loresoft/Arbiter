using Arbiter.CommandQuery.Models;

namespace Tracker.Domain;

[Equatable]
public partial class EntityCreateModel : EntityCreateModel<int>
{
    public EntityCreateModel()
    {
        string includeEntity = "true";
        if (bool.TryParse(includeEntity, out var b) && b)
        {
        }

    }
}
