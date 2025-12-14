using System;

namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Analyzes glob patterns to detect optimization opportunities and pattern characteristics.
/// </summary>
internal static class PatternAnalyzer
{
    /// <summary>
    /// Analyzes a glob pattern and returns optimization metadata.
    /// </summary>
    public static PatternInfo Analyze(ReadOnlySpan<char> pattern, bool caseSensitive)
    {
        if (pattern.Length == 0)
        {
            return PatternInfoBuilder.Create()
                .AsExact(string.Empty)
                .Build();
        }

        var characteristics = ScanCharacteristics(pattern);
        var firstLiteral = FindFirstLiteralCharacter(pattern, caseSensitive);

        // Detect simple patterns for fast-path optimization
        if (characteristics.HasNoComplexFeatures())
        {
            return AnalyzeSimplePattern(pattern, characteristics.StarCount, firstLiteral);
        }

        // Complex pattern
        return PatternInfoBuilder.Create()
            .AsComplex()
            .WithCharacteristics(characteristics)
            .WithFirstLiteral(firstLiteral)
            .Build();
    }

    private static PatternCharacteristics ScanCharacteristics(ReadOnlySpan<char> pattern)
    {
        var result = new PatternCharacteristics();

        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];

            switch (c)
            {
                case '*':
                    result.HasStar = true;
                    result.StarCount++;

                    // Check for double star
                    if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                    {
                        result.HasDoubleStar = true;
                        i++; // Skip next star
                    }
                    break;

                case '?':
                    result.HasQuestion = true;
                    break;

                case '[':
                    result.HasCharClass = true;
                    break;

                case '{':
                    result.HasBrace = true;
                    break;
            }
        }

        return result;
    }

    private static FirstLiteralChar FindFirstLiteralCharacter(ReadOnlySpan<char> pattern, bool caseSensitive)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (c != '*' && c != '?' && c != '[' && c != '{')
            {
                return new FirstLiteralChar
                {
                    Char = c,
                    Upper = caseSensitive ? '\0' : char.ToUpperInvariant(c)
                };
            }
        }

        return default;
    }

    private static PatternInfo AnalyzeSimplePattern(
        ReadOnlySpan<char> pattern, 
        int starCount, 
        FirstLiteralChar firstLiteral)
    {
        // No wildcards at all - exact match
        if (starCount == 0)
        {
            return PatternInfoBuilder.Create()
                .AsExact(pattern.ToString())
                .WithFirstLiteral(firstLiteral)
                .Build();
        }

        // Single star patterns
        if (starCount == 1)
        {
            int starPos = pattern.IndexOf('*');

            if (starPos == 0)
            {
                // Pattern: *suffix
                return PatternInfoBuilder.Create()
                    .AsEndsWith(pattern[1..].ToString())
                    .WithFirstLiteral(firstLiteral)
                    .Build();
            }

            if (starPos == pattern.Length - 1)
            {
                // Pattern: prefix*
                return PatternInfoBuilder.Create()
                    .AsStartsWith(pattern[..^1].ToString())
                    .WithFirstLiteral(firstLiteral)
                    .Build();
            }
        }

        // Multiple stars or star in middle - treat as complex
        return PatternInfoBuilder.Create()
            .AsComplex()
            .WithFirstLiteral(firstLiteral)
            .Build();
    }
}
