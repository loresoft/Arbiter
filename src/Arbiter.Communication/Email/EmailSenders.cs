namespace Arbiter.Communication.Email;

/// <summary>
/// Represents the sender and optional reply-to addresses for an email.
/// </summary>
/// <param name="From">The sender's email address.</param>
/// <param name="ReplyTo">The optional collection of reply-to addresses.</param>
public readonly record struct EmailSenders(
    EmailAddress From,
    IReadOnlyList<EmailAddress>? ReplyTo = null
);
