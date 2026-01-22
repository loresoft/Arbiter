using Arbiter.Dispatcher;

using Rocks;

namespace Arbiter.Dispatcher.Client.Tests;

[RockPartial(typeof(IDispatcherDataService), BuildType.Create)]
public sealed partial class MockDataService;
