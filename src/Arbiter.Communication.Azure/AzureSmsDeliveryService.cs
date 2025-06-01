using Arbiter.Communication.Extensions;
using Arbiter.Communication.Sms;

using Azure.Communication.Sms;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Communication.Azure;

/// <summary>
/// Provides an implementation of <see cref="ISmsDeliveryService"/> that delivers SMS messages using Azure Communication Services.
/// </summary>
public class AzureSmsDeliveryService : ISmsDeliveryService
{
    private readonly ILogger<AzureSmsDeliveryService> _logger;
    private readonly SmsClient _smsClient;
    private readonly IOptions<SmsConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSmsDeliveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="smsClient">The Azure Communication Services SMS client.</param>
    /// <param name="options">The SMS configuration options.</param>
    public AzureSmsDeliveryService(ILogger<AzureSmsDeliveryService> logger, SmsClient smsClient, IOptions<SmsConfiguration> options)
    {
        _logger = logger;
        _smsClient = smsClient;
        _options = options;
    }

    /// <summary>
    /// Sends the specified <see cref="SmsMessage"/> asynchronously using Azure Communication Services.
    /// </summary>
    /// <param name="message">The SMS message to send, including sender, recipient, and message content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="SmsResult"/> indicating whether the SMS was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    public async Task<SmsResult> Send(SmsMessage message, CancellationToken cancellationToken = default)
    {
        var truncatedMessage = message.Message.Truncate(20);
        var senderNumber = message.Sender.HasValue() ? message.Sender : _options.Value.SenderNumber;

        _logger.LogDebug("Sending SMS to '{Recipient}' from '{Sender} with message '{Message}' using Azure Communication", message.Recipient, senderNumber, truncatedMessage);

        try
        {
            var results = await _smsClient
                .SendAsync(
                    from: message.Sender,
                    to: message.Recipient,
                    message: message.Message,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            if (results.Value.Successful)
            {
                _logger.LogInformation("Sent SMS to '{Recipients}' with message '{Message}'", message.Recipient, truncatedMessage);
                return SmsResult.Success("SMS sent successfully.");
            }

            _logger.LogError("Error sending SMS to '{Recipients}' with message '{Message}'", message.Recipient, truncatedMessage);
            return SmsResult.Fail($"Error sending SMS: {results.Value.ErrorMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to '{Recipients}' with message '{Message}': {ErrorMessage}", message.Recipient, truncatedMessage, ex.Message);
            return SmsResult.Fail("An error occurred while sending the SMS", ex);
        }
    }
}
