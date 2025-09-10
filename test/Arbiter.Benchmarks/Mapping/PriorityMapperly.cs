#pragma warning disable RMG012 // Source member was not found for target member

using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

namespace Arbiter.Benchmarks.Mapping;

[Mapper]
public partial class PriorityMapperly : IMapper<Priority, PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial PriorityReadModel? Map(Priority? source);

    public partial void Map(Priority source, PriorityReadModel destination);

    public partial IQueryable<PriorityReadModel> ProjectTo(IQueryable<Priority> source);
}
