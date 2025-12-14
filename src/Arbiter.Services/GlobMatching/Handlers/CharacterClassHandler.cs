using System;

namespace Arbiter.Services.GlobMatching.Handlers;

/// <summary>
/// Handles character class [abc] or range [a-z] pattern matching.
/// </summary>
internal sealed class CharacterClassHandler : IPatternHandler
{
    public bool CanHandle(char patternChar, ReadOnlySpan<char> pattern, int patternIdx)
    {
        return patternChar == '[';
    }

    public MatchResult TryMatch(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIdx,
        ref int pathIdx,
        MatchingStrategy strategy)
    {
        int closeIdx = pattern[(patternIdx + 1)..].IndexOf(']');
        
        if (closeIdx == -1)
        {
            // Invalid pattern, treat [ as literal
            if (strategy.CharEquals(pattern[patternIdx], path[pathIdx]))
            {
                patternIdx++;
                pathIdx++;
                return MatchResult.Matched();
            }
            return MatchResult.Failed();
        }

        closeIdx += patternIdx + 1;
        var charClass = pattern[(patternIdx + 1)..closeIdx];
        bool negate = charClass.Length > 0 && charClass[0] == '!';
        
        if (negate)
            charClass = charClass[1..];

        bool match = strategy.CharClassMatcher(charClass, path[pathIdx]);
        
        if (negate)
            match = !match;

        if (match)
        {
            patternIdx = closeIdx + 1;
            pathIdx++;
            return MatchResult.Matched();
        }

        return MatchResult.Failed();
    }
}
