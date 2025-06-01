namespace Arbiter.Communication.Email;

/// <summary>
/// Provides methods for sending templated emails using a model and recipient information.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Sends an email using a named template, binding the specified model to the template and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="templateName">The name of the template to use for the email.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email, including To, Cc, and Bcc addresses.</param>
    /// <param name="senders">The sender and optional reply-to addresses for the email. If null, default senders may be used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    Task<EmailResult> Send<TModel>(
        string templateName,
        TModel emailModel,
        EmailRecipients recipients,
        EmailSenders? senders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using the provided <see cref="EmailTemplate"/>, binding the specified model and delivering it to the given recipients.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for template binding.</typeparam>
    /// <param name="emailTemplate">The email template to use for the message.</param>
    /// <param name="emailModel">The model containing data to bind to the template.</param>
    /// <param name="recipients">The recipients of the email, including To, Cc, and Bcc addresses.</param>
    /// <param name="senders">The sender and optional reply-to addresses for the email. If null, default senders may be used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating the outcome of the send operation,
    /// including success status, message, and any exception details.
    /// </returns>
    Task<EmailResult> Send<TModel>(
        EmailTemplate emailTemplate,
        TModel emailModel,
        EmailRecipients recipients,
        EmailSenders? senders = null,
        CancellationToken cancellationToken = default);
}
