using Azure.Messaging.WebPubSub.Clients;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub.Tests;

/// <summary>
/// A minimal processor used to verify Web PubSub processor registration.
/// </summary>
public sealed class TestProcessor(WebPubSubHubContext context, IHostApplicationLifetime lifetime, ILogger<TestProcessor> logger)
    : WebPubSubProcessorBase(context, lifetime, logger)
{
    protected override Task ProcessGroupMessageAsync(WebPubSubGroupMessageEventArgs args)
        => Task.CompletedTask;

    protected override Task ProcessServerMessageAsync(WebPubSubServerMessageEventArgs args)
        => Task.CompletedTask;
}
