// Ignore Spelling: Bcc

namespace Arbiter.Communication.Email;

/// <summary>
/// Provides a fluent builder for constructing <see cref="EmailMessage"/> instances, including sender, recipients, content, headers, and attachments.
/// </summary>
/// <remarks>
/// Use <see cref="EmailBuilder"/> to incrementally build an <see cref="EmailMessage"/> with a clear and readable API.
/// This builder enforces that a sender, subject, and at least one recipient are set before building the message.
/// </remarks>
public class EmailBuilder
{
    private readonly Dictionary<string, string> _headers = [];
    private readonly List<EmailAttachment> _attachments = [];
    private readonly List<EmailAddress> _replyTo = [];
    private readonly List<EmailAddress> _to = [];
    private readonly List<EmailAddress> _cc = [];
    private readonly List<EmailAddress> _bcc = [];
    private EmailAddress _from;
    private string _subject = string.Empty;
    private string _htmlBody = string.Empty;
    private string? _textBody;

    /// <summary>
    /// Sets the sender address for the email.
    /// </summary>
    /// <param name="address">The sender's email address.</param>
    /// <param name="displayName">The optional display name for the sender.</param>
    /// <param name="condition">An optional predicate to determine if the sender should be set.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> is null or empty.</exception>
    public EmailBuilder From(
        string? address,
        string? displayName = null,
        Func<string?, string?, bool>? condition = null)
    {
        if (condition is not null && !condition(address, displayName))
            return this;

        ArgumentException.ThrowIfNullOrEmpty(address);

        _from = new EmailAddress(address, displayName);
        return this;
    }

    /// <summary>
    /// Adds a reply-to address to the email.
    /// </summary>
    /// <param name="address">The reply-to email address.</param>
    /// <param name="displayName">The optional display name for the reply-to address.</param>
    /// <param name="condition">An optional predicate to determine if the reply-to should be added.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> is null or empty.</exception>
    public EmailBuilder ReplyTo(
        string? address,
        string? displayName = null,
        Func<string?, string?, bool>? condition = null)
    {
        if (condition is not null && !condition(address, displayName))
            return this;

        ArgumentException.ThrowIfNullOrEmpty(address);

        _replyTo.Add(new EmailAddress(address, displayName));
        return this;
    }

    /// <summary>
    /// Adds a recipient to the "To" field of the email.
    /// </summary>
    /// <param name="address">The recipient's email address.</param>
    /// <param name="displayName">The optional display name for the recipient.</param>
    /// <param name="condition">An optional predicate to determine if the recipient should be added.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> is null or empty.</exception>
    public EmailBuilder To(
        string address,
        string? displayName = null,
        Func<string?, string?, bool>? condition = null)
    {
        if (condition is not null && !condition(address, displayName))
            return this;

        ArgumentException.ThrowIfNullOrEmpty(address);

        _to.Add(new EmailAddress(address, displayName));
        return this;
    }

    /// <summary>
    /// Adds a recipient to the "Cc" field of the email.
    /// </summary>
    /// <param name="address">The Cc recipient's email address.</param>
    /// <param name="displayName">The optional display name for the Cc recipient.</param>
    /// <param name="condition">An optional predicate to determine if the Cc recipient should be added.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> is null or empty.</exception>
    public EmailBuilder Cc(
        string address,
        string? displayName = null,
        Func<string?, string?, bool>? condition = null)
    {
        if (condition is not null && !condition(address, displayName))
            return this;

        ArgumentException.ThrowIfNullOrEmpty(address);

        _cc.Add(new EmailAddress(address, displayName));
        return this;
    }

    /// <summary>
    /// Adds a recipient to the "Bcc" field of the email.
    /// </summary>
    /// <param name="address">The Bcc recipient's email address.</param>
    /// <param name="displayName">The optional display name for the Bcc recipient.</param>
    /// <param name="condition">An optional predicate to determine if the Bcc recipient should be added.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> is null or empty.</exception>
    public EmailBuilder Bcc(
        string address,
        string? displayName = null,
        Func<string?, string?, bool>? condition = null)
    {
        if (condition is not null && !condition(address, displayName))
            return this;

        ArgumentException.ThrowIfNullOrEmpty(address);

        _bcc.Add(new EmailAddress(address, displayName));
        return this;
    }

    /// <summary>
    /// Sets the subject of the email.
    /// </summary>
    /// <param name="subject">The subject text.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="subject"/> is null or empty.</exception>
    public EmailBuilder Subject(string subject)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);

        _subject = subject;
        return this;
    }

    /// <summary>
    /// Sets the HTML body content of the email.
    /// </summary>
    /// <param name="htmlBody">The HTML body content.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="htmlBody"/> is null or empty.</exception>
    public EmailBuilder HtmlBody(string htmlBody)
    {
        ArgumentException.ThrowIfNullOrEmpty(htmlBody);

        _htmlBody = htmlBody;
        return this;
    }

    /// <summary>
    /// Sets the plain text body content of the email.
    /// </summary>
    /// <param name="textBody">The plain text body content. May be <see langword="null"/>.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    public EmailBuilder TextBody(string? textBody)
    {
        _textBody = textBody;
        return this;
    }

    /// <summary>
    /// Adds a custom header to the email.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
    public EmailBuilder AddHeader(string name, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _headers[name] = value;
        return this;
    }

    /// <summary>
    /// Adds an attachment to the email.
    /// </summary>
    /// <param name="name">The name of the attachment file.</param>
    /// <param name="contentType">The MIME type of the attachment.</param>
    /// <param name="content">The binary content of the attachment.</param>
    /// <param name="contentId">The optional content ID for inline attachments.</param>
    /// <returns>The current <see cref="EmailBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> or <paramref name="contentType"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> is null.</exception>
    public EmailBuilder AddAttachment(
        string name,
        string contentType,
        byte[] content,
        string? contentId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(contentType);
        ArgumentNullException.ThrowIfNull(content);

        var attachment = new EmailAttachment(name, contentType, content, contentId);
        _attachments.Add(attachment);

        return this;
    }

    /// <summary>
    /// Builds and returns an instance of <see cref="EmailSenders"/> containing the configured sender and reply-to email addresses.
    /// </summary>
    /// <remarks>
    /// Use this method to create an <see cref="EmailSenders"/> instance with the current configuration.
    /// Ensure that the sender and reply-to addresses have been properly set before calling this method.
    /// </remarks>
    /// <returns>An <see cref="EmailSenders"/> object initialized with the sender and reply-to email addresses.</returns>
    public EmailSenders BuildSenders() => new(_from, _replyTo);

    /// <summary>
    /// Constructs an <see cref="EmailRecipients"/> object using the current recipient lists.
    /// </summary>
    /// <returns>
    /// An <see cref="EmailRecipients"/> instance containing the current "To", "Cc", and "Bcc" recipient lists.
    /// </returns>
    public EmailRecipients BuildRecipients() => new(_to, _cc, _bcc);

    /// <summary>
    /// Builds and returns an <see cref="EmailContent"/> object containing the email's subject, HTML body, and text body.
    /// </summary>
    /// <returns>
    /// An <see cref="EmailContent"/> instance initialized with the current subject, HTML body, and text body values.
    /// </returns>
    public EmailContent BuildContent() => new(_subject, _htmlBody, _textBody);

    /// <summary>
    /// Builds and returns the <see cref="EmailMessage"/> instance using the configured values.
    /// </summary>
    /// <returns>The constructed <see cref="EmailMessage"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the sender address, subject, or at least one recipient (To, Cc, or Bcc) is not set.
    /// </exception>
    public EmailMessage BuildMessage()
    {
        if (string.IsNullOrEmpty(_from.Address))
            throw new InvalidOperationException("Sender address must be set before building the email message.");

        if (_to.Count == 0 && _cc.Count == 0 && _bcc.Count == 0)
            throw new InvalidOperationException("At least one recipient must be specified (To, Cc, or Bcc) before building the email message.");

        if (string.IsNullOrEmpty(_subject))
            throw new InvalidOperationException("Email subject must be set before building the email message.");

        var senders = new EmailSenders(_from, _replyTo);
        var recipients = new EmailRecipients(_to, _cc, _bcc);
        var content = new EmailContent(_subject, _htmlBody, _textBody);

        return new EmailMessage(senders, recipients, content, _headers, _attachments);
    }

    /// <summary>
    /// Creates a new <see cref="EmailBuilder"/> instance, optionally setting the sender address and display name.
    /// </summary>
    /// <param name="fromAddress">The sender's email address. If <see langword="null"/>, the sender is not set.</param>
    /// <param name="fromName">The optional display name for the sender.</param>
    /// <returns>A new <see cref="EmailBuilder"/> instance.</returns>
    public static EmailBuilder Create(string? fromAddress = null, string? fromName = null)
    {
        var builder = new EmailBuilder();

        if (!string.IsNullOrEmpty(fromAddress))
            builder.From(fromAddress, fromName);

        return builder;
    }
}
