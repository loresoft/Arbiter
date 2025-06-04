using System.Reflection;

using Arbiter.Communication.Extensions;
using Arbiter.Communication.Template;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Communication.Email;

/// <summary>
/// Provides functionality for sending templated emails by applying models to templates and delivering the result to recipients.
/// </summary>
/// <remarks>
/// This service supports loading templates from embedded resources, applying model data, and sending emails using the configured delivery service.
/// </remarks>
public class EmailTemplateService : IEmailTemplateService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic and error messages.</param>
    /// <param name="options">The email options containing sender and template configuration.</param>
    /// <param name="templateService">The template service for applying models to templates.</param>
    /// <param name="deliveryService">The service responsible for delivering emails.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="logger"/>, <paramref name="options"/>, <paramref name="templateService"/>, or <paramref name="deliveryService"/> is <see langword="null"/>.
    /// </exception>
    public EmailTemplateService(
        ILogger<EmailTemplateService> logger,
        IOptions<EmailConfiguration> options,
        ITemplateService templateService,
        IEmailDeliveryService deliveryService)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        TemplateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        DeliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
    }

    /// <summary>
    /// Gets the logger used for diagnostic and error messages.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the email configuration options.
    /// </summary>
    protected IOptions<EmailConfiguration> Options { get; }

    /// <summary>
    /// Gets the template service for applying models to templates.
    /// </summary>
    protected ITemplateService TemplateService { get; }

    /// <summary>
    /// Gets the service responsible for delivering emails.
    /// </summary>
    protected IEmailDeliveryService DeliveryService { get; }

    /// <summary>
    /// Sends an email using a named template, binding the specified model to the template and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="templateName">The name of the template to use for the email.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email, including To, Cc, and Bcc addresses.</param>
    /// <param name="senders">The sender and optional reply-to addresses for the email. If <see langword="null"/>, default senders from configuration are used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="templateName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="emailModel"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during template loading or email sending.</exception>
    public async Task<EmailResult> Send<TModel>(
        string templateName,
        TModel emailModel,
        EmailRecipients recipients,
        EmailSenders? senders = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNull(emailModel);

        var options = Options.Value;

        try
        {
            var templateAssembly = options.TemplateAssembly ?? Assembly.GetExecutingAssembly();
            var resourceName = !string.IsNullOrEmpty(options.TemplateResourceFormat)
                ? string.Format(options.TemplateResourceFormat, templateName)
                : templateName;

            if (!TemplateService.TryGetResourceTemplate<EmailTemplate>(templateAssembly, resourceName, out var emailTemplate))
            {
                Logger.LogError("Could not find email template: {TemplateName}", templateName);
                return EmailResult.Fail($"Could not find template '{templateName}'");
            }

            return await Send(emailTemplate, emailModel, recipients, senders, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending email: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Sends an email using the provided <see cref="EmailTemplate"/>, binding the specified model and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="emailTemplate">The email template to use for the message.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email, including To, Cc, and Bcc addresses.</param>
    /// <param name="senders">The sender and optional reply-to addresses for the email. If <see langword="null"/>, default senders from configuration are used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="emailModel"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during template application or email sending.</exception>
    public async Task<EmailResult> Send<TModel>(
        EmailTemplate emailTemplate,
        TModel emailModel,
        EmailRecipients recipients,
        EmailSenders? senders = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emailModel);

        var options = Options.Value;

        try
        {
            var subject = TemplateService.ApplyTemplate(emailTemplate.Subject, emailModel);
            var htmlBody = TemplateService.ApplyTemplate(emailTemplate.HtmlBody, emailModel);
            var textBody = TemplateService.ApplyTemplate(emailTemplate.TextBody, emailModel);

            var fromName = options.FromName;
            var fromEmail = options.FromAddress;

            var localSenders = senders ?? new EmailSenders(new EmailAddress(fromEmail, fromName));

            if (localSenders.From.Address.IsNullOrWhiteSpace())
            {
                Logger.LogError("From address is not configured in EmailOptions.");
                return EmailResult.Fail("From address is not configured.");
            }

            var content = new EmailContent(subject, htmlBody, textBody);
            var message = new EmailMessage(localSenders, recipients, content);

            return await DeliveryService.Send(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error Sending Email: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
