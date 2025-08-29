namespace Arbiter.Benchmarks.Mapping;

public partial class PriorityProfile
    : AutoMapper.Profile
{
    public PriorityProfile()
    {
        CreateMap<Priority, PriorityReadModel>();
    }
}
