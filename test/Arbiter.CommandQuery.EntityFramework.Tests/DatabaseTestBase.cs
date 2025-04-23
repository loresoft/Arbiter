namespace Arbiter.CommandQuery.EntityFramework.Tests;

public abstract class DatabaseTestBase
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    public IServiceProvider ServiceProvider => Application.Services;
}
