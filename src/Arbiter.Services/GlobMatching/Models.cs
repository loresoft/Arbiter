namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Pattern analysis result structure containing metadata and optimization hints.
/// </summary>
internal readonly struct PatternInfo
{
    public MatchMode MatchMode { get; init; }
    public string? LiteralValue { get; init; }
    public bool HasDoubleStar { get; init; }
    public bool HasCharacterClass { get; init; }
    public bool HasBraceExpansion { get; init; }
    public char? FirstLiteralChar { get; init; }
    public char FirstLiteralCharUpper { get; init; }
}

/// <summary>
/// Pattern characteristics discovered during scanning.
/// </summary>
internal struct PatternCharacteristics
{
    public bool HasStar;
    public bool HasDoubleStar;
    public bool HasQuestion;
    public bool HasCharClass;
    public bool HasBrace;
    public int StarCount;

    public readonly bool HasNoComplexFeatures()
    {
        return !HasDoubleStar && !HasQuestion && !HasCharClass && !HasBrace;
    }
}

/// <summary>
/// Information about the first literal character in a pattern.
/// </summary>
internal struct FirstLiteralChar
{
    public char? Char;
    public char Upper;
}

/// <summary>
/// Match mode enumeration for fast-path optimization.
/// </summary>
internal enum MatchMode
{
    Complex,      // Full pattern matching required
    Exact,        // Exact string match (no wildcards)
    StartsWith,   // Pattern like "prefix*"
    EndsWith,     // Pattern like "*suffix"
    Contains      // Pattern like "*substring*"
}
