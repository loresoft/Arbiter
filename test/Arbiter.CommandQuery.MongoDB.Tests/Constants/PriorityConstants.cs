using Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

namespace Arbiter.CommandQuery.MongoDB.Tests.Constants;

public static class PriorityConstants
{
    ///<summary>High Priority</summary>
    public static readonly Priority High = new()
    {
        Id = "607a27d37d1e32895e494a80",
        Key = new("f6bc3530-5b30-4963-9071-1b6d329ef43f"),
        Name = "High",
        Description = "High Priority",
        IsActive = true,
        DisplayOrder = 1
    };

    ///<summary>Normal Priority</summary>
    public static readonly Priority Normal = new()
    {
        Id = "607a27d57d1e32895e494a8b",
        Key = new("7df54a7a-6602-446d-b6f2-6565b684c2cb"),
        Name = "Normal",
        Description = "Normal Priority",
        IsActive = true,
        DisplayOrder = 2
    };

    ///<summary>Low Priority</summary>
    public static readonly Priority Low = new()
    {
        Id = "607a27d77d1e32895e494a95",
        Key = new("66cc427d-2e83-4def-9174-443c8d79e5bb"),
        Name = "Low",
        Description = "Low Priority",
        IsActive = true,
        DisplayOrder = 3
    };
}
