using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

public partial class AuditProfile
    : AutoMapper.Profile
{
    public AuditProfile()
    {
        CreateMap<Data.Entities.Audit, AuditReadModel>();

        CreateMap<AuditReadModel, AuditUpdateModel>();

        CreateMap<AuditCreateModel, Data.Entities.Audit>();

        CreateMap<Data.Entities.Audit, AuditUpdateModel>();

        CreateMap<AuditUpdateModel, Data.Entities.Audit>();
    }

}
