namespace Arbiter.Communication.Sms;

/// <summary>
/// Provides methods for sending templated SMS messages using a model and recipient information.
/// </summary>
public interface ISmsTemplateService
{
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
    Task<SmsResult> Send<TModel>(
        string templateName,
        TModel model,
        string recipient,
        string? sender = null,
        CancellationToken cancellationToken = default);

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
    Task<SmsResult> Send<TModel>(
        SmsTemplate template,
        TModel model,
        string recipient,
        string? sender = null,
        CancellationToken cancellationToken = default);
}
