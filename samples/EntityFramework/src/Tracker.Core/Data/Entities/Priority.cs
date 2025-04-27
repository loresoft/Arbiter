using System;
using System.Collections.Generic;

namespace Tracker.Data.Entities;

public partial class Priority
    : Arbiter.CommandQuery.Definitions.IHaveIdentifier<int>, Arbiter.CommandQuery.Definitions.ITrackCreated, Arbiter.CommandQuery.Definitions.ITrackUpdated
{
    public Priority()
    {
        #region Generated Constructor
        Tasks = new HashSet<Task>();
        #endregion
    }

    #region Generated Properties
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }

    #endregion

    #region Generated Relationships
    public virtual ICollection<Task> Tasks { get; set; }

    #endregion

}
