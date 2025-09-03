namespace Arbiter.CommandQuery.MongoDB.Tests.Constants;

public static class TaskConstants
{
    public static readonly Data.Entities.Task FindEarth = new()
    {
        Id = "68b79c9924ef22c4b60177b5",
        Title = "Find Earth",
        Description = "Find the lost colony of Earth",
        PriorityId = PriorityConstants.Normal.Id,
        StatusId = StatusConstants.NotStarted.Id,
        AssignedId = UserConstants.LauraRoslin.Id,
        TenantId = TenantConstants.Test.Id,
    };

    public static readonly Data.Entities.Task ProtectThePresident = new()
    {
        Id = "68b79c9924ef22c4b60177b6",
        Title = "Protect the President",
        Description = "Ensure the safety of President Roslin at all costs",
        PriorityId = PriorityConstants.Normal.Id,
        StatusId = StatusConstants.NotStarted.Id,
        AssignedId = UserConstants.LeeAdama.Id,
        TenantId = TenantConstants.Test.Id,
    };

    public static readonly Data.Entities.Task DefendTheFleet = new()
    {
        Id = "68b79c9924ef22c4b60177b7",
        Title = "Defend the Fleet",
        Description = "Defend the fleet from Cylon attacks",
        PriorityId = PriorityConstants.High.Id,
        StatusId = StatusConstants.InProgress.Id,
        AssignedId = UserConstants.KaraThrace.Id,
        TenantId = TenantConstants.Test.Id,
    };
}
