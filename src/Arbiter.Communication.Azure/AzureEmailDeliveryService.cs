using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;

using Azure;

using Microsoft.Extensions.Logging;

using Ace = Azure.Communication.Email;

namespace Arbiter.Communication.Azure;

/// <summary>
/// Provides an implementation of <see cref="IEmailDeliveryService"/> that delivers emails using Azure Communication Services.
/// </summary>
public partial class AzureEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger<AzureEmailDeliveryService> _logger;
    private readonly Ace.EmailClient _emailClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEmailDeliveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="emailClient">The Azure Communication Services email client.</param>
    public AzureEmailDeliveryService(ILogger<AzureEmailDeliveryService> logger, Ace.EmailClient emailClient)
    {
        _logger = logger;
        _emailClient = emailClient;
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
        var truncatedSubject = emailMessage.Content.Subject.Truncate(20);

        LogSendingEmailAzure(_logger, recipients, truncatedSubject);

        try
        {
            Ace.EmailMessage message = ConvertMessage(emailMessage);

            var sendOperation = await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken).ConfigureAwait(false);

            if (sendOperation.Value.Status == Ace.EmailSendStatus.Succeeded)
            {
                LogEmailSentAzure(_logger, recipients, truncatedSubject);
                return EmailResult.Success("Email sent successfully.");
            }

            LogEmailSendErrorAzure(_logger, recipients, truncatedSubject, sendOperation.Value.Status);
            return EmailResult.Fail($"Email send failed with status {sendOperation.Value.Status}");
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


    [LoggerMessage(1, LogLevel.Debug, "Sending email to '{Recipients}' with subject '{Subject}' using Azure Communication")]
    static partial void LogSendingEmailAzure(ILogger logger, string recipients, string subject);

    [LoggerMessage(2, LogLevel.Information, "Sent email to '{Recipients}' with subject '{Subject}'")]
    static partial void LogEmailSentAzure(ILogger logger, string recipients, string subject);

    [LoggerMessage(3, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}'; Status: {Status}")]
    static partial void LogEmailSendErrorAzure(ILogger logger, string recipients, string subject, Ace.EmailSendStatus status);

    [LoggerMessage(4, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}': {ErrorMessage}")]
    static partial void LogEmailSendExceptionAzure(ILogger logger, string recipients, string subject, string errorMessage, Exception exception);
}
