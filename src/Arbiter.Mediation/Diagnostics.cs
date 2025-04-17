using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Arbiter.Mediation;

internal static class Diagnostics
{
    public const string DiagnosticName = "Arbiter.Mediator";

    public static readonly ActivitySource ActivitySource = new(DiagnosticName, ThisAssembly.Version);


    internal static class Activties
    {
        public const string SendName = "Mediator.Send";
        public const string PublishName = "Mediator.Publish";
    }

    internal static class Tags
    {
        public const string RequestType = "mediator.request_type";
        public const string ResponseType = "mediator.response_type";
        public const string NotificationType = "mediator.notification_type";
    }

    internal static class Metrics
    {
        public const string SendCount = "mediator.send.count";
        public const string PublishCount = "mediator.publish.count";
        public const string ErrorCount = "mediator.error.count";

        public static readonly Meter Meter = new(DiagnosticName, ThisAssembly.Version);

        public static readonly Counter<long> SendCounter = Meter.CreateCounter<long>(SendCount, "requests", "Number of requests sent");
        public static readonly Counter<long> PublishedCounter = Meter.CreateCounter<long>(PublishCount, "notifications", "Number of notifications published");
        public static readonly Counter<long> ErrorsCounter = Meter.CreateCounter<long>(ErrorCount, "errors", "Number of errors encountered");
    }

    internal static void Increment<T>(
        this Counter<T> counter,
        T delta,
        string? requestType = null,
        string? responseType = null,
        string? notificationType = null)
        where T : struct
    {
        if (!counter.Enabled)
            return;

        var tagList = new TagList();
        if (requestType != null)
            tagList.Add(Tags.RequestType, requestType);

        if (responseType != null)
            tagList.Add(Tags.ResponseType, responseType);

        if (notificationType != null)
            tagList.Add(Tags.NotificationType, notificationType);

        counter.Add(delta, tagList);
    }
}
