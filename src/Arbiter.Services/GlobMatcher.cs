using System.Runtime.CompilerServices;
using Arbiter.Services.GlobMatching;

namespace Arbiter.Services;

/// <summary>
/// Provides glob pattern matching functionality for file paths and strings.
/// Supports wildcards (*, ?), character classes ([abc]), ranges ([a-z]), and brace expansion ({jpg,png}).
/// </summary>
public sealed class GlobMatcher
{
    private readonly string _pattern;
    private readonly bool _caseSensitive;
    private readonly string? _patternUpper;
    private readonly StringComparison _stringComparison;
    private readonly PatternInfo _patternInfo;
    private readonly ComplexPatternMatcher? _complexMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobMatcher"/> class.
    /// </summary>
    /// <param name="pattern">The glob pattern to match against.</param>
    /// <param name="caseSensitive">Whether matching should be case-sensitive (default: false).</param>
    public GlobMatcher(string pattern, bool caseSensitive = false)
    {
        _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        _caseSensitive = caseSensitive;

        // Pre-compute string comparison mode once
        _stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // Pre-convert pattern to uppercase for case-insensitive matching
        if (!caseSensitive)
        {
            _patternUpper = pattern.ToUpperInvariant();
        }

        // Pre-analyze pattern once at construction
        _patternInfo = PatternAnalyzer.Analyze(_pattern.AsSpan(), caseSensitive);

        // Create complex matcher if needed
        if (_patternInfo.MatchMode == MatchMode.Complex)
        {
            _complexMatcher = new ComplexPatternMatcher(_pattern, _patternUpper, caseSensitive);
        }
    }

    /// <summary>
    /// Determines whether the specified path matches the glob pattern.
    /// </summary>
    /// <param name="path">The path to test against the pattern.</param>
    /// <returns>true if the path matches the pattern; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMatch(string path)
    {
        return IsMatch(path.AsSpan());
    }

    /// <summary>
    /// Determines whether the specified path matches the glob pattern.
    /// </summary>
    /// <param name="path">The path to test against the pattern.</param>
    /// <returns>true if the path matches the pattern; otherwise, false.</returns>
    public bool IsMatch(ReadOnlySpan<char> path)
    {
        // Fast-path optimization for simple patterns
        if (_patternInfo.MatchMode != MatchMode.Complex)
        {
            return MatchSimplePattern(path);
        }

        // Complex pattern matching
        return _complexMatcher?.Match(path) ?? false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchSimplePattern(ReadOnlySpan<char> path)
    {
        return _patternInfo.MatchMode switch
        {
            MatchMode.Exact => SimplePatternMatcher.MatchExact(path, _patternInfo.LiteralValue!, _stringComparison),
            MatchMode.StartsWith => SimplePatternMatcher.MatchStartsWith(path, _patternInfo.LiteralValue!, _stringComparison),
            MatchMode.EndsWith => SimplePatternMatcher.MatchEndsWith(path, _patternInfo.LiteralValue!, _stringComparison),
            MatchMode.Contains => SimplePatternMatcher.MatchContains(path, _patternInfo.LiteralValue!, _stringComparison),
            _ => false
        };
    }
}
