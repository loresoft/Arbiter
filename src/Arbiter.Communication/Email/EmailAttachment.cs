namespace Arbiter.Communication.Email;

/// <summary>
/// Represents an email attachment, including its name, content type, content, and optional content ID.
/// </summary>
/// <param name="Name">The name of the attachment file.</param>
/// <param name="ContentType">The MIME type of the attachment.</param>
/// <param name="Content">The binary content of the attachment.</param>
/// <param name="ContentId">The optional content ID for inline attachments.</param>
public readonly record struct EmailAttachment(
    string Name,
    string ContentType,
    byte[] Content,
    string? ContentId = null
);
