using System.Collections;
using System.Diagnostics;
using System.Security.Claims;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Provides logging scope creation enriched with user identity claims from <see cref="ClaimsPrincipal"/> for structured
/// logging and distributed tracing correlation.
/// </summary>
/// <remarks>
/// Extracts configured claims from authenticated users and adds them as structured scope properties to
/// the logger and as tags to the current <see cref="Activity"/>. For unauthenticated requests, uses deterministic
/// fallback values to ensure stable logging keys.
/// </remarks>
internal static class ClaimLoggingContext
{
    // Fallback value used when no authenticated identity is available.
    private const string Anonymous = "anonymous";

    /// <summary>
    /// Creates a logging scope enriched with user identity claims and distributed tracing tags.
    /// </summary>
    /// <remarks>
    /// For unauthenticated users, scope values are set to "Anonymous" to ensure stable structured logging keys.
    /// Extracted claim values are also set as activity tags on the current distributed trace span.
    /// </remarks>
    /// <param name="logger">The logger instance on which to create the scope.</param>
    /// <param name="user">The claims principal representing the current user, or <see langword="null"/> if no user context is available.</param>
    /// <param name="options">Configuration specifying which claims to extract and how to resolve them.</param>
    /// <returns>An <see cref="IDisposable"/> that removes the scope when disposed, or <see langword="null"/> if the logger does
    /// not support scopes.</returns>
    public static IDisposable? BeginScope(
        ILogger logger,
        ClaimsPrincipal? user,
        RequestLoggingOptions options)
    {
        var userNameClaim = options.UserNameClaim;
        var userIdentifierClaim = options.UserIdentifierClaim;

        // If the request is not authenticated, still create deterministic logging values
        // so downstream logs/telemetry have stable keys and do not need null handling.
        if (user?.Identity?.IsAuthenticated != true)
        {
            // Add user context to the current distributed trace span/activity.
            SetActivityTag(userNameClaim.ActivityTagName, Anonymous);
            SetActivityTag(userIdentifierClaim.ActivityTagName, Anonymous);

            // Create a scope payload that logging providers can enumerate.
            // This gives us structured values like {UserName} and {UserId} on all logs in scope.
            LoggingScopeState state = new(
                userNameKey: userNameClaim.ScopeKey,
                userName: Anonymous,
                userIdKey: userIdentifierClaim.ScopeKey,
                userId: Anonymous,
                additionalClaims: null);

            return logger.BeginScope(state);
        }

        // Resolve username and identifier using configured claim priority/fallback rules.
        var userName = ResolveUserName(user, userNameClaim);
        var userId = ResolveClaimValue(user, userIdentifierClaim) ?? userName;

        // Mirror the same identity values into activity tags for tracing systems.
        SetActivityTag(userNameClaim.ActivityTagName, userName);
        SetActivityTag(userIdentifierClaim.ActivityTagName, userId);

        List<ClaimValue>? additionalClaims = null;

        // Resolve any optional configured claims and include only populated values.
        foreach (var claim in options.AdditionalClaims)
        {
            var value = ResolveClaimValue(user, claim);
            if (value is null)
                continue;

            // Keep tracing metadata in sync with logging scope metadata.
            SetActivityTag(claim.ActivityTagName, value);

            // Lazy allocation: avoid list allocation when there are no additional claim values.
            additionalClaims ??= new List<ClaimValue>(options.AdditionalClaims.Count);
            additionalClaims.Add(new ClaimValue(claim.ScopeKey, value));
        }

        // Package all resolved values into one compact scope state object.
        LoggingScopeState scopeState = new(
            userNameKey: userNameClaim.ScopeKey,
            userName: userName,
            userIdKey: userIdentifierClaim.ScopeKey,
            userId: userId,
            additionalClaims: additionalClaims);

        return logger.BeginScope(scopeState);
    }

    private static string? ResolveClaimValue(ClaimsPrincipal user, ClaimLoggingType claim)
    {
        // Support multiple claim types (ordered by preference) for interoperability
        // across identity providers that emit different claim names.
        foreach (var claimType in claim.ClaimTypes)
        {
            var value = user.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string ResolveUserName(ClaimsPrincipal user, ClaimLoggingType claim)
    {
        // Identity.Name is the canonical .NET user display/identity name when available.
        var userName = user.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(userName))
            return userName;

        // Fallback to configured claim types; if none exist, use anonymous sentinel.
        return ResolveClaimValue(user, claim) ?? Anonymous;
    }

    private static void SetActivityTag(string? tagName, string value)
    {
        // Allow callers to disable specific tags by leaving the configured tag name empty.
        if (string.IsNullOrWhiteSpace(tagName))
            return;

        // Activity.Current is null when no active trace span exists for this execution path.
        Activity.Current?.SetTag(tagName, value);
    }


    private readonly record struct ClaimValue(string ScopeKey, string Value);

    // Represents the structured state passed to ILogger.BeginScope(...).
    //
    // Why IReadOnlyList<KeyValuePair<string, object?>>?
    // - Many logging providers (including Microsoft.Extensions.Logging implementations)
    //   treat scope state as enumerable key/value pairs.
    // - This allows providers/sinks to capture each claim as structured scope properties
    //   instead of a single formatted string.
    // - The custom list + struct enumerator minimizes allocations compared to using a
    //   dictionary per request.
    private sealed class LoggingScopeState(
        string userNameKey,
        string userName,
        string userIdKey,
        string userId,
        IReadOnlyList<ClaimValue>? additionalClaims)
        : IReadOnlyList<KeyValuePair<string, object?>>
    {
        // Two required values (username + userId), plus optional additional claims.
        public int Count => 2 + (additionalClaims?.Count ?? 0);

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                // Position 0 is always the configured username key/value.
                if (index == 0)
                    return new KeyValuePair<string, object?>(userNameKey, userName);

                // Position 1 is always the configured user identifier key/value.
                if (index == 1)
                    return new KeyValuePair<string, object?>(userIdKey, userId);

                // Remaining positions map to optional claim values.
                // Unsigned bounds check pattern avoids extra branches for negative indices.
                if (additionalClaims is not null && (uint)(index - 2) < (uint)additionalClaims.Count)
                {
                    var claim = additionalClaims[index - 2];
                    return new KeyValuePair<string, object?>(claim.ScopeKey, claim.Value);
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Value-type enumerator to iterate scope key/value pairs with low allocation overhead.
        public struct Enumerator(LoggingScopeState state) : IEnumerator<KeyValuePair<string, object?>>
        {
            private int _index = -1;

            public readonly KeyValuePair<string, object?> Current => state[_index];

            readonly object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_index >= state.Count - 1)
                    return false;

                _index++;
                return true;
            }

            public void Reset() => _index = -1;

            public readonly void Dispose() { }
        }
    }
}
