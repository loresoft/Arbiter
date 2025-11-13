namespace Arbiter.CommandQuery.EntityFramework.Tests.Constants;

public static class PriorityConstants
{
    ///<summary>High Priority</summary>
    public const int High = 1;
    ///<summary>Normal Priority</summary>
    public const int Normal = 2;
    ///<summary>Low Priority</summary>
    public const int Low = 3;

    public static readonly Guid HighKey = new("f6bc3530-5b30-4963-9071-1b6d329ef43f");

    public static readonly Guid NormalKey = new("7df54a7a-6602-446d-b6f2-6565b684c2cb");

    public static readonly Guid LowKey = new("66cc427d-2e83-4def-9174-443c8d79e5bb");
}
