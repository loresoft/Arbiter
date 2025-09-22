namespace Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

public partial class Role
    : IHaveIdentifier<int>, ITrackCreated, ITrackUpdated
{
    public Role()
    {
        #region Generated Constructor
        UserRoles = new HashSet<UserRole>();
        #endregion
    }

    #region Generated Properties
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    public virtual ICollection<UserRole> UserRoles { get; set; }

    #endregion

}
