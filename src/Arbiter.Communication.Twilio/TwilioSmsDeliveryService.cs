using System.Globalization;

using Arbiter.Communication.Extensions;
using Arbiter.Communication.Sms;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Arbiter.Communication.Twilio;

/// <summary>
/// Provides an implementation of <see cref="ISmsDeliveryService"/> that delivers SMS messages using Twilio.
/// </summary>
public partial class TwilioSmsDeliveryService : ISmsDeliveryService
{
    private readonly ILogger<TwilioSmsDeliveryService> _logger;
    private readonly IOptions<SmsConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioSmsDeliveryService"/> class.
    /// Initializes the Twilio client with credentials from configuration.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="options">The SMS configuration options, including Twilio credentials and sender number.</param>
    public TwilioSmsDeliveryService(ILogger<TwilioSmsDeliveryService> logger, IOptions<SmsConfiguration> options)
    {
        _logger = logger;
        _options = options;

        // Initialize Twilio client with credentials from configuration
        TwilioClient.Init(_options.Value.UserName, _options.Value.Password);
    }


    /// <summary>
    /// Sends the specified <see cref="SmsMessage"/> asynchronously using Twilio.
    /// </summary>
    /// <param name="message">The SMS message to send, including sender, recipient, and message content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="SmsResult"/> indicating whether the SMS was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    public async Task<SmsResult> Send(SmsMessage message, CancellationToken cancellationToken = default)
    {
        var truncatedMessage = message.Message.Truncate(50);
        var senderNumber = message.Sender.HasValue() ? message.Sender : _options.Value.SenderNumber;

        LogSendingSms(_logger, message.Recipient, senderNumber, truncatedMessage);

        try
        {
            var messageResponse = await MessageResource
                .CreateAsync(
                    to: new PhoneNumber(message.Recipient),
                    from: new PhoneNumber(senderNumber),
                    body: message.Message
                )
                .ConfigureAwait(false);

            LogTwilioResponse(_logger, messageResponse.Status, messageResponse.Sid, messageResponse.To, messageResponse.ErrorMessage);

            if (messageResponse.ErrorCode.HasValue)
                return SmsResult.Fail($"SMS send failed with status {messageResponse.Status}");

            var segments = int.TryParse(messageResponse.NumSegments, CultureInfo.InvariantCulture, out var s) ? s : SmsSegment.Calculate(message.Message);
            return SmsResult.Success("SMS sent successfully.", segments);
        }
        catch (Exception ex)
        {
            LogSendError(_logger, ex, message.Recipient, ex.Message);
            return SmsResult.Fail("An error occurred while sending the SMS", ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sending SMS to '{Recipient}' from '{Sender}' with message '{Message}' using Twilio")]
    private static partial void LogSendingSms(ILogger logger, string recipient, string sender, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Twilio Message Response {Status}; Sid: {Sid}; Recipient: {Recipient}; Error: {ErrorMessage}")]
    private static partial void LogTwilioResponse(ILogger logger, object status, string sid, string recipient, string? errorMessage);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending SMS to '{Recipient}': {ErrorMessage}")]
    private static partial void LogSendError(ILogger logger, Exception exception, string recipient, string errorMessage);
}
