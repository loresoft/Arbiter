using MailKit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

namespace Arbiter.Communication.Email;

/// <summary>
/// Provides an implementation of <see cref="IEmailDeliveryService"/> that delivers emails using SMTP via MailKit.
/// </summary>
/// <remarks>
/// This service composes and sends email messages using SMTP configuration options. It supports attachments, custom headers,
/// and logs protocol details for diagnostics. The service will attempt to authenticate if credentials are provided and will accept all SSL certificates.
/// </remarks>
public sealed class SmtpEmailDeliverService : IEmailDeliveryService
{
    private readonly ILogger<SmtpEmailDeliverService> _logger;
    private readonly IOptions<EmailConfiguration> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailDeliverService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="emailOptions">The email configuration options.</param>
    public SmtpEmailDeliverService(ILogger<SmtpEmailDeliverService> logger, IOptions<EmailConfiguration> emailOptions)
    {
        _logger = logger;
        _options = emailOptions;
    }

    /// <summary>
    /// Sends the specified <see cref="EmailMessage"/> asynchronously using SMTP.
    /// </summary>
    /// <param name="emailMessage">The email message to send, including sender, recipients, content, headers, and attachments.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating whether the email was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0004:Use Task.ConfigureAwait", Justification = "<Pending>")]
    public async Task<EmailResult> Send(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        var emailServer = _options.Value;

        var host = emailServer.Server;
        var port = emailServer.Port;
        var useSsl = emailServer.UseSSL;

        var userName = emailServer.UserName;
        var password = emailServer.Password;

        var mimeMessage = ConvertMessage(emailMessage);

        var to = mimeMessage.To.ToString();
        var subject = mimeMessage.Subject[..20];

        await using var logStream = new MemoryStream();

        try
        {
            // make sure there is a from address
            if (mimeMessage.From.Count == 0)
                mimeMessage.From.Add(CreateAddress(emailServer.FromAddress, emailServer.FromName));

            using var logger = new ProtocolLogger(logStream, leaveOpen: true);
            using var client = new MailKit.Net.Smtp.SmtpClient(logger);

            // accept all SSL certificates
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(host, port, useSsl, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(password))
                await client.AuthenticateAsync(userName, password, cancellationToken).ConfigureAwait(false);

            await client.SendAsync(mimeMessage, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Sent email to '{ToAddress}' with subject '{EmailSubject}' using Host '{SmtpHost}'", to, subject, host);
            return EmailResult.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to '{ToAddress}' with subject '{EmailSubject}' using Host '{SmtpHost}'", to, subject, host);
            return EmailResult.Fail("An error occurred while sending the email", ex);
        }
        finally
        {
            await WriteLogStream(logStream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes the SMTP protocol log to the logger if trace logging is enabled.
    /// </summary>
    /// <param name="logStream">The memory stream containing the protocol log.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task WriteLogStream(MemoryStream logStream, CancellationToken cancellationToken)
    {
        if (logStream.Length == 0 || !_logger.IsEnabled(LogLevel.Trace))
            return;

        logStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(logStream);
        var logContent = await reader
            .ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogTrace("SMTP Protocol Log:\n{LogContent}", logContent);
    }

    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a <see cref="MimeMessage"/> for SMTP delivery.
    /// </summary>
    /// <param name="emailMessage">The email message to convert.</param>
    /// <returns>A <see cref="MimeMessage"/> representing the email message.</returns>
    private static MimeMessage ConvertMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();

        message.Subject = emailMessage.Content.Subject;
        message.From.Add(CreateAddress(emailMessage.Senders.From));

        if (emailMessage.Senders.ReplyTo?.Count > 0)
        {
            foreach (var replyTo in emailMessage.Senders.ReplyTo)
                message.ReplyTo.Add(CreateAddress(replyTo));
        }

        foreach (var to in emailMessage.Recipients.To)
        {
            message.To.Add(CreateAddress(to));
        }

        if (emailMessage.Recipients.Cc?.Count > 0)
        {
            foreach (var cc in emailMessage.Recipients.Cc)
                message.Cc.Add(CreateAddress(cc));
        }

        if (emailMessage.Recipients.Bcc?.Count > 0)
        {
            foreach (var bcc in emailMessage.Recipients.Bcc)
                message.Bcc.Add(CreateAddress(bcc));
        }

        if (emailMessage.Headers?.Count > 0)
        {
            foreach (var header in emailMessage.Headers)
                message.Headers.Add(header.Key, header.Value);
        }

        var builder = new BodyBuilder();
        builder.TextBody = emailMessage.Content.TextBody;
        builder.HtmlBody = emailMessage.Content.HtmlBody;

        if (emailMessage.Attachments != null)
        {
            foreach (var attachment in emailMessage.Attachments)
            {
                ContentType.TryParse(attachment.ContentType, out var contentType);
                var contentBytes = attachment.Content.ToArray();

                builder.Attachments.Add(attachment.Name, contentBytes, contentType);
            }
        }

        message.Body = builder.ToMessageBody();

        return message;
    }

    /// <summary>
    /// Creates a <see cref="MailboxAddress"/> from an <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="sender">The email address and optional display name.</param>
    /// <returns>A <see cref="MailboxAddress"/> instance.</returns>
    private static MailboxAddress CreateAddress(EmailAddress sender)
        => new(sender.DisplayName ?? sender.Address, sender.Address);

    /// <summary>
    /// Creates a <see cref="MailboxAddress"/> from an address and optional display name.
    /// </summary>
    /// <param name="address">The email address.</param>
    /// <param name="name">The optional display name.</param>
    /// <returns>A <see cref="MailboxAddress"/> instance.</returns>
    private static MailboxAddress CreateAddress(string address, string? name)
        => new(name ?? address, address);
}
