using System.Reflection;

using Arbiter.Communication.Template;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Communication.Sms;

/// <summary>
/// Provides functionality for sending templated SMS messages by applying models to templates and delivering the result to recipients.
/// </summary>
/// <remarks>
/// This service supports loading SMS templates from embedded resources, applying model data, and sending SMS messages using the configured delivery service.
/// </remarks>
public class SmsTemplateService : ISmsTemplateService
{
    private readonly ILogger<SmsTemplateService> _logger;
    private readonly IOptions<SmsConfiguration> _options;
    private readonly ITemplateService _templateService;
    private readonly ISmsDeliveryService _deliveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsTemplateService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic and error messages.</param>
    /// <param name="options">The SMS configuration options containing sender and template configuration.</param>
    /// <param name="templateService">The template service for applying models to templates.</param>
    /// <param name="deliveryService">The service responsible for delivering SMS messages.</param>
    public SmsTemplateService(
        ILogger<SmsTemplateService> logger,
        IOptions<SmsConfiguration> options,
        ITemplateService templateService,
        ISmsDeliveryService deliveryService)
    {
        _logger = logger;
        _options = options;
        _templateService = templateService;
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Sends an SMS using a named template, binding the specified model to the template and delivering it to the given recipient.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="templateName">The name of the template to use for the SMS.</param>
    /// <param name="model">The model containing data to bind to the template.</param>
    /// <param name="recipient">The recipient's phone number in E.164 format.</param>
    /// <param name="sender">The sender's phone number in E.164 format. If null, the default sender from configuration is used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="SmsResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="templateName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
    public async Task<SmsResult> Send<TModel>(
        string templateName,
        TModel model,
        string recipient,
        string? sender = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNull(model);

        var options = _options.Value;

        try
        {
            var templateAssembly = options.TemplateAssembly ?? Assembly.GetExecutingAssembly();
            var resourceName = !string.IsNullOrEmpty(options.TemplateResourceFormat)
                ? string.Format(options.TemplateResourceFormat, templateName)
                : templateName;

            var template = _templateService.GetResourceTemplate<SmsTemplate?>(templateAssembly, resourceName);
            if (template == null)
            {
                _logger.LogError("Could not find template: {TemplateName}", templateName);
                return SmsResult.Fail($"Could not find template '{templateName}'");
            }

            return await Send(template.Value, model, recipient, sender, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Sends an SMS using the provided <see cref="SmsTemplate"/>, binding the specified model and delivering it to the given recipient.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="template">The SMS template to use for the message.</param>
    /// <param name="model">The model containing data to bind to the template.</param>
    /// <param name="recipient">The recipient's phone number in E.164 format.</param>
    /// <param name="sender">The sender's phone number in E.164 format. If null, the default sender from configuration is used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="SmsResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
    public async Task<SmsResult> Send<TModel>(
        SmsTemplate template,
        TModel model,
        string recipient,
        string? sender = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var options = _options.Value;

        try
        {
            var message = _templateService.ApplyTemplate(template.Message, model);
            sender ??= options.SenderNumber;

            if (string.IsNullOrEmpty(sender))
            {
                _logger.LogError("Sender number is not configured in SmsConfiguration.");
                return SmsResult.Fail("Sender number is not configured.");
            }

            var smsMessage = new SmsMessage(sender, recipient, message);

            return await _deliveryService
                .Send(smsMessage, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Sending SMS: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
