namespace Arbiter.CommandQuery.Services;

/// <summary>
/// A static class that contains claim names used in the application.
/// </summary>
public static class ClaimNames
{
    /// <summary>
    /// The claim name for the object identifier.
    /// </summary>
    public const string ObjectIdentifier = "oid";

    /// <summary>
    /// The claim name for the subject identifier.
    /// </summary>
    public const string Subject = "sub";

    /// <summary>
    /// The claim name for the name of the user.
    /// </summary>
    public const string NameClaim = "name";

    /// <summary>
    /// The claim name for the email address of the user.
    /// </summary>
    public const string EmailClaim = "email";

    /// <summary>
    /// The claim name for the email addresses of the user.
    /// </summary>
    public const string EmailsClaim = "emails";

    /// <summary>
    /// The claim name for the security provider.
    /// </summary>
    public const string ProviderClaim = "idp";

    /// <summary>
    /// The claim name for the preferred username of the user.
    /// </summary>
    public const string PreferredUserName = "preferred_username";

    /// <summary>
    /// The claim name for the identity provider.
    /// </summary>
    public const string IdentityClaim = "http://schemas.microsoft.com/identity/claims/identityprovider";

    /// <summary>
    /// The claim name for the object identifier of the user.
    /// </summary>
    public const string IdentifierClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    /// <summary>
    /// The claim name for the user identifier.
    /// </summary>
    public const string UserId = "uid";

    /// <summary>
    /// The claim name for the user employee number.
    /// </summary>
    public const string EmployeeNumber = "emp";

    /// <summary>
    /// The claim name for the user rules.
    /// </summary>
    public const string RuleClaim = "rules";
}
