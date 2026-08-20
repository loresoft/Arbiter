using System.Net;

using Azure;
using Azure.Core.Pipeline;
using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.WebPubSub.Tests;

public class HealthCheckTests
{
    [Test]
    public void AddWebPubSub_ThrowsForNullBuilder()
    {
        IHealthChecksBuilder? health = null;

        var act = () => health!.AddWebPubSub("notifications");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddWebPubSub_ThrowsForEmptyHubName()
    {
        var services = new ServiceCollection();
        var health = services.AddHealthChecks();

        var act = () => health.AddWebPubSub(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSub_RegistersHubHealthCheck()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceClient = CreateServiceClient(new TestMessageHandler(HttpStatusCode.OK));
        services.AddKeyedSingleton("notifications", serviceClient);

        var health = services.AddHealthChecks();
        var result = health.AddWebPubSub("notifications");

        result.Should().BeSameAs(health);

        using var provider = services.BuildServiceProvider();
        var registration = provider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations
            .Should()
            .ContainSingle()
            .Which;

        registration.Name.Should().Be("Web PubSub Hub: 'notifications'");
        registration.FailureStatus.Should().Be(HealthStatus.Unhealthy);
        registration.Tags.Should().ContainSingle().Which.Should().Be("WebPubSub");
        registration.Factory(provider).Should().BeOfType<WebPubSubHealthCheck>();
    }

    [Test]
    public async Task CheckHealthAsync_ReturnsHealthyForSuccessfulProbe()
    {
        var handler = new TestMessageHandler(HttpStatusCode.OK);
        var serviceClient = CreateServiceClient(handler);
        var healthCheck = new WebPubSubHealthCheck(
            NullLogger<WebPubSubHealthCheck>.Instance,
            serviceClient);
        var context = CreateContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("test.webpubsub.azure.com");
        result.Description.Should().Contain("notifications");
        handler.RequestCount.Should().Be(1);
    }

    [Test]
    public async Task CheckHealthAsync_ReturnsFailureStatusForFailedProbe()
    {
        var handler = new TestMessageHandler(HttpStatusCode.ServiceUnavailable);
        var serviceClient = CreateServiceClient(handler);
        var healthCheck = new WebPubSubHealthCheck(
            NullLogger<WebPubSubHealthCheck>.Instance,
            serviceClient);
        var context = CreateContext(HealthStatus.Degraded);

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Exception.Should().BeOfType<RequestFailedException>();
        result.Description.Should().Contain("notifications");
    }

    [Test]
    public async Task CheckHealthAsync_PassesCancellationTokenToProbe()
    {
        var handler = new TestMessageHandler(HttpStatusCode.OK);
        var serviceClient = CreateServiceClient(handler);
        var healthCheck = new WebPubSubHealthCheck(
            NullLogger<WebPubSubHealthCheck>.Instance,
            serviceClient);
        var context = CreateContext();
        using var cancellationSource = new CancellationTokenSource();

        await healthCheck.CheckHealthAsync(context, cancellationSource.Token);

        handler.CancellationToken.CanBeCanceled.Should().BeTrue();
    }

    private static HealthCheckContext CreateContext(HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "Web PubSub test",
                _ => null!,
                failureStatus,
                tags: null)
        };
    }

    private static WebPubSubServiceClient CreateServiceClient(HttpMessageHandler handler)
    {
        var options = new WebPubSubServiceClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler))
        };

        return new WebPubSubServiceClient(
            "Endpoint=https://test.webpubsub.azure.com;AccessKey=YWJj;Version=1.0;",
            "notifications",
            options);
    }

    private sealed class TestMessageHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            CancellationToken = cancellationToken;

            return Task.FromResult(new HttpResponseMessage(status)
            {
                RequestMessage = request
            });
        }
    }
}
