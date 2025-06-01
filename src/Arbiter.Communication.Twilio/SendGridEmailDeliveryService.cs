using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;

using Microsoft.Extensions.Logging;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace Arbiter.Communication.Twilio;

/// <summary>
/// Provides an implementation of <see cref="IEmailDeliveryService"/> that delivers emails using SendGrid.
/// </summary>
public class SendGridEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger<SendGridEmailDeliveryService> _logger;
    private readonly ISendGridClient _sendGridClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendGridEmailDeliveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="sendGridClient">The SendGrid client used to send emails.</param>
    public SendGridEmailDeliveryService(ILogger<SendGridEmailDeliveryService> logger, ISendGridClient sendGridClient)
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
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
        var truncatedSubject = emailMessage.Content.Subject.Truncate(20);

        _logger.LogDebug("Sending email to '{Recipients}' with subject '{Subject}' using SendGrid", recipients, truncatedSubject);

        try
        {
            var message = ConvertMessage(emailMessage);

            var response = await _sendGridClient
                .SendEmailAsync(message, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sent email to '{Recipients}' with subject '{Subject}'", recipients, truncatedSubject);
                return EmailResult.Success("Email sent successfully.");
            }

            _logger.LogError("Error sending email to '{Recipients}' with subject '{Subject}'; Status: {Status}", recipients, truncatedSubject, response.StatusCode);
            return EmailResult.Fail($"Email send failed with status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to '{Recipients}' with subject '{Subject}': {ErrorMessage}", recipients, truncatedSubject, ex.Message);
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

        foreach (var recipient in emailMessage.Recipients.To)
            message.AddTo(recipient.Address, recipient.DisplayName);

        if (emailMessage.Recipients.Cc?.Count > 0)
        {
            foreach (var cc in emailMessage.Recipients.Cc)
                message.AddCc(cc.Address, cc.DisplayName);
        }

        if (emailMessage.Recipients.Bcc?.Count > 0)
        {
            foreach (var bcc in emailMessage.Recipients.Bcc)
                message.AddBcc(bcc.Address, bcc.DisplayName);
        }

        if (emailMessage.Headers?.Count > 0)
        {
            foreach (var header in emailMessage.Headers)
                message.AddHeader(header.Key, header.Value);
        }

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

    private static SendGrid.Helpers.Mail.EmailAddress ConvertEmail(Email.EmailAddress emailAddress)
        => new(emailAddress.Address, emailAddress.DisplayName);
}
