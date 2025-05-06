using Task = Tracker.WebService.Data.Entities.Task;

namespace Tracker.WebService.Constants;

public static class TaskConstants
{
    ///<summary>Make it to Earth</summary>
    public static readonly Task Earth = new()
    {
        Id = "681a3a315eca7f838f237216",
        Title = "Make it to Earth",
        Description = "Find and make it to Earth",
        StatusId = StatusConstants.InProgress.Id,
        PriorityId = PriorityConstants.High.Id,
        AssignedId = UserConstants.WilliamAdama.Id,
        TenantId = TenantConstants.Battlestar.Id,
        IsDeleted = false,
        CreatedBy = UserConstants.WilliamAdama.Id,
        UpdatedBy = UserConstants.WilliamAdama.Id
    };
    ///<summary>Destroy Humans</summary>
    public static readonly Task Destroy = new()
    {
        Id = "681a3a37f948045fbf5f6e9c",
        Title = "Destroy Humans",
        Description = "Find and destroy all humans",
        StatusId = StatusConstants.InProgress.Id,
        PriorityId = PriorityConstants.High.Id,
        AssignedId = UserConstants.NumberSix.Id,
        TenantId = TenantConstants.Cylons.Id,
        IsDeleted = false,
        CreatedBy = UserConstants.NumberSix.Id,
        UpdatedBy = UserConstants.NumberSix.Id
    };
}
