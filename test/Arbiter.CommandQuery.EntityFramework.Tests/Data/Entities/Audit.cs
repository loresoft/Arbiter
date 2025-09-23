namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class Audit
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated
{
    public Audit()
    {
        #region Generated Constructor
        #endregion
    }

    #region Generated Properties
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string Content { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    #endregion

}
