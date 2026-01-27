using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace Arbiter.Communication.Twilio;

/// <summary>
/// Provides an implementation of <see cref="IEmailDeliveryService"/> that delivers emails using SendGrid.
/// </summary>
public partial class SendGridEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger<SendGridEmailDeliveryService> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly IOptions<EmailConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendGridEmailDeliveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="sendGridClient">The SendGrid client used to send emails.</param>
    /// <param name="options">The email configuration options.</param>
    public SendGridEmailDeliveryService(
        ILogger<SendGridEmailDeliveryService> logger,
        ISendGridClient sendGridClient,
        IOptions<EmailConfiguration> options)
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
        _options = options;
    }

    /// <summary>
    /// Sends the specified <see cref="EmailMessage"/> asynchronously using SendGrid.
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

        LogSendingEmailSendGrid(_logger, recipients, truncatedSubject);

        try
        {
            emailMessage = OverrideRecipient(emailMessage);
            var message = ConvertMessage(emailMessage);

            var response = await _sendGridClient
                .SendEmailAsync(message, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                LogEmailSentSendGrid(_logger, recipients, truncatedSubject);
                return EmailResult.Success("Email sent successfully.");
            }

            var body = await response.Body.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            LogEmailSendErrorSendGrid(_logger, recipients, truncatedSubject, response.StatusCode, body);
            return EmailResult.Fail($"Email send failed with status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            LogEmailSendExceptionSendGrid(_logger, recipients, truncatedSubject, ex.Message, ex);
            return EmailResult.Fail("An error occurred while sending the email", ex);
        }
    }

    private static SendGridMessage ConvertMessage(EmailMessage emailMessage)
    {
        var message = new SendGridMessage
        {
            From = ConvertEmail(emailMessage.Senders.From),
            Subject = emailMessage.Content.Subject,
            HtmlContent = emailMessage.Content.HtmlBody,
            PlainTextContent = emailMessage.Content.TextBody,
        };

        // only one reply-to address is supported by SendGrid
        if (emailMessage.Senders.ReplyTo?.Count > 0)
        {
            var emailAddress = ConvertEmail(emailMessage.Senders.ReplyTo[0]);
            message.SetReplyTo(emailAddress);
        }

        // recipients are handled via personalizations
        var personalization = CreatePersonalization(emailMessage, message);

        message.Personalizations ??= [];
        message.Personalizations.Add(personalization);

        if (emailMessage.Attachments?.Count > 0)
        {
            foreach (var attachment in emailMessage.Attachments)
            {
                var mailAttachment = new SendGrid.Helpers.Mail.Attachment
                {
                    Content = Convert.ToBase64String(attachment.Content),
                    Filename = attachment.Name,
                    Type = attachment.ContentType,
                };
                message.AddAttachment(mailAttachment);
            }
        }

        return message;
    }

    private static Personalization CreatePersonalization(EmailMessage emailMessage, SendGridMessage message)
    {
        var personalization = new Personalization();

        foreach (var recipient in emailMessage.Recipients.To)
        {
            var emailAddress = ConvertEmail(recipient);
            personalization.Tos ??= [];
            personalization.Tos.Add(emailAddress);
        }

        if (emailMessage.Recipients.Cc?.Count > 0)
        {
            foreach (var cc in emailMessage.Recipients.Cc)
            {
                var emailAddress = ConvertEmail(cc);

                // SendGrid requires at least one "To" recipient
                if (personalization.Tos == null || personalization.Tos.Count == 0)
                {
                    personalization.Tos ??= [];
                    personalization.Tos.Add(emailAddress);
                }
                else
                {
                    personalization.Ccs ??= [];
                    personalization.Ccs.Add(emailAddress);
                }
            }
        }

        if (emailMessage.Recipients.Bcc?.Count > 0)
        {
            foreach (var bcc in emailMessage.Recipients.Bcc)
            {
                var emailAddress = ConvertEmail(bcc);

                // SendGrid requires at least one "To" recipient
                if (personalization.Tos == null || personalization.Tos.Count == 0)
                {
                    personalization.Tos ??= [];
                    personalization.Tos.Add(emailAddress);
                }
                else
                {
                    personalization.Bccs ??= [];
                    personalization.Bccs.Add(emailAddress);
                }
            }
        }

        if (emailMessage.Headers?.Count > 0)
        {
            personalization.Headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in emailMessage.Headers)
                personalization.Headers[header.Key] = header.Value;
        }

        return personalization;
    }

    private static SendGrid.Helpers.Mail.EmailAddress ConvertEmail(Email.EmailAddress emailAddress)
        => new(emailAddress.Address, emailAddress.DisplayName);

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
        var overrideAddress = new Email.EmailAddress(recipientOverride);
        var htmlMessage = $"{emailMessage.Content.HtmlBody}<p>Original Recipients: {originalRecipients}</p>";

        LogRecipientOverride(_logger, recipientOverride, originalRecipients);

        return emailMessage with
        {
            Recipients = new EmailRecipients { To = [overrideAddress] },
            Content = emailMessage.Content with { HtmlBody = htmlMessage },
        };
    }


    [LoggerMessage(1, LogLevel.Debug, "Sending email to '{Recipients}' with subject '{Subject}' using SendGrid")]
    static partial void LogSendingEmailSendGrid(ILogger logger, string recipients, string subject);

    [LoggerMessage(2, LogLevel.Information, "Sent email to '{Recipients}' with subject '{Subject}'")]
    static partial void LogEmailSentSendGrid(ILogger logger, string recipients, string subject);

    [LoggerMessage(3, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}'; Status: {Status};\n{Body}")]
    static partial void LogEmailSendErrorSendGrid(ILogger logger, string recipients, string subject, System.Net.HttpStatusCode status, string? body);

    [LoggerMessage(4, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}': {ErrorMessage}")]
    static partial void LogEmailSendExceptionSendGrid(ILogger logger, string recipients, string subject, string errorMessage, Exception exception);

    [LoggerMessage(5, LogLevel.Information, "Overriding email recipient to '{RecipientOverride}'; Original Recipients: {OriginalRecipients}")]
    static partial void LogRecipientOverride(ILogger logger, string recipientOverride, string originalRecipients);
}
