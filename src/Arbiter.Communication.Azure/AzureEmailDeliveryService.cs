using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;

using Azure;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Ace = Azure.Communication.Email;

namespace Arbiter.Communication.Azure;

/// <summary>
/// Provides an implementation of <see cref="IEmailDeliveryService"/> that delivers emails using Azure Communication Services.
/// </summary>
public partial class AzureEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger<AzureEmailDeliveryService> _logger;
    private readonly Ace.EmailClient _emailClient;
    private readonly IOptions<EmailConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEmailDeliveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="emailClient">The Azure Communication Services email client.</param>
    /// <param name="options">The email configuration options.</param>
    public AzureEmailDeliveryService(
        ILogger<AzureEmailDeliveryService> logger,
        Ace.EmailClient emailClient,
        IOptions<EmailConfiguration> options)
    {
        _logger = logger;
        _emailClient = emailClient;
        _options = options;
    }

    /// <summary>
    /// Sends the specified <see cref="EmailMessage"/> asynchronously using Azure Communication Services.
    /// </summary>
    /// <param name="emailMessage">The email message to send, including sender, recipients, content, headers, and attachments.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating whether the email was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    public async Task<EmailResult> Send(
        EmailMessage emailMessage,
        CancellationToken cancellationToken = default)
    {
        var recipients = emailMessage.Recipients.ToString();
        var truncatedSubject = emailMessage.Content.Subject.Truncate(100);

        LogSendingEmailAzure(_logger, recipients, truncatedSubject);

        try
        {
            emailMessage = OverrideRecipient(emailMessage);
            Ace.EmailMessage message = ConvertMessage(emailMessage);

            // Send the email message asynchronously
            var sendOperation = await _emailClient.SendAsync(WaitUntil.Started, message, cancellationToken).ConfigureAwait(false);

            if (sendOperation == null)
            {
                LogEmailSendErrorAzure(_logger, recipients, truncatedSubject, "Unknown");
                return EmailResult.Fail($"Email send failed with status Unknown");
            }

            LogEmailSentAzure(_logger, recipients, truncatedSubject, sendOperation.Id);
            return EmailResult.Success($"Email queued successfully: {sendOperation.Id}");

        }
        catch (Exception ex)
        {
            LogEmailSendExceptionAzure(_logger, recipients, truncatedSubject, ex.Message, ex);
            return EmailResult.Fail("An error occurred while sending the email", ex);
        }
    }

    private static Ace.EmailMessage ConvertMessage(EmailMessage emailMessage)
    {
        Ace.EmailRecipients recipients = new(
            to: emailMessage.Recipients.To.Select(e => new Ace.EmailAddress(e.Address, e.DisplayName)),
            cc: emailMessage.Recipients.Cc?.Select(e => new Ace.EmailAddress(e.Address, e.DisplayName)),
            bcc: emailMessage.Recipients.Bcc?.Select(e => new Ace.EmailAddress(e.Address, e.DisplayName))
        );

        Ace.EmailContent content = new(emailMessage.Content.Subject)
        {
            PlainText = emailMessage.Content.TextBody,
            Html = emailMessage.Content.HtmlBody,
        };

        var senderAddress = emailMessage.Senders.From.Address;

        var message = new Ace.EmailMessage(
            senderAddress: senderAddress,
            recipients: recipients,
            content: content
        );

        if (emailMessage.Headers?.Count > 0)
        {
            foreach (var header in emailMessage.Headers)
                message.Headers.Add(header.Key, header.Value);
        }

        if (emailMessage.Attachments?.Count > 0)
        {
            foreach (var attachment in emailMessage.Attachments)
            {
                Ace.EmailAttachment item = new(
                    attachment.Name,
                    attachment.ContentType,
                    BinaryData.FromBytes(attachment.Content)
                );
                message.Attachments.Add(item);
            }
        }

        if (emailMessage.Senders.ReplyTo?.Count > 0)
        {
            foreach (var replyTo in emailMessage.Senders.ReplyTo)
                message.ReplyTo.Add(new Ace.EmailAddress(replyTo.Address, replyTo.DisplayName));
        }

        return message;
    }

    /// <summary>
    /// Overrides the recipient of the email message if a recipient override is configured.
    /// </summary>
    /// <param name="emailMessage">The email message to modify.</param>
    /// <returns>The modified email message with the overridden recipient.</returns>
    private EmailMessage OverrideRecipient(EmailMessage emailMessage)
    {
        var recipientOverride = _options.Value.RecipientOverride;
        if (string.IsNullOrWhiteSpace(recipientOverride))
            return emailMessage;


        var originalRecipients = emailMessage.Recipients.ToString();
        var overrideAddress = new EmailAddress(recipientOverride);
        var htmlMessage = $"{emailMessage.Content.HtmlBody}<p>Original Recipients: {originalRecipients}</p>";

        LogRecipientOverride(_logger, recipientOverride, originalRecipients);

        return emailMessage with
        {
            Recipients = new EmailRecipients { To = [overrideAddress] },
            Content = emailMessage.Content with { HtmlBody = htmlMessage },
        };
    }

    [LoggerMessage(1, LogLevel.Debug, "Sending email to '{Recipients}' with subject '{Subject}' using Azure Communication")]
    static partial void LogSendingEmailAzure(ILogger logger, string recipients, string subject);

    [LoggerMessage(2, LogLevel.Information, "Sent email to '{Recipients}' with subject '{Subject}': {OperationId}")]
    static partial void LogEmailSentAzure(ILogger logger, string recipients, string subject, string? operationId);

    [LoggerMessage(3, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}'; Status: {Status}")]
    static partial void LogEmailSendErrorAzure(ILogger logger, string recipients, string subject, Ace.EmailSendStatus status);

    [LoggerMessage(4, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}': {ErrorMessage}")]
    static partial void LogEmailSendExceptionAzure(ILogger logger, string recipients, string subject, string errorMessage, Exception exception);

    [LoggerMessage(5, LogLevel.Information, "Overriding email recipient to '{RecipientOverride}'; Original Recipients: {OriginalRecipients}")]
    static partial void LogRecipientOverride(ILogger logger, string recipientOverride, string originalRecipients);
}
