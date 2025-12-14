using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Handles question mark (?) wildcard pattern matching.
/// </summary>
internal sealed class QuestionMarkHandler : IPatternHandler
{
    public bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx)
    {
        return patternChar == '?';
    }

    public MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy)
    {
        // Question mark matches exactly one character
        patternIdx++;
        pathIdx++;
        return MatchResult.Matched();
    }
}
