using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Arbiter.Mediation.OpenTelemetry;

/// <summary>
/// Provides OpenTelemetry-based diagnostics for logging activities and metrics related to requests and notifications.
/// </summary>
public class MediatorDiagnostic : IMediatorDiagnostic
{
    /// <summary>
    /// The name of the diagnostic source for the mediator.
    /// </summary>
    public const string ActivitySourceName = "Arbiter.Mediator";

    /// <summary>
    /// The name of the activity for sending requests.
    /// </summary>
    public const string SendActivity = "Arbiter.Mediator.Send";

    /// <summary>
    /// The name of the activity for publishing notifications.
    /// </summary>
    public const string PublishActivity = "Arbiter.Mediator.Publish";

    /// <summary>
    /// The name of the meter for the mediator.
    /// </summary>
    public const string MeterName = "Arbiter.Mediator";

    /// <summary>
    /// The tag name for the request type in diagnostic activities.
    /// </summary>
    public const string RequestTypeTag = "mediator.request_type";

    /// <summary>
    /// The tag name for the response type in diagnostic activities.
    /// </summary>
    public const string ResponseTypeTag = "mediator.response_type";

    /// <summary>
    /// The tag name for the notification type in diagnostic activities.
    /// </summary>
    public const string NotificationTypeTag = "mediator.notification_type";

    /// <summary>
    /// The tag name for the caller member in diagnostic activities.
    /// </summary>
    public const string CallerTag = "mediator.caller_member";

    /// <summary>
    /// The name of the counter for the number of requests sent.
    /// </summary>
    public const string SendCount = "mediator.send.count";

    /// <summary>
    /// The name of the counter for the number of notifications published.
    /// </summary>
    public const string PublishCount = "mediator.publish.count";

    /// <summary>
    /// The name of the counter for the number of errors encountered.
    /// </summary>
    public const string ErrorCount = "mediator.error.count";

    /// <summary>
    /// The <see cref="ActivitySource"/> used for creating diagnostic activities.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ThisAssembly.Version);

    private readonly Meter _meter;
    private readonly Counter<long> _sendCounter;
    private readonly Counter<long> _publishedCounter;
    private readonly Counter<long> _errorsCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorDiagnostic"/> class.
    /// </summary>
    /// <param name="meterFactory">The factory used to create the <see cref="Meter"/> for diagnostics.</param>
    public MediatorDiagnostic(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName, ThisAssembly.Version);

        _sendCounter = _meter.CreateCounter<long>(SendCount, "requests", "Number of requests sent");
        _publishedCounter = _meter.CreateCounter<long>(PublishCount, "notifications", "Number of notifications published");
        _errorsCounter = _meter.CreateCounter<long>(ErrorCount, "errors", "Number of errors encountered");
    }

    /// <inheritdoc />
    public IDisposable? StartSend<TRequest, TResponse>()
        => StartSend(typeof(TRequest).FullName, typeof(TResponse).FullName);

    /// <inheritdoc />
    public IDisposable? StartSend<TResponse>(IRequest<TResponse> request)
        => StartSend(request.GetType().FullName, typeof(TResponse).FullName);

    /// <inheritdoc />
    public IDisposable? StartSend(object request)
        => StartSend(request.GetType().FullName);

    /// <inheritdoc />
    public IDisposable? StartPublish<TNotification>()
    {
        var notificationType = typeof(TNotification).FullName;

        var activity = ActivitySource.StartActivity(PublishActivity, ActivityKind.Internal);
        activity?.SetTag(NotificationTypeTag, notificationType);

        if (!_publishedCounter.Enabled)
            return activity;

        var tagList = new TagList
        {
            { NotificationTypeTag, notificationType },
        };

        _publishedCounter.Add(1, tagList);

        return activity;
    }

    /// <inheritdoc />
    public void ActivityError(IDisposable? activity, Exception? exception, object? request, [CallerMemberName] string memberName = "")
    {
        var requestType = request?.GetType().FullName;

        if (activity is Activity instance)
        {
            instance.SetStatus(ActivityStatusCode.Error);
            instance.SetTag(CallerTag, memberName);
            if (exception != null)
                instance.AddException(exception);
        }

        if (!_errorsCounter.Enabled)
            return;

        var tagList = new TagList
        {
            { RequestTypeTag, requestType },
            { CallerTag, memberName },
        };

        _errorsCounter.Add(1, tagList);
    }


    private Activity? StartSend(string? requestType, string? responseType = null)
    {
        var activity = ActivitySource.StartActivity(SendActivity, ActivityKind.Internal);

        activity?.SetTag(RequestTypeTag, requestType);
        activity?.SetTag(ResponseTypeTag, responseType);

        if (!_sendCounter.Enabled)
            return activity;

        var tagList = new TagList
        {
            { RequestTypeTag, requestType },
            { ResponseTypeTag, responseType },
        };

        _sendCounter.Add(1, tagList);

        return activity;
    }
}
