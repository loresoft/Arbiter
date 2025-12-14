using System;

namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Encapsulates all matching strategies for case-sensitive and case-insensitive modes.
/// </summary>
internal readonly struct MatchingStrategy
{
    public required CharEqualityComparer CharEquals { get; init; }
    public required CharClassMatcher CharClassMatcher { get; init; }
    public required BraceExpansionMatcher BraceExpansionMatcher { get; init; }
    public required DoubleStarMatcher DoubleStarMatcher { get; init; }

    public static MatchingStrategy CreateCaseSensitive(ComplexPatternMatcher matcher)
    {
        return new MatchingStrategy
        {
            CharEquals = static (a, b) => a == b,
            CharClassMatcher = ComplexPatternMatcher.MatchCharClassCaseSensitive,
            BraceExpansionMatcher = matcher.TryMatchBraceExpansion,
            DoubleStarMatcher = matcher.MatchDoubleStarPattern
        };
    }

    public static MatchingStrategy CreateCaseInsensitive(ComplexPatternMatcher matcher)
    {
        return new MatchingStrategy
        {
            CharEquals = ComplexPatternMatcher.CharEqualsCaseInsensitive,
            CharClassMatcher = ComplexPatternMatcher.MatchCharClassCaseInsensitive,
            BraceExpansionMatcher = matcher.TryMatchBraceExpansionCaseInsensitive,
            DoubleStarMatcher = matcher.MatchDoubleStarPatternCaseInsensitive
        };
    }
}

// Delegate types for strategy pattern
internal delegate bool CharEqualityComparer(char a, char b);
internal delegate bool CharClassMatcher(ReadOnlySpan<char> charClass, char c);
internal delegate bool BraceExpansionMatcher(ReadOnlySpan<char> options, ReadOnlySpan<char> remaining, ReadOnlySpan<char> path);
internal delegate bool DoubleStarMatcher(ReadOnlySpan<char> remainingPattern, ReadOnlySpan<char> remainingPath);
internal delegate bool PatternMatcher(ReadOnlySpan<char> pattern, ReadOnlySpan<char> path);
