namespace Arbiter.Communication.Email;

/// <summary>
/// Represents an email address with an optional display name.
/// </summary>
/// <param name="Address">The email address (e.g., user@example.com).</param>
/// <param name="DisplayName">The optional display name for the address.</param>
public readonly record struct EmailAddress(
    string Address,
    string? DisplayName = null
);
