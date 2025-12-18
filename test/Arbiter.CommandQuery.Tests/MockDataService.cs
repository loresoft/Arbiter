using Arbiter.Dispatcher;

using Rocks;

namespace Arbiter.CommandQuery.Tests;

[RockPartial(typeof(IDispatcherDataService), BuildType.Create)]
public sealed partial class MockDataService;
