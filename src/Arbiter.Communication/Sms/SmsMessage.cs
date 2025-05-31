namespace Arbiter.Communication.Sms;

/// <summary>
/// Represents an SMS message with sender, recipient, and message content.
/// </summary>
/// <param name="Sender">The sender's phone number in E.164 format (e.g., +15554441234).</param>
/// <param name="Recipient">The recipient's phone number in E.164 format (e.g., +15554441234).</param>
/// <param name="Message">The message content to send.</param>
public readonly record struct SmsMessage(
    string Sender,
    string Recipient,
    string Message
);
