namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class TaskExtended
{
    public TaskExtended()
    {
        #region Generated Constructor
        #endregion
    }

    #region Generated Properties
    public int TaskId { get; set; }

    public string? UserAgent { get; set; }

    public string? Browser { get; set; }

    public string? OperatingSystem { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    public virtual Task Task { get; set; } = null!;

    #endregion

}
