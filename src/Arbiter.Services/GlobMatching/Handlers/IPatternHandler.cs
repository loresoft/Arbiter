using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Interface for pattern-specific matching handlers.
/// </summary>
internal interface IPatternHandler
{
    /// <summary>
    /// Determines if this handler can process the given pattern character.
    /// </summary>
    bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx);

    /// <summary>
    /// Attempts to match the pattern at the current position.
    /// Returns a result indicating success and how to update indices.
    /// </summary>
    MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy);
}

/// <summary>
/// Result of a pattern matching attempt.
/// </summary>
internal readonly struct MatchResult
{
    public MatchResultType Type { get; init; }
    public bool Success { get; init; }

    public static MatchResult Matched() => new() { Type = MatchResultType.Matched, Success = true };
    public static MatchResult Failed() => new() { Type = MatchResultType.Failed, Success = false };
    public static MatchResult Complete(bool success) => new() { Type = MatchResultType.Complete, Success = success };
}

internal enum MatchResultType
{
    Matched,    // Pattern matched, continue
    Failed,     // Pattern failed, try backtracking
    Complete    // Matching complete, return result
}
