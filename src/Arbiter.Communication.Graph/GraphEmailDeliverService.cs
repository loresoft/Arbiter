using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using EmailAddress = Microsoft.Graph.Models.EmailAddress;

namespace Arbiter.Communication.Graph;

/// <summary>
/// Delivers email messages through Microsoft Graph.
/// </summary>
public partial class GraphEmailDeliverService : IEmailDeliveryService
{
    private readonly ILogger<GraphEmailDeliverService> _logger;
    private readonly GraphServiceClient _graphClient;
    private readonly IOptions<EmailConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphEmailDeliverService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and operational messages.</param>
    /// <param name="graphClient">The Microsoft Graph client used to send email messages.</param>
    /// <param name="options">The email configuration options.</param>
    public GraphEmailDeliverService(
        ILogger<GraphEmailDeliverService> logger,
        GraphServiceClient graphClient,
        IOptions<EmailConfiguration> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sends an email message using Microsoft Graph.
    /// </summary>
    /// <param name="emailMessage">The email message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="EmailResult"/> when the email is queued; otherwise, a failed result containing the exception.
    /// </returns>
    public async Task<EmailResult> Send(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        var recipients = emailMessage.Recipients.ToString();
        var truncatedSubject = emailMessage.Content.Subject.Truncate(100);

        LogSendingEmail(_logger, recipients, truncatedSubject);

        var fromAddress = _options.Value.FromAddress;
        if (string.IsNullOrWhiteSpace(fromAddress))
        {
            LogMissingFromAddress(_logger);
            return EmailResult.Fail("Email configuration is missing FromAddress");
        }

        try
        {
            emailMessage = OverrideRecipient(emailMessage);
            var message = ConvertMessage(emailMessage);

            var body = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = false,
            };

            await _graphClient
                .Users[fromAddress]
                .SendMail
                .PostAsync(body, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            LogEmailSent(_logger, recipients, truncatedSubject);
            return EmailResult.Success("Email queued successfully");

        }
        catch (Exception ex)
        {
            LogEmailSendException(_logger, recipients, truncatedSubject, ex.Message, ex);
            return EmailResult.Fail("An error occurred while sending the email", ex);
        }
    }

    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a Microsoft Graph <see cref="Message"/>.
    /// </summary>
    /// <param name="emailMessage">The source email message.</param>
    /// <returns>The converted Microsoft Graph message.</returns>
    private static Message ConvertMessage(EmailMessage emailMessage)
    {
        var bodyContent = emailMessage.Content.HtmlBody;
        var bodyType = BodyType.Html;

        if (string.IsNullOrWhiteSpace(bodyContent))
        {
            bodyContent = emailMessage.Content.TextBody;
            bodyType = BodyType.Text;
        }

        var message = new Message
        {
            Subject = emailMessage.Content.Subject,
            Body = new ItemBody
            {
                ContentType = bodyType,
                Content = bodyContent,
            },
        };

        if (emailMessage.Recipients.To.Count > 0)
            message.ToRecipients = [.. emailMessage.Recipients.To.Select(ConvertRecipient)];

        if (emailMessage.Recipients.Cc?.Count > 0)
            message.CcRecipients = [.. emailMessage.Recipients.Cc.Select(ConvertRecipient)];

        if (emailMessage.Recipients.Bcc?.Count > 0)
            message.BccRecipients = [.. emailMessage.Recipients.Bcc.Select(ConvertRecipient)];

        message.From = ConvertRecipient(emailMessage.Senders.From);
        message.Sender = ConvertRecipient(emailMessage.Senders.From);

        if (emailMessage.Senders.ReplyTo?.Count > 0)
            message.ReplyTo = [.. emailMessage.Senders.ReplyTo.Select(ConvertRecipient)];

        if (emailMessage.Attachments?.Count > 0)
        {
            foreach (var attachment in emailMessage.Attachments)
            {
                var fileAttachment = new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    Name = attachment.Name,
                    ContentType = attachment.ContentType,
                    ContentBytes = attachment.Content,
                };

                message.Attachments ??= [];
                message.Attachments.Add(fileAttachment);
            }
        }

        return message;
    }

    /// <summary>
    /// Converts an Arbiter email address to a Microsoft Graph recipient.
    /// </summary>
    /// <param name="e">The source email address.</param>
    /// <returns>The converted Microsoft Graph recipient.</returns>
    private static Recipient ConvertRecipient(Email.EmailAddress e)
    {
        return new Recipient
        {
            EmailAddress = new EmailAddress
            {
                Address = e.Address,
                Name = e.DisplayName,
            },
        };
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
        var overrideAddress = new Email.EmailAddress(recipientOverride);

        var htmlMessage = emailMessage.Content.HtmlBody.HasValue()
            ? $"{emailMessage.Content.HtmlBody}<p>Original Recipients: {originalRecipients}</p>"
            : emailMessage.Content.HtmlBody;

        var textMessage = emailMessage.Content.TextBody.HasValue()
            ? $"{emailMessage.Content.TextBody}\n\nOriginal Recipients: {originalRecipients}"
            : emailMessage.Content.TextBody;

        LogRecipientOverride(_logger, recipientOverride, originalRecipients);

        return emailMessage with
        {
            Recipients = new EmailRecipients { To = [overrideAddress] },
            Content = emailMessage.Content with { HtmlBody = htmlMessage, TextBody = textMessage },
        };
    }

    [LoggerMessage(1, LogLevel.Debug, "Sending email to '{Recipients}' with subject '{Subject}' using Microsoft Graph")]
    static partial void LogSendingEmail(ILogger logger, string recipients, string subject);

    [LoggerMessage(2, LogLevel.Information, "Sent email to '{Recipients}' with subject '{Subject}'")]
    static partial void LogEmailSent(ILogger logger, string recipients, string subject);

    [LoggerMessage(3, LogLevel.Error, "Email configuration is missing FromAddress")]
    static partial void LogMissingFromAddress(ILogger logger);

    [LoggerMessage(4, LogLevel.Error, "Error sending email to '{Recipients}' with subject '{Subject}': {ErrorMessage}")]
    static partial void LogEmailSendException(ILogger logger, string recipients, string subject, string errorMessage, Exception exception);

    [LoggerMessage(5, LogLevel.Information, "Overriding email recipient to '{RecipientOverride}'; Original Recipients: {OriginalRecipients}")]
    static partial void LogRecipientOverride(ILogger logger, string recipientOverride, string originalRecipients);

}
