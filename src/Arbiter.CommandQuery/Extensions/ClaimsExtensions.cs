using System.Globalization;
using System.Security.Claims;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for reading and writing claims on <see cref="ClaimsPrincipal"/>
/// and <see cref="ClaimsIdentity"/> instances.
/// </summary>
/// <remarks>
/// Claim values are always stored as <see cref="string"/>. Typed values are written and read using
/// <see cref="CultureInfo.InvariantCulture"/> so they round trip consistently regardless of the current culture.
/// </remarks>
public static class ClaimsExtensions
{
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimTypes.Email"/>, <see cref="ClaimNames.EmailClaim"/>
    /// and <see cref="ClaimNames.EmailsClaim"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetEmail(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimTypes.Email)
            ?? claimsPrincipal.GetValue(ClaimNames.EmailClaim)
            ?? claimsPrincipal.GetValue(ClaimNames.EmailsClaim);
    }

    /// <summary>
    /// Gets the name identifier of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the <see cref="ClaimTypes.NameIdentifier"/> claim if found; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetIdentifier(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Gets the object identifier of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The first claim value found in order of <see cref="ClaimNames.IdentifierClaim"/>, <see cref="ClaimNames.ObjectIdentifier"/>
    /// and <see cref="ClaimTypes.NameIdentifier"/> parsed as a <see cref="Guid"/>; otherwise, <see langword="null"/>
    /// if no claim is found or the value is not a valid <see cref="Guid"/>.
    /// </returns>
    public static Guid? GetObjectId(this ClaimsPrincipal? claimsPrincipal)
    {
        var value = claimsPrincipal.GetValue(ClaimNames.IdentifierClaim)
            ?? claimsPrincipal.GetValue(ClaimNames.ObjectIdentifier)
            ?? claimsPrincipal.GetValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var oid) ? oid : null;
    }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.NameClaim"/>, <see cref="ClaimTypes.Name"/>
    /// and <see cref="ClaimNames.Subject"/>; otherwise, the identity name, or <see langword="null"/> if not available.
    /// </returns>
    public static string? GetName(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.NameClaim)
            ?? claimsPrincipal.GetValue(ClaimTypes.Name)
            ?? claimsPrincipal.GetValue(ClaimNames.Subject)
            ?? claimsPrincipal?.Identity?.Name;
    }

    /// <summary>
    /// Gets the identity provider of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.ProviderClaim"/> and
    /// <see cref="ClaimNames.IdentityClaim"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetProvider(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.ProviderClaim)
            ?? claimsPrincipal.GetValue(ClaimNames.IdentityClaim);
    }

    /// <summary>
    /// Gets the user name of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.PreferredUserName"/>, <see cref="ClaimTypes.Name"/>
    /// and <see cref="ClaimNames.Subject"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetUserName(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.PreferredUserName)
            ?? claimsPrincipal.GetValue(ClaimTypes.Name)
            ?? claimsPrincipal.GetValue(ClaimNames.Subject);
    }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.DisplayName"/>, <see cref="ClaimNames.NameClaim"/>,
    /// <see cref="ClaimTypes.Name"/> and <see cref="ClaimNames.Subject"/>; otherwise, the identity name, or
    /// <see langword="null"/> if not available.
    /// </returns>
    public static string? GetDisplayName(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.DisplayName)
            ?? claimsPrincipal.GetValue(ClaimNames.NameClaim)
            ?? claimsPrincipal.GetValue(ClaimTypes.Name)
            ?? claimsPrincipal.GetValue(ClaimNames.Subject)
            ?? claimsPrincipal?.Identity?.Name;
    }

    /// <summary>
    /// Gets the given name (first name) of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.GivenName"/> and
    /// <see cref="ClaimTypes.GivenName"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetGivenName(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.GivenName)
            ?? claimsPrincipal.GetValue(ClaimTypes.GivenName);
    }

    /// <summary>
    /// Gets the family name (last name) of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.FamilyName"/> and
    /// <see cref="ClaimTypes.Surname"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetFamilyName(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.FamilyName)
            ?? claimsPrincipal.GetValue(ClaimTypes.Surname);
    }

    /// <summary>
    /// Gets the phone number of the user.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <returns>
    /// The value of the first claim found in order of <see cref="ClaimNames.PhoneNumber"/>,
    /// <see cref="ClaimTypes.MobilePhone"/> and <see cref="ClaimTypes.HomePhone"/>; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? GetPhoneNumber(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetValue(ClaimNames.PhoneNumber)
            ?? claimsPrincipal.GetValue(ClaimTypes.MobilePhone)
            ?? claimsPrincipal.GetValue(ClaimTypes.HomePhone);
    }

    /// <summary>
    /// Gets the user identifier from the <see cref="ClaimNames.UserId"/> claim parsed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the claim value as.</typeparam>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The parsed claim value if found and parsable; otherwise, the default value of <typeparamref name="T"/>.
    /// </returns>
    public static T? GetUserId<T>(this ClaimsPrincipal? principal)
        where T : IParsable<T>
    {
        return principal.GetValue<T>(ClaimNames.UserId);
    }

    /// <summary>
    /// Gets the tenant identifier from the <see cref="ClaimNames.TenantId"/> claim parsed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the claim value as.</typeparam>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The parsed claim value if found and parsable; otherwise, the default value of <typeparamref name="T"/>.
    /// </returns>
    public static T? GetTenantId<T>(this ClaimsPrincipal? principal)
        where T : IParsable<T>
    {
        return principal.GetValue<T>(ClaimNames.TenantId);
    }

    /// <summary>
    /// Gets the tenant identifiers from all <see cref="ClaimNames.TenantId"/> claims parsed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the claim values as.</typeparam>
    /// <param name="principal">The principal to read the claims from.</param>
    /// <returns>
    /// A read-only list of parsed claim values. If no claims are found or none are parsable, an empty list is returned.
    /// </returns>
    public static IReadOnlyList<T> GetTenantIds<T>(this ClaimsPrincipal? principal)
        where T : IParsable<T>
    {
        return principal.GetValues<T>(ClaimNames.TenantId);
    }


    /// <summary>
    /// Gets the value of the first claim with the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <param name="type">The claim type to find.</param>
    /// <returns>
    /// The claim value if found; otherwise, <see langword="null"/>. Also returns <see langword="null"/>
    /// when <paramref name="claimsPrincipal"/> is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string? GetValue(this ClaimsPrincipal? claimsPrincipal, string type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var claim = claimsPrincipal?.FindFirst(type);
        return claim?.Value;
    }

    /// <summary>
    /// Gets the value of the first claim with the specified <paramref name="type"/> parsed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the claim value as, such as <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/> or <see cref="string"/>.</typeparam>
    /// <param name="claimsPrincipal">The principal to read the claim from.</param>
    /// <param name="type">The claim type to find.</param>
    /// <returns>
    /// The parsed claim value if found and parsable; otherwise, the default value of <typeparamref name="T"/>.
    /// A missing claim and an unparsable value are not distinguished.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public static T? GetValue<T>(this ClaimsPrincipal? claimsPrincipal, string type)
        where T : IParsable<T>
    {
        ArgumentNullException.ThrowIfNull(type);

        if (claimsPrincipal == null)
            return default;

        var claim = claimsPrincipal.FindFirst(type);
        if (claim == null)
            return default;

        return T.TryParse(claim.Value, CultureInfo.InvariantCulture, out var result) ? result : default;
    }

    /// <summary>
    /// Gets the values of all claims with the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="claimsPrincipal">The principal to read the claims from.</param>
    /// <param name="type">The claim type to find.</param>
    /// <returns>
    /// The matching claim values, or an empty list when <paramref name="claimsPrincipal"/> is
    /// <see langword="null"/> or no claims match.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetValues(this ClaimsPrincipal? claimsPrincipal, string type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (claimsPrincipal is null)
            return [];

        List<string> values = [];

        foreach (var claim in claimsPrincipal.FindAll(type))
            values.Add(claim.Value);

        return values;
    }

    /// <summary>
    /// Gets the values of all claims with the specified <paramref name="type"/> parsed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the claim values as.</typeparam>
    /// <param name="claimsPrincipal">The principal to read the claims from.</param>
    /// <param name="type">The claim type to find.</param>
    /// <returns>
    /// The parsed claim values, or an empty list when <paramref name="claimsPrincipal"/> is
    /// <see langword="null"/> or no claims match. Values that cannot be parsed are skipped.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<T> GetValues<T>(this ClaimsPrincipal? claimsPrincipal, string type)
        where T : IParsable<T>
    {
        ArgumentNullException.ThrowIfNull(type);

        if (claimsPrincipal is null)
            return [];

        List<T> values = [];

        foreach (var claim in claimsPrincipal.FindAll(type))
        {
            if (T.TryParse(claim.Value, CultureInfo.InvariantCulture, out var result))
                values.Add(result);
        }

        return values;
    }


    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> and <paramref name="value"/> to the
    /// primary <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to add the claim to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="value">The claim value to add. The claim is not added when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal AddClaim(this ClaimsPrincipal principal, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.AddClaim(type, value);

        return principal;

    }

    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> and <paramref name="value"/> to the
    /// primary <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>.
    /// </summary>
    /// <typeparam name="T">The type of the claim value.</typeparam>
    /// <param name="principal">The principal to add the claim to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="value">The claim value to add. The claim is not added when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal AddClaim<T>(this ClaimsPrincipal principal, string type, T? value)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.AddClaim(type, value);

        return principal;
    }

    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> for each of the <paramref name="values"/> to the
    /// primary <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>.
    /// </summary>
    /// <typeparam name="T">The type of the claim values.</typeparam>
    /// <param name="principal">The principal to add the claims to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="values">The claim values to add. Values that are <see langword="null"/> or empty are skipped.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal AddClaims<T>(this ClaimsPrincipal principal, string type, IEnumerable<T>? values)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.AddClaims(type, values);

        return principal;
    }

    /// <summary>
    /// Replaces the first claim with the specified <paramref name="type"/> on the primary
    /// <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to replace the claim on.</param>
    /// <param name="type">The claim type to replace.</param>
    /// <param name="value">The new claim value. No change is made when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal ReplaceClaim(this ClaimsPrincipal principal, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.ReplaceClaim(type, value);

        return principal;
    }

    /// <summary>
    /// Adds a role claim to the primary <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>
    /// when the specified <paramref name="condition"/> evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="principal">The principal to add the role to.</param>
    /// <param name="role">The role to add, using the identity role claim type.</param>
    /// <param name="condition">The condition evaluated to determine whether the role is added.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/>, <paramref name="role"/> or <paramref name="condition"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal AddRole(this ClaimsPrincipal principal, string role, Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(condition);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.AddRole(role, condition);

        return principal;
    }

    /// <summary>
    /// Adds a role claim to the primary <see cref="ClaimsIdentity"/> of the <paramref name="principal"/>
    /// when the specified <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="principal">The principal to add the role to.</param>
    /// <param name="role">The role to add, using the identity role claim type.</param>
    /// <param name="condition">When <see langword="true"/>, the role is added; otherwise, no change is made.</param>
    /// <returns>The same <paramref name="principal"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> or <paramref name="role"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="principal"/> does not have a <see cref="ClaimsIdentity"/>.</exception>
    public static ClaimsPrincipal AddRole(this ClaimsPrincipal principal, string role, bool condition = true)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(role);

        var identity = principal.Identity as ClaimsIdentity
            ?? throw new ArgumentException("The ClaimsPrincipal does not have a ClaimsIdentity.", nameof(principal));

        identity.AddRole(role, condition);

        return principal;
    }



    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> and <paramref name="value"/> to the <paramref name="identity"/>.
    /// </summary>
    /// <param name="identity">The identity to add the claim to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="value">The claim value to add. The claim is not added when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity AddClaim(this ClaimsIdentity identity, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(type);

        if (value.HasValue())
            identity.AddClaim(new Claim(type, value));

        return identity;
    }

    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> and <paramref name="value"/> to the <paramref name="identity"/>.
    /// </summary>
    /// <typeparam name="T">The type of the claim value.</typeparam>
    /// <param name="identity">The identity to add the claim to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="value">The claim value to add. The claim is not added when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <remarks>
    /// The value is formatted using <see cref="CultureInfo.InvariantCulture"/>, with date and time types using the
    /// round trip (<c>o</c>) format, so it can be read back with <see cref="GetValue{T}(ClaimsPrincipal, string)"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity AddClaim<T>(this ClaimsIdentity identity, string type, T? value)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(type);

        AddClaim(identity, type, FormatValue(value));
        return identity;
    }

    /// <summary>
    /// Adds a claim with the specified <paramref name="type"/> for each of the <paramref name="values"/> to the <paramref name="identity"/>.
    /// </summary>
    /// <typeparam name="T">The type of the claim values.</typeparam>
    /// <param name="identity">The identity to add the claims to.</param>
    /// <param name="type">The claim type to add.</param>
    /// <param name="values">The claim values to add. Values that are <see langword="null"/> or empty are skipped.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity AddClaims<T>(this ClaimsIdentity identity, string type, IEnumerable<T>? values)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(type);

        if (values is null)
            return identity;

        foreach (var value in values)
            AddClaim(identity, type, value);

        return identity;
    }

    /// <summary>
    /// Replaces the first claim with the specified <paramref name="type"/> on the <paramref name="identity"/>.
    /// </summary>
    /// <param name="identity">The identity to replace the claim on.</param>
    /// <param name="type">The claim type to replace.</param>
    /// <param name="value">The new claim value. No change is made when <see langword="null"/> or empty.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <remarks>
    /// The claim is added when it does not already exist, and left unchanged when the existing value matches.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity ReplaceClaim(this ClaimsIdentity identity, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(type);

        if (value.IsNullOrEmpty())
            return identity;

        var claim = identity.FindFirst(type);
        if (claim != null)
        {
            if (string.Equals(claim.Value, value, StringComparison.Ordinal))
                return identity;

            identity.RemoveClaim(claim);
        }

        var newClaim = new Claim(type, value);
        identity.AddClaim(newClaim);

        return identity;
    }

    /// <summary>
    /// Adds a role claim to the <paramref name="identity"/> when the specified <paramref name="condition"/>
    /// evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="identity">The identity to add the role to.</param>
    /// <param name="role">The role to add, using <see cref="ClaimsIdentity.RoleClaimType"/>.</param>
    /// <param name="condition">The condition evaluated to determine whether the role is added.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/>, <paramref name="role"/> or <paramref name="condition"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity AddRole(this ClaimsIdentity identity, string role, Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(condition);

        if (condition())
            identity.AddClaim(identity.RoleClaimType, role);

        return identity;
    }

    /// <summary>
    /// Adds a role claim to the <paramref name="identity"/> when the specified <paramref name="condition"/>
    /// is <see langword="true"/>.
    /// </summary>
    /// <param name="identity">The identity to add the role to.</param>
    /// <param name="role">The role to add, using <see cref="ClaimsIdentity.RoleClaimType"/>.</param>
    /// <param name="condition">When <see langword="true"/>, the role is added; otherwise, no change is made.</param>
    /// <returns>The same <paramref name="identity"/> so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> or <paramref name="role"/> is <see langword="null"/>.</exception>
    public static ClaimsIdentity AddRole(this ClaimsIdentity identity, string role, bool condition = true)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(role);

        if (condition)
            identity.AddClaim(identity.RoleClaimType, role);

        return identity;
    }


    // claim values must round trip with GetValue<T>, so format culture invariant
    private static string? FormatValue<T>(T? value) => value switch
    {
        null => null,
        string text => text,
        DateTime dateTime => dateTime.ToString("o", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("o", CultureInfo.InvariantCulture),
        DateOnly dateOnly => dateOnly.ToString("o", CultureInfo.InvariantCulture),
        TimeOnly timeOnly => timeOnly.ToString("o", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(format: null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

}
