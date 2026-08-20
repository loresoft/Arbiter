namespace Arbiter.Messaging.WebPubSub.Tests;

public sealed class TestPublisher(WebPubSubHubContext context) : WebPubSubPublisherBase(context)
{
    public string ResolvedHubName => HubName;

    public IReadOnlyDictionary<string, string> ResolvedGroups => Groups;

    public string GetResolvedGroupName(string groupName) => GetGroupName(groupName);
}
