using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arbiter.Services.GlobMatching.Handlers;

namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Handles complex glob pattern matching with wildcards, character classes, and brace expansion.
/// </summary>
internal sealed class ComplexPatternMatcher
{
    private readonly string _pattern;
    private readonly string? _patternUpper;
    private readonly bool _caseSensitive;

    // Pattern handlers for different wildcard types
    private static readonly IPatternHandler[] _handlers =
    [
        new DoubleStarHandler(),
        new SingleStarHandler(),
        new QuestionMarkHandler(),
        new CharacterClassHandler(),
        new BraceExpansionHandler()
    ];

    public ComplexPatternMatcher(string pattern, string? patternUpper, bool caseSensitive)
    {
        _pattern = pattern;
        _patternUpper = patternUpper;
        _caseSensitive = caseSensitive;
    }

    public bool Match(ReadOnlySpan<char> path)
    {
        // For case-insensitive matching, use pre-computed uppercase pattern
        if (!_caseSensitive && _patternUpper != null)
        {
            var strategy = MatchingStrategy.CreateCaseInsensitive(this);
            return MatchPatternImplementation(_patternUpper.AsSpan(), path, strategy);
        }

        var caseSensitiveStrategy = MatchingStrategy.CreateCaseSensitive(this);
        return MatchPatternImplementation(_pattern.AsSpan(), path, caseSensitiveStrategy);
    }

    private bool MatchPatternImplementation(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        MatchingStrategy strategy)
    {
        int patternIdx = 0;
        int pathIdx = 0;
        int starIdx = -1;
        int matchIdx = 0;

        while (pathIdx < path.Length)
        {
            if (patternIdx < pattern.Length)
            {
                char p = pattern[patternIdx];
                bool handlerMatched = false;

                // Try pattern handlers
                foreach (var handler in _handlers)
                {
                    if (handler.CanHandle(p, pattern, patternIdx))
                    {
                        handlerMatched = true;
                        var result = handler.TryMatch(pattern, path, ref patternIdx, ref pathIdx, strategy);

                        if (result.Type == MatchResultType.Complete)
                            return result.Success;

                        if (result.Type == MatchResultType.Matched)
                        {
                            // Special handling for single star - update backtracking points
                            if (p == '*')
                            {
                                starIdx = patternIdx - 1;  // Star was consumed, so -1
                                matchIdx = pathIdx;
                            }
                            goto continueMatching;  // Continue the while loop
                        }

                        // Failed - will try backtracking below
                        goto tryBacktrack;
                    }
                }

                // If no handler matched, try literal character match
                if (!handlerMatched)
                {
                    if (strategy.CharEquals(p, path[pathIdx]))
                    {
                        patternIdx++;
                        pathIdx++;
                        continue;
                    }
                }

            tryBacktrack:
                // No match, backtrack to star if available
                if (starIdx != -1)
                {
                    patternIdx = starIdx + 1;
                    matchIdx++;
                    pathIdx = matchIdx;
                    continue;
                }

                return false;

            continueMatching:
                continue;
            }
            else
            {
                // Pattern exhausted but path remains
                if (starIdx != -1)
                {
                    patternIdx = starIdx + 1;
                    matchIdx++;
                    pathIdx = matchIdx;
                    continue;
                }

                return false;
            }
        }

        // Consume remaining stars in pattern
        while (patternIdx < pattern.Length && pattern[patternIdx] == '*')
            patternIdx++;

        return patternIdx >= pattern.Length;
    }

    // Double-star matching methods
    public bool MatchDoubleStarPattern(ReadOnlySpan<char> remainingPattern, ReadOnlySpan<char> remainingPath)
    {
        return MatchDoubleStarPatternImplementation(
            remainingPattern,
            remainingPath,
            static (a, b) => a == b,
            (p, path) => MatchPatternImplementation(p, path, MatchingStrategy.CreateCaseSensitive(this)));
    }

    public bool MatchDoubleStarPatternCaseInsensitive(ReadOnlySpan<char> remainingPattern, ReadOnlySpan<char> remainingPath)
    {
        return MatchDoubleStarPatternImplementation(
            remainingPattern,
            remainingPath,
            CharEqualsCaseInsensitive,
            (p, path) => MatchPatternImplementation(p, path, MatchingStrategy.CreateCaseInsensitive(this)));
    }

    private bool MatchDoubleStarPatternImplementation(
        ReadOnlySpan<char> remainingPattern,
        ReadOnlySpan<char> remainingPath,
        CharEqualityComparer charEquals,
        PatternMatcher patternMatcher)
    {
        if (remainingPattern.Length == 0)
            return true;

        char firstPatternChar = remainingPattern[0];

        // For literal characters, optimize by searching
        if (firstPatternChar != '*' && firstPatternChar != '?' &&
            firstPatternChar != '[' && firstPatternChar != '{')
        {
            // Try matching at each position in the path
            for (int i = 0; i <= remainingPath.Length; i++)
            {
                if (patternMatcher(remainingPattern, remainingPath[i..]))
                    return true;

                // Skip ahead to next occurrence of first character for optimization
                if (i < remainingPath.Length)
                {
                    int nextPos = i + 1;
                    bool found = false;

                    while (nextPos < remainingPath.Length)
                    {
                        if (charEquals(firstPatternChar, remainingPath[nextPos]))
                        {
                            found = true;
                            break;
                        }
                        nextPos++;
                    }

                    // If we can't find the character ahead, no point continuing
                    if (!found && nextPos >= remainingPath.Length)
                    {
                        return false;
                    }

                    // Jump to that position (subtract 1 because loop will increment)
                    if (found)
                    {
                        i = nextPos - 1;
                    }
                }
            }
        }
        else
        {
            // Pattern starts with wildcard, try matching at every position
            for (int i = 0; i <= remainingPath.Length; i++)
            {
                if (patternMatcher(remainingPattern, remainingPath[i..]))
                    return true;
            }
        }

        return false;
    }

    // Brace expansion methods
    public bool TryMatchBraceExpansion(ReadOnlySpan<char> options, ReadOnlySpan<char> remaining, ReadOnlySpan<char> path)
    {
        return TryMatchBraceExpansionImplementation(
            options,
            remaining,
            path,
            (p, path) => MatchPatternImplementation(p, path, MatchingStrategy.CreateCaseSensitive(this)));
    }

    public bool TryMatchBraceExpansionCaseInsensitive(ReadOnlySpan<char> options, ReadOnlySpan<char> remaining, ReadOnlySpan<char> path)
    {
        return TryMatchBraceExpansionImplementation(
            options,
            remaining,
            path,
            (p, path) => MatchPatternImplementation(p, path, MatchingStrategy.CreateCaseInsensitive(this)));
    }

    private bool TryMatchBraceExpansionImplementation(
        ReadOnlySpan<char> options,
        ReadOnlySpan<char> remaining,
        ReadOnlySpan<char> path,
        PatternMatcher patternMatcher)
    {
        const int MaxStackAllocSize = 512;
        int maxPatternLength = options.Length + remaining.Length;

        Span<char> buffer = maxPatternLength <= MaxStackAllocSize
            ? stackalloc char[maxPatternLength]
            : new char[maxPatternLength];

        var enumerator = new BraceOptionEnumerator(options);
        while (enumerator.MoveNext())
        {
            var option = enumerator.Current;

            option.CopyTo(buffer);
            remaining.CopyTo(buffer[option.Length..]);
            var testPattern = buffer[..(option.Length + remaining.Length)];

            if (patternMatcher(testPattern, path))
                return true;
        }

        return false;
    }

    // Character class matching
    public static bool MatchCharClassCaseSensitive(ReadOnlySpan<char> charClass, char c)
    {
        for (int i = 0; i < charClass.Length; i++)
        {
            if (i + 2 < charClass.Length && charClass[i + 1] == '-')
            {
                char start = charClass[i];
                char end = charClass[i + 2];
                if (c >= start && c <= end)
                    return true;

                i += 2;
            }
            else
            {
                if (charClass[i] == c)
                    return true;
            }
        }
        return false;
    }

    public static bool MatchCharClassCaseInsensitive(ReadOnlySpan<char> charClass, char c)
    {
        char cUpper = char.ToUpperInvariant(c);

        for (int i = 0; i < charClass.Length; i++)
        {
            if (i + 2 < charClass.Length && charClass[i + 1] == '-')
            {
                char start = charClass[i];
                char end = charClass[i + 2];
                if (cUpper >= start && cUpper <= end)
                    return true;

                i += 2;
            }
            else
            {
                if (charClass[i] == cUpper)
                    return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CharEqualsCaseInsensitive(char a, char b)
    {
        return a == char.ToUpperInvariant(b);
    }

    // Stack-only enumerator to avoid allocations
    [StructLayout(LayoutKind.Sequential)]
    private ref struct BraceOptionEnumerator
    {
        private readonly ReadOnlySpan<char> _options;
        private int _position;

        public BraceOptionEnumerator(ReadOnlySpan<char> options)
        {
            _options = options;
            _position = 0;
            Current = default;
        }

        public ReadOnlySpan<char> Current { get; private set; }

        public bool MoveNext()
        {
            if (_position >= _options.Length)
                return false;

            int start = _position;
            int nextComma = _options[start..].IndexOf(',');

            if (nextComma == -1)
            {
                Current = _options[start..];
                _position = _options.Length;
            }
            else
            {
                Current = _options[start..(start + nextComma)];
                _position = start + nextComma + 1;
            }

            return true;
        }
    }
}
