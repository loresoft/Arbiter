using Azure.Messaging.ServiceBus;

using MessagePack;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Messaging.ServiceBus.Tests;

[Category("LocalOnly")]
[Property("Category", "LocalOnly")]
public class ServiceBusIntegrationTests
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;

    [Test]
    public async Task SendQueueMessage()
    {
        var sender = Services.GetRequiredKeyedService<ServiceBusSender>("test-queue");
        sender.Should().NotBeNull();

        var messageId = $"{nameof(SendQueueMessage)}-{Guid.NewGuid():N}";
        var message = new ServiceBusMessage(BinaryData.FromString($"Integration test queue message {messageId}"))
        {
            MessageId = messageId,
            Subject = nameof(SendQueueMessage),
        };

        await sender.SendMessageAsync(message);
    }

    [Test]
    public async Task SendTopicMessage()
    {
        var sender = Services.GetRequiredKeyedService<ServiceBusSender>("test-topic");
        sender.Should().NotBeNull();

        var messageId = $"{nameof(SendTopicMessage)}-{Guid.NewGuid():N}";
        var message = new ServiceBusMessage(BinaryData.FromString($"Integration test topic message {messageId}"))
        {
            MessageId = messageId,
            Subject = nameof(SendTopicMessage),
        };

        await sender.SendMessageAsync(message);
    }

    [Test]
    public async Task SendQueueMessageAsJson()
    {
        var sender = Services.GetRequiredKeyedService<ServiceBusSender>("test-queue");
        sender.Should().NotBeNull();

        var message = new IntegrationTestMessage(
            Id: $"{nameof(SendQueueMessageAsJson)}-{Guid.NewGuid():N}",
            Source: nameof(SendQueueMessageAsJson),
            Created: DateTimeOffset.UtcNow);

        await sender.SendAsJsonAsync(message);
    }

    [Test]
    public async Task SendTopicMessageAsMessagePack()
    {
        var sender = Services.GetRequiredKeyedService<ServiceBusSender>("test-topic");
        sender.Should().NotBeNull();

        var message = new IntegrationTestMessage(
            Id: $"{nameof(SendTopicMessageAsMessagePack)}-{Guid.NewGuid():N}",
            Source: nameof(SendTopicMessageAsMessagePack),
            Created: DateTimeOffset.UtcNow);

        await sender.SendAsMessagePackAsync(message);
    }
}

[MessagePackObject]
public sealed record IntegrationTestMessage(
    [property: Key(0)] string Id,
    [property: Key(1)] string Source,
    [property: Key(2)] DateTimeOffset Created);
