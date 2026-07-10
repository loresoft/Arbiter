using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery;
using Arbiter.Mediation;

using Azure.Messaging.ServiceBus;

using MessagePack;

using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusBackgroundServiceTests
{
    [Test]
    public async Task ProcessMessageAsync_WithNullArgs_ThrowsArgumentNullException()
    {
        var mediator = new TestMediator();
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);

        var act = async () => await service.ProcessMessageAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task ProcessMessageAsync_WithValidRequest_SendsRequestAndCompletesMessage()
    {
        var request = new TestRequest("Processed");
        var args = CreateProcessMessageArgs(request);
        var mediator = new TestMediator();
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);

        await service.ProcessMessageAsync(args);

        mediator.Requests.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(request);
        args.CompletedMessage.Should().BeSameAs(args.Message);
        args.DeadLetterCount.Should().Be(0);
    }

    [Test]
    public async Task ProcessMessageAsync_WithValidReceivedMessage_SendsRequestAndCompletesMessage()
    {
        var request = new TestRequest("Processed");
        var message = CreateServiceBusMessage(request);
        var mediator = new TestMediator();
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);
        ServiceBusReceivedMessage? completedMessage = null;
        var deadLetterCount = 0;

        await service.ProcessMessageAsync(
            message,
            completeMessage: (receivedMessage, _) =>
            {
                completedMessage = receivedMessage;
                return Task.CompletedTask;
            },
            deadLetterMessage: (_, _, _, _) =>
            {
                deadLetterCount++;
                return Task.CompletedTask;
            });

        mediator.Requests.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(request);
        completedMessage.Should().BeSameAs(message);
        deadLetterCount.Should().Be(0);
    }

    [Test]
    public async Task ProcessMessageAsync_WithEmptySubject_DeadLettersMessage()
    {
        var message = CreateMessage(MessagePackSerializer.Serialize(new TestRequest("Ignored")), subject: " ");
        var args = new TestProcessMessageEventArgs(message);
        var mediator = new TestMediator();
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);

        await service.ProcessMessageAsync(args);

        mediator.Requests.Should().BeEmpty();
        args.CompletedMessage.Should().BeNull();
        args.DeadLetterCount.Should().Be(1);
        args.DeadLetterReason.Should().Be("Message subject is empty, unable to determine request type");
    }

    [Test]
    public async Task ProcessMessageAsync_WhenMessagePackDeserializesNull_DeadLettersMessage()
    {
        var message = CreateMessage(MessagePackSerializer.Serialize<object?>(null));
        var args = new TestProcessMessageEventArgs(message);
        var mediator = new TestMediator();
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);

        await service.ProcessMessageAsync(args);

        mediator.Requests.Should().BeEmpty();
        args.CompletedMessage.Should().BeNull();
        args.DeadLetterCount.Should().Be(1);
        args.DeadLetterReason.Should().Be("Invalid background message, failed to deserialize");
    }

    [Test]
    public async Task ProcessMessageAsync_WhenMediatorThrows_DeadLettersMessage()
    {
        var args = CreateProcessMessageArgs(new TestRequest("Failed"));
        var mediator = new TestMediator
        {
            Exception = new InvalidOperationException("Mediator failed"),
        };
        var service = new ServiceBusBackgroundService(
            NullLogger<ServiceBusBackgroundService>.Instance,
            mediator);

        await service.ProcessMessageAsync(args);

        mediator.Requests.Should().ContainSingle();
        args.CompletedMessage.Should().BeNull();
        args.DeadLetterCount.Should().Be(1);
        args.DeadLetterReason.Should().Be("Error processing message");
        args.DeadLetterErrorDescription.Should().Be("Mediator failed");
    }

    private static TestProcessMessageEventArgs CreateProcessMessageArgs(TestRequest request)
    {
        var message = CreateServiceBusMessage(request);

        return new TestProcessMessageEventArgs(message);
    }

    private static ServiceBusReceivedMessage CreateServiceBusMessage(TestRequest request)
    {
        var body = MessagePackSerializer.Serialize(typeof(TestRequest), request, MessagePackDefaults.DefaultSerializerOptions);

        return CreateMessage(body);
    }

    private static ServiceBusReceivedMessage CreateMessage(byte[] body, string? subject = null)
        => ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromBytes(body),
            messageId: Guid.NewGuid().ToString("N"),
            subject: subject ?? typeof(TestRequest).GetPortableName());

    [MessagePackObject]
    public sealed record TestRequest([property: Key(0)] string Message) : IRequest<Unit>;

    private sealed class TestMediator : IMediator
    {
        public List<object> Requests { get; } = [];

        public Exception? Exception { get; init; }

        public ValueTask<TResponse?> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
        {
            Requests.Add(request);

            if (Exception is not null)
                return ValueTask.FromException<TResponse?>(Exception);

            return ValueTask.FromResult<TResponse?>(default);
        }

        public ValueTask<TResponse?> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            if (Exception is not null)
                return ValueTask.FromException<TResponse?>(Exception);

            return ValueTask.FromResult<TResponse?>(default);
        }

        public ValueTask<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            if (Exception is not null)
                return ValueTask.FromException<object?>(Exception);

            return ValueTask.FromResult<object?>(null);
        }

        public ValueTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => ValueTask.CompletedTask;
    }

    private sealed class TestProcessMessageEventArgs : ProcessMessageEventArgs
    {
        public TestProcessMessageEventArgs(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
            : base(message, new TestServiceBusReceiver(), cancellationToken)
        {
        }

        public ServiceBusReceivedMessage? CompletedMessage { get; private set; }

        public int DeadLetterCount { get; private set; }

        public string? DeadLetterReason { get; private set; }

        public string? DeadLetterErrorDescription { get; private set; }

        public override Task CompleteMessageAsync(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
        {
            CompletedMessage = message;
            return Task.CompletedTask;
        }

        public override Task DeadLetterMessageAsync(
            ServiceBusReceivedMessage message,
            string deadLetterReason,
            string? deadLetterErrorDescription = default,
            CancellationToken cancellationToken = default)
        {
            DeadLetterCount++;
            DeadLetterReason = deadLetterReason;
            DeadLetterErrorDescription = deadLetterErrorDescription;
            return Task.CompletedTask;
        }
    }

    private sealed class TestServiceBusReceiver : ServiceBusReceiver;
}
