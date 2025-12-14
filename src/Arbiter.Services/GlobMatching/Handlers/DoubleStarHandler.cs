using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Handles double-star (**) pattern matching for directory recursion.
/// </summary>
internal sealed class DoubleStarHandler : IPatternHandler
{
    public bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx)
    {
        return patternChar == '*' && 
               patternIdx + 1 < pattern.Length && 
               pattern[patternIdx + 1] == '*';
    }

    public MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy)
    {
        // Skip ** and any following slashes
        patternIdx += 2;
        while (patternIdx < pattern.Length && (pattern[patternIdx] == '/' || pattern[patternIdx] == '\\'))
            patternIdx++;

        if (patternIdx >= pattern.Length)
            return MatchResult.Complete(true); // ** at end matches everything

        bool result = strategy.DoubleStarMatcher(pattern[patternIdx..], path[pathIdx..]);
        return MatchResult.Complete(result);
    }
}
