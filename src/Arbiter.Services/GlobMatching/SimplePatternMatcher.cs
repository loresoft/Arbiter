using System;
using System.Runtime.CompilerServices;

namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Handles simple pattern matching with fast-path optimizations.
/// </summary>
internal static class SimplePatternMatcher
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MatchExact(ReadOnlySpan<char> path, string literal, StringComparison comparison)
    {
        return path.Equals(literal, comparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MatchStartsWith(ReadOnlySpan<char> path, string prefix, StringComparison comparison)
    {
        return path.StartsWith(prefix, comparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MatchEndsWith(ReadOnlySpan<char> path, string suffix, StringComparison comparison)
    {
        return path.EndsWith(suffix, comparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MatchContains(ReadOnlySpan<char> path, string substring, StringComparison comparison)
    {
        return path.Contains(substring, comparison);
    }
}
