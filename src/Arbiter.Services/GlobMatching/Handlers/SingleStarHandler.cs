using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Handles single-star (*) wildcard pattern matching.
/// </summary>
internal sealed class SingleStarHandler : IPatternHandler
{
    public bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx)
    {
        return patternChar == '*';
    }

    public MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy)
    {
        // Single star handled by backtracking mechanism in main loop
        // Just advance pattern index
        patternIdx++;
        return MatchResult.Matched();
    }
}
