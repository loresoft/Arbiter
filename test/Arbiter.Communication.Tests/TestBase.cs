namespace Arbiter.Communication.Tests;

public abstract class TestBase
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;
}
