using System.Reflection;

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
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly IOptions<EmailConfiguration> _options;
    private readonly ITemplateService _templateService;
    private readonly IEmailDeliveryService _deliveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic and error messages.</param>
    /// <param name="options">The email options containing sender and template configuration.</param>
    /// <param name="templateService">The template service for applying models to templates.</param>
    /// <param name="deliveryService">The service responsible for delivering emails.</param>
    public EmailTemplateService(
        ILogger<EmailTemplateService> logger,
        IOptions<EmailConfiguration> options,
        ITemplateService templateService,
        IEmailDeliveryService deliveryService)
    {
        _logger = logger;
        _options = options;
        _templateService = templateService;
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Sends an email using a named template, binding the specified model to the template and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="templateName">The name of the template to use for the email.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="templateName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="emailModel"/> is null.</exception>
    public async Task<EmailResult> Send<TModel>(
        string templateName,
        TModel emailModel,
        EmailRecipients recipients,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNull(emailModel);

        var options = _options.Value;

        try
        {
            var templateAssembly = options.TemplateAssembly ?? Assembly.GetExecutingAssembly();
            var resourceName = !string.IsNullOrEmpty(options.TemplateResourceFormat)
                ? string.Format(options.TemplateResourceFormat, templateName)
                : templateName;

            var emailTemplate = _templateService.GetResourceTemplate<EmailTemplate?>(templateAssembly, resourceName);
            if (emailTemplate == null)
            {
                _logger.LogError("Could not find email template: {TemplateName}", templateName);
                return EmailResult.Fail($"Could not find template '{templateName}'");
            }

            return await Send(emailTemplate.Value, emailModel, recipients, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Sends an email using the provided <see cref="EmailTemplate"/>, binding the specified model and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="emailTemplate">The email template to use for the message.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="emailModel"/> is null.</exception>
    public async Task<EmailResult> Send<TModel>(
        EmailTemplate emailTemplate,
        TModel emailModel,
        EmailRecipients recipients,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emailModel);

        var options = _options.Value;

        try
        {
            var subject = _templateService.ApplyTemplate(emailTemplate.Subject, emailModel);
            var htmlBody = _templateService.ApplyTemplate(emailTemplate.HtmlBody, emailModel);
            var textBody = _templateService.ApplyTemplate(emailTemplate.TextBody, emailModel);

            var fromName = options.FromName;
            var fromEmail = options.FromAddress;

            if (string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("From address is not configured in EmailOptions.");
                return EmailResult.Fail("From address is not configured.");
            }

            var senders = new EmailSenders(new EmailAddress(fromEmail, fromName));
            var content = new EmailContent(subject, htmlBody, textBody);
            var message = new EmailMessage(senders, recipients, content);

            return await _deliveryService.Send(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Sending Email: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
