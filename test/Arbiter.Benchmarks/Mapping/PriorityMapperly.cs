#pragma warning disable RMG012 // SourceExpression member was not found for target member

using System.Diagnostics.CodeAnalysis;

using Arbiter.Mapping;

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
