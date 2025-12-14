using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Handles brace expansion {jpg,png,gif} pattern matching.
/// </summary>
internal sealed class BraceExpansionHandler : IPatternHandler
{
    public bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx)
    {
        return patternChar == '{';
    }

    public MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy)
    {
        int closeIdx = FindMatchingBrace(pattern[patternIdx..]);
        
        if (closeIdx == -1)
        {
            // Invalid pattern, treat { as literal
            if (strategy.CharEquals(pattern[patternIdx], path[pathIdx]))
            {
                patternIdx++;
                pathIdx++;
                return MatchResult.Matched();
            }
            return MatchResult.Failed();
        }

        closeIdx += patternIdx;
        var options = pattern[(patternIdx + 1)..closeIdx];
        var remaining = pattern[(closeIdx + 1)..];

        bool result = strategy.BraceExpansionMatcher(options, remaining, path[pathIdx..]);
        return MatchResult.Complete(result);
    }

    private static int FindMatchingBrace(ReadOnlySpan<char> span)
    {
        int depth = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == '{')
            {
                depth++;
            }
            else if (span[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }
        return -1;
    }
}
