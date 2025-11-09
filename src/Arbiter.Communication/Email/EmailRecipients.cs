namespace Arbiter.Communication.Email;

/// <summary>
/// Represents the recipients of an email, including To, Cc, and Bcc addresses.
/// </summary>
/// <param name="To">The primary recipients of the email.</param>
/// <param name="Cc">The optional carbon copy recipients.</param>
/// <param name="Bcc">The optional blind carbon copy recipients.</param>
public readonly record struct EmailRecipients(
    IReadOnlyList<EmailAddress> To,
    IReadOnlyList<EmailAddress>? Cc = null,
    IReadOnlyList<EmailAddress>? Bcc = null
)
{
    /// <summary>
    /// Returns a string representation of the email recipients, including "To", "Cc", and "Bcc" addresses.
    /// </summary>
    /// <remarks>
    /// The returned string contains a comma-separated list of unique email addresses from the "To",
    /// "Cc", and "Bcc" fields. If no recipients are specified, the method returns "No recipients".
    /// </remarks>
    /// <returns>A semicolon-separated string of unique email addresses, or "No recipients" if no addresses are present.</returns>
    public override string ToString()
    {
        var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var address in To)
            addresses.Add(address.Address);

        if (Cc != null)
        {
            foreach (var address in Cc)
                addresses.Add(address.Address);
        }
        if (Bcc != null)
        {
            foreach (var address in Bcc)
                addresses.Add(address.Address);
        }

        if (addresses.Count == 0)
            return "No recipients";

        return string.Join(';', addresses);
    }
};
