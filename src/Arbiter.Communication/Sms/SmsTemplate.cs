namespace Arbiter.Communication.Sms;

/// <summary>
/// Represents an SMS template containing the message body.
/// </summary>
/// <param name="Message">The template message content.</param>
public readonly record struct SmsTemplate(
    string Message
);
