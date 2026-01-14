using System.Buffers;

namespace Arbiter.Services;

/// <summary>
/// Provides glob pattern matching functionality for file paths and strings.
/// Supports wildcards (*, ?), character classes ([abc]), negated character classes ([!abc]),
/// ranges ([a-z]), double wildcards (**), and brace expansion ({jpg,png}).
/// </summary>
/// <remarks>
/// <para>
/// The GlobMatcher class implements standard glob pattern matching with the following features:
/// </para>
/// <list type="bullet">
/// <item><description><c>*</c> - Matches zero or more characters within a single path segment (does not cross directory boundaries)</description></item>
/// <item><description><c>?</c> - Matches exactly one character (does not match path separators)</description></item>
/// <item><description><c>**</c> - Matches zero or more path segments (crosses directory boundaries)</description></item>
/// <item><description><c>[abc]</c> - Matches any single character in the set</description></item>
/// <item><description><c>[a-z]</c> - Matches any single character in the range</description></item>
/// <item><description><c>[!abc]</c> - Matches any single character NOT in the set</description></item>
/// <item><description><c>{txt,csv}</c> - Matches any of the comma-separated alternatives</description></item>
/// </list>
/// <para>
/// Pattern matching is case-insensitive by default but can be made case-sensitive via the constructor.
/// Both forward slash (/) and backslash (\) are treated as path separators.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic wildcard matching
/// var matcher = new GlobMatcher("*.txt");
/// bool match = matcher.IsMatch("file.txt");  // true
///
/// // Double wildcard for recursive matching
/// var matcher2 = new GlobMatcher("**/*.cs");
/// bool match2 = matcher2.IsMatch("src/Program.cs");  // true
///
/// // Brace expansion
/// var matcher3 = new GlobMatcher("file.{txt,csv}");
/// bool match3 = matcher3.IsMatch("file.txt");  // true
///
/// // Case-sensitive matching
/// var matcher4 = new GlobMatcher("Test.txt", caseSensitive: true);
/// bool match4 = matcher4.IsMatch("test.txt");  // false
/// </code>
/// </example>
public class GlobMatcher
{
    private readonly ReadOnlyMemory<char> _pattern;
    private readonly bool _caseSensitive;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobMatcher"/> class with the specified pattern.
    /// </summary>
    /// <param name="pattern">The glob pattern to match against. Cannot be null.</param>
    /// <param name="caseSensitive">
    /// <c>true</c> to perform case-sensitive matching; <c>false</c> for case-insensitive matching.
    /// Default is <c>false</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Case-insensitive matching (default)
    /// var matcher1 = new GlobMatcher("*.TXT");
    /// bool match1 = matcher1.IsMatch("file.txt");  // true
    ///
    /// // Case-sensitive matching
    /// var matcher2 = new GlobMatcher("*.TXT", caseSensitive: true);
    /// bool match2 = matcher2.IsMatch("file.txt");  // false
    /// </code>
    /// </example>
    public GlobMatcher(string pattern, bool caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        _pattern = pattern.AsMemory();
        _caseSensitive = caseSensitive;
    }


    /// <summary>
    /// Determines whether the specified path matches the glob pattern.
    /// </summary>
    /// <param name="path">The path to test against the pattern.</param>
    /// <returns>
    /// <c>true</c> if the path matches the pattern; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses zero-allocation span-based matching for optimal performance.
    /// The same <see cref="GlobMatcher"/> instance can be reused to match multiple paths
    /// against the same pattern.
    /// </para>
    /// <para>
    /// Path separators (/ and \) are normalized during matching, so patterns work
    /// consistently across Windows and Unix-like systems.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var matcher = new GlobMatcher("src/**/*.cs");
    ///
    /// // Match against multiple paths
    /// bool match1 = matcher.IsMatch("src/Program.cs");           // true
    /// bool match2 = matcher.IsMatch("src/Utils/Helper.cs");      // true
    /// bool match3 = matcher.IsMatch("test/Program.cs");          // false
    ///
    /// // Works with ReadOnlySpan&lt;char&gt; for zero allocations
    /// ReadOnlySpan&lt;char&gt; pathSpan = "src/file.cs".AsSpan();
    /// bool match4 = matcher.IsMatch(pathSpan);                   // true
    /// </code>
    /// </example>
    public bool IsMatch(ReadOnlySpan<char> path)
    {
        return MatchPattern(_pattern.Span, path);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Pattern patching logic")]
    private bool MatchPattern(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path)
    {
        // Early handling for empty path: only empty patterns or patterns with only wildcards can match
        // Examples:
        //   - Pattern "" with path "" -> true (both empty)
        //   - Pattern "*" with path "" -> true (* matches zero chars)
        //   - Pattern "**" with path "" -> true (** matches zero segments)
        //   - Pattern "test.txt" with path "" -> false (literal chars need input)
        if (path.Length == 0)
        {
            return ConsumeRemainingPattern(pattern, 0);
        }

        // Pattern matching state: current positions in both pattern and path
        int patternIndex = 0;
        int pathIndex = 0;

        // Backtracking state: stores the position of the last '*' wildcard
        // Used to backtrack when a match fails (greedy matching strategy)
        int starPatternIndex = -1;
        int starPathIndex = -1;

        // Main matching loop: iterate through the path string
        while (pathIndex < path.Length)
        {
            // Check if we still have pattern characters to process
            if (patternIndex < pattern.Length)
            {
                char patternChar = pattern[patternIndex];

                // Handle brace expansion {a,b,c} - try each alternative inline
                if (patternChar == '{')
                {
                    return HandleBraceExpansion(pattern, path, patternIndex, pathIndex);
                }

                // Handle character class [abc] or [a-z] or [!abc]
                // Matches a single character against a set or range
                if (patternChar == '[')
                {
                    if (TryMatchCharacterClass(pattern, path, ref patternIndex, ref pathIndex, starPatternIndex, ref starPathIndex))
                        continue;

                    return false;
                }

                // Handle double wildcard ** - matches zero or more path segments
                // Can cross directory boundaries (e.g., **/file.txt)
                if (patternChar == '*' && patternIndex + 1 < pattern.Length && pattern[patternIndex + 1] == '*')
                {
                    return HandleDoubleWildcard(pattern, path, patternIndex, pathIndex);
                }

                // Handle single wildcard * - matches zero or more characters within a segment
                // Does not cross directory boundaries
                if (patternChar == '*')
                {
                    // Save this position for potential backtracking
                    starPatternIndex = patternIndex;
                    starPathIndex = pathIndex;
                    patternIndex++;
                    continue;
                }

                // Handle question mark ? - matches exactly one character
                // Does not match path separators (similar to single wildcard *)
                if (patternChar == '?')
                {
                    // Question mark should NOT match path separators
                    // Only matches regular characters within a path segment
                    // Example: "a?c" should NOT match "a/c" or "a\c"
                    if (IsPathSeparator(path[pathIndex]))
                        return false;

                    patternIndex++;
                    pathIndex++;
                    continue;
                }

                // Handle regular character - must match exactly (case-sensitive or not)
                if (CharEquals(patternChar, path[pathIndex]))
                {
                    patternIndex++;
                    pathIndex++;
                    continue;
                }

                // Character doesn't match - try backtracking to the last '*'
                // This implements greedy matching: * initially matches zero chars,
                // then we expand it one character at a time on backtracking
                if (TryBacktrack(ref patternIndex, ref pathIndex, starPatternIndex, ref starPathIndex, path))
                    continue;

                return false;
            }
            else
            {
                // Pattern exhausted but path still has characters
                // Try backtracking to see if the last '*' can consume more
                if (TryBacktrack(ref patternIndex, ref pathIndex, starPatternIndex, ref starPathIndex, path))
                    continue;

                return false;
            }
        }

        // Path is exhausted, check if remaining pattern is all wildcards
        // Allows patterns like "file*" or "file**" to match "file"
        return ConsumeRemainingPattern(pattern, patternIndex);
    }

    private static bool TryBacktrack(
        ref int patternIndex,
        ref int pathIndex,
        int starPatternIndex,
        ref int starPathIndex,
        ReadOnlySpan<char> path)
    {
        // No star to backtrack to
        if (starPatternIndex == -1)
            return false;

        // Single wildcard * should NOT match path separators (/ or \)
        // This ensures * only matches within a single path segment
        // Example: pattern "*.txt" should NOT match "folder/file.txt"
        // Only ** (double wildcard) can cross directory boundaries
        if (starPathIndex < path.Length && IsPathSeparator(path[starPathIndex]))
            return false;

        // Standard backtracking: reset pattern to after the '*', consume one more path character
        patternIndex = starPatternIndex + 1;
        starPathIndex++;
        pathIndex = starPathIndex;

        return true;
    }

    private static bool ConsumeRemainingPattern(
        ReadOnlySpan<char> pattern,
        int patternIndex)
    {
        // This method is called after the path has been fully consumed (pathIndex >= path.Length)
        // It checks if any remaining pattern characters can be satisfied
        // Only wildcards (* and **) can match empty strings, all other characters require path input

        // Iterate through any remaining pattern characters
        while (patternIndex < pattern.Length)
        {
            // If we encounter a non-wildcard character, the pattern cannot match
            // Examples that return false:
            //   - Pattern "test" with path "tes" leaves 't' in pattern -> false
            //   - Pattern "file.txt" with path "file" leaves '.txt' -> false
            if (pattern[patternIndex] != '*')
                return false;

            // Check if this is a double wildcard **
            // Double wildcards can match zero or more path segments (including empty)
            if (patternIndex + 1 < pattern.Length && pattern[patternIndex + 1] == '*')
            {
                // Skip past the two asterisks
                patternIndex += 2;

                // Skip optional path separator after **
                // This normalizes patterns like "file**/" and "file**" to behave identically
                // Example: pattern "src/**" with path "src" should match (** matches empty)
                if (patternIndex < pattern.Length && IsPathSeparator(pattern[patternIndex]))
                    patternIndex++;
            }
            else
            {
                // Single wildcard * can match zero or more characters (including empty)
                // Examples that return true:
                //   - Pattern "file*" with path "file" -> * matches empty string
                //   - Pattern "test***" with path "test" -> all * match empty
                patternIndex++;
            }
        }

        // All remaining pattern characters were wildcards that can match empty strings
        // The pattern successfully matches the path
        // Examples that return true:
        //   - Pattern "test*" with path "test" -> true
        //   - Pattern "file**" with path "file" -> true
        //   - Pattern "src/**/foo.txt" with path "src/foo.txt" -> true (** matches zero dirs)
        return true;
    }


    private bool HandleDoubleWildcard(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        int patternIndex,
        int pathIndex)
    {
        // Double wildcard ** is a special glob pattern that matches:
        // - Zero or more directory levels (e.g., "src/**/*.cs" matches "src/file.cs" and "src/a/b/c/file.cs")
        // - Any characters including directory separators ('/' or '\')
        // - Empty string (e.g., "**/file.txt" matches "file.txt")

        // Skip past the two asterisks
        patternIndex += 2;

        // Normalize the pattern by skipping an optional path separator after **
        // This allows patterns like "**/" and "**" to behave identically
        // Examples:
        //   - "**/file.txt" and "file.txt" both match "file.txt"
        //   - "src/**/file.cs" matches "src/file.cs" (zero directories)
        if (patternIndex < pattern.Length && IsPathSeparator(pattern[patternIndex]))
            patternIndex++;

        // Special case: if ** is at the end of the pattern (or followed only by separator)
        // it matches everything remaining in the path
        // Examples:
        //   - "src/**" matches "src/anything/here.txt"
        //   - "logs/**" matches "logs/2024/01/file.log"
        if (patternIndex >= pattern.Length)
            return true;

        // Try to match the remainder of the pattern from each possible position in the path
        // This implements the "zero or more directories" behavior:
        // - i = pathIndex: ** matches zero characters (try immediate match)
        // - i = pathIndex + 1: ** matches one character
        // - i = pathIndex + 2: ** matches two characters
        // - ... and so on until the end of the path
        //
        // Example with pattern "**/test.txt" and path "a/b/test.txt":
        //   - Try matching "test.txt" at position 0: "a/b/test.txt" - no match
        //   - Try matching "test.txt" at position 1: "/b/test.txt" - no match
        //   - Try matching "test.txt" at position 2: "b/test.txt" - no match
        //   - Try matching "test.txt" at position 3: "/test.txt" - no match
        //   - Try matching "test.txt" at position 4: "test.txt" - MATCH!
        for (int i = pathIndex; i <= path.Length; i++)
        {
            // Recursively match the rest of the pattern against the remaining path
            // If successful, the entire pattern matches
            if (MatchPattern(pattern[patternIndex..], path[i..]))
                return true;
        }

        // No position in the path matched the remainder of the pattern
        return false;
    }

    private bool TryMatchCharacterClass(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        ref int patternIndex,
        ref int pathIndex,
        int starPatternIndex,
        ref int starPathIndex)
    {
        // Find the closing bracket ']' for this character class
        // Character classes are patterns like [abc], [a-z], [!0-9], etc.
        int closeIndex = FindClosingBracket(pattern, patternIndex);

        // Valid character class found (has matching closing bracket)
        if (closeIndex > patternIndex)
        {
            // Extract the content between '[' and ']'
            // Examples:
            //   - "[abc]" -> charClass = "abc"
            //   - "[a-z]" -> charClass = "a-z"
            //   - "[!0-9]" -> charClass = "!0-9"
            var charClass = pattern.Slice(patternIndex + 1, closeIndex - patternIndex - 1);

            // Try to match the current path character against the character class
            if (MatchCharacterClass(charClass, path[pathIndex]))
            {
                // Match successful! Advance both indices past the matched portions
                // Pattern advances past the entire character class "[...]"
                // Path advances past the single matched character
                patternIndex = closeIndex + 1;
                pathIndex++;
                return true;
            }

            // Character class didn't match - try backtracking to the last '*'
            // This allows patterns like "*[abc]" to work correctly
            // Example: pattern "*[xyz]" matching path "abcz"
            //   - First '*' matches "abc", then try to match 'z' with [xyz]
            return TryBacktrackForCharacterClass(ref patternIndex, ref pathIndex, starPatternIndex, ref starPathIndex, path);
        }

        // Malformed character class (no closing ']' found)
        // Treat the '[' as a literal character instead of special syntax
        // Example: pattern "[test.txt" tries to match literal '[' character
        if (CharEquals(pattern[patternIndex], path[pathIndex]))
        {
            // The literal '[' matched, continue with next characters
            patternIndex++;
            pathIndex++;
            return true;
        }

        // Literal '[' didn't match - try backtracking to the last '*'
        // This allows patterns like "*[" to work even with malformed brackets
        return TryBacktrackForCharacterClass(ref patternIndex, ref pathIndex, starPatternIndex, ref starPathIndex, path);
    }

    private static bool TryBacktrackForCharacterClass(
        ref int patternIndex,
        ref int pathIndex,
        int starPatternIndex,
        ref int starPathIndex,
        ReadOnlySpan<char> path)
    {
        // No star to backtrack to
        if (starPatternIndex == -1)
            return false;

        // Single wildcard * should NOT match path separators
        // Only ** can cross directory boundaries
        if (starPathIndex < path.Length && IsPathSeparator(path[starPathIndex]))
            return false;

        // Backtrack: reset pattern to after the '*', consume one more path character
        patternIndex = starPatternIndex + 1;
        starPathIndex++;
        pathIndex = starPathIndex;
        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Pattern patching logic")]
    private bool MatchCharacterClass(
        ReadOnlySpan<char> charClass,
        char pathChar)
    {
        // Empty character class cannot match anything
        // Example: "[]" is malformed and returns false
        if (charClass.IsEmpty)
            return false;

        // Check if this is a negated character class
        // Negated classes match any character NOT in the set
        // Example: "[!abc]" matches 'd', 'e', 'z' but not 'a', 'b', or 'c'
        bool negated = charClass[0] == '!';
        if (negated)
            charClass = charClass[1..]; // Skip the '!' character

        // Track whether we found a match in the character class
        bool matched = false;

        // Iterate through the character class looking for matches
        for (int i = 0; i < charClass.Length; i++)
        {
            // Check if this is a range pattern (e.g., "a-z", "0-9", "A-Z")
            // A range requires: current char + '-' + end char (minimum 3 characters)
            // Examples:
            //   - "[a-z]" matches any lowercase letter
            //   - "[0-9]" matches any digit
            //   - "[A-Za-z]" matches any letter (two ranges combined)
            if (i + 2 < charClass.Length && charClass[i + 1] == '-')
            {
                char rangeStart = charClass[i];     // e.g., 'a' in "a-z"
                char rangeEnd = charClass[i + 2];   // e.g., 'z' in "a-z"

                // Check if the path character falls within this range
                // Respects case sensitivity setting
                if (IsInRange(pathChar, rangeStart, rangeEnd))
                {
                    matched = true;
                    break; // Found a match, no need to continue
                }

                // Skip past the range pattern (3 characters: start, '-', end)
                i += 2;
            }
            // Check for individual character match (not a range)
            // Examples:
            //   - "[abc]" matches 'a', 'b', or 'c'
            //   - "[xyz123]" matches 'x', 'y', 'z', '1', '2', or '3'
            else if (CharEquals(charClass[i], pathChar))
            {
                matched = true;
                break; // Found a match, no need to continue
            }
        }

        // Return the final result based on negation
        // For normal classes: return true if matched
        // For negated classes: return true if NOT matched
        // Examples:
        //   - "[abc]" with 'a' -> matched=true, negated=false -> returns true
        //   - "[abc]" with 'd' -> matched=false, negated=false -> returns false
        //   - "[!abc]" with 'a' -> matched=true, negated=true -> returns false
        //   - "[!abc]" with 'd' -> matched=false, negated=true -> returns true
        return negated ? !matched : matched;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Pattern patching logic")]
    private bool HandleBraceExpansion(
        ReadOnlySpan<char> pattern,
        ReadOnlySpan<char> path,
        int patternIndex,
        int pathIndex)
    {
        // Find the matching closing brace for this opening brace
        // Handles nested braces correctly by tracking depth
        int closeBrace = FindCloseBrace(pattern, patternIndex);
        if (closeBrace == -1)
        {
            // Malformed brace pattern (no matching '}')
            // Treat the '{' as a literal character and continue matching
            if (CharEquals(pattern[patternIndex], path[pathIndex]))
                return MatchPattern(pattern[(patternIndex + 1)..], path[(pathIndex + 1)..]);

            return false;
        }

        // Split the pattern into three parts:
        // 1. prefix: everything before the '{'  (e.g., "file." in "file.{txt,csv}")
        // 2. alternatives: comma-separated options inside braces (e.g., "txt,csv")
        // 3. suffix: everything after the '}'  (e.g., ".bak" in "{a,b}.bak")
        var prefix = pattern[..patternIndex];
        var alternatives = pattern[(patternIndex + 1)..closeBrace];
        var suffix = pattern[(closeBrace + 1)..];

        // Quick optimization: verify the path starts with the prefix
        // If it doesn't, no need to try any alternatives
        var matchLength = Math.Min(prefix.Length, path.Length);
        if (!MatchesPrefix(prefix, path[..matchLength]))
            return false;

        // Rent a buffer from the pool to build expanded patterns
        // This avoids allocating new strings for each alternative
        int maxPatternLength = prefix.Length + alternatives.Length + suffix.Length;
        var buffer = ArrayPool<char>.Shared.Rent(maxPatternLength);

        try
        {
            // Iterate through alternatives separated by commas
            // Example: "txt,csv,json" -> ["txt", "csv", "json"]
            int alternativeStart = 0;
            for (int i = 0; i <= alternatives.Length; i++)
            {
                // Split on ',' but only at the top-level brace depth
                // This handles nested braces: "{a,{b,c}}" has two alternatives: "a" and "{b,c}"
                if (i == alternatives.Length || (alternatives[i] == ',' && GetBraceDepth(alternatives[..i]) == 0))
                {
                    // Extract the current alternative
                    var alternative = alternatives[alternativeStart..i];

                    // Build the expanded pattern by concatenating: prefix + alternative + suffix
                    // Example: "file." + "txt" + "" = "file.txt"
                    int pos = 0;
                    prefix.CopyTo(buffer.AsSpan(pos));

                    pos += prefix.Length;
                    alternative.CopyTo(buffer.AsSpan(pos));

                    pos += alternative.Length;
                    suffix.CopyTo(buffer.AsSpan(pos));

                    pos += suffix.Length;

                    var expandedPattern = buffer.AsSpan(0, pos);

                    // Recursively match the expanded pattern against the path
                    // This handles nested braces and other glob patterns in alternatives
                    if (MatchPattern(expandedPattern, path))
                        return true;

                    // Move to the next alternative (skip the comma)
                    alternativeStart = i + 1;
                }
            }

            // No alternative matched
            return false;
        }
        finally
        {
            // Always return the rented buffer to the pool
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    private bool MatchesPrefix(
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> path)
    {
        // Empty prefix always matches
        if (prefix.IsEmpty)
            return true;

        // Quick validation: prefix cannot be longer than the path
        if (prefix.Length > path.Length)
            return false;

        // Check each character in the prefix against the path
        for (int i = 0; i < prefix.Length; i++)
        {
            // If the prefix contains wildcards or special characters,
            // we can't do simple character-by-character comparison.
            // In this case, return true to allow the full pattern matching
            // algorithm to handle it in the brace expansion alternatives.
            //
            // This is an optimization that avoids false negatives:
            // - Pattern "*.{txt,csv}" with path "file.txt" - prefix "*" needs full matching
            // - Pattern "test?.{a,b}" with path "test1.a" - prefix "test?" needs full matching
            // - Pattern "[abc].{x,y}" with path "a.x" - prefix "[abc]" needs full matching
            //
            // Returning true here means "this prefix MIGHT match, let the full algorithm decide"
            // rather than "this prefix DEFINITELY matches"
            if (prefix[i] == '*' || prefix[i] == '?' || prefix[i] == '[' || prefix[i] == '{')
            {
                return true;
            }

            // Regular character comparison (respects case sensitivity setting)
            // If any character doesn't match, the prefix doesn't match
            if (!CharEquals(prefix[i], path[i]))
                return false;
        }

        // All prefix characters matched exactly (no wildcards encountered)
        return true;
    }


    private bool CharEquals(char a, char b)
    {
        if (_caseSensitive)
            return a == b;

        // Case-insensitive comparison
        return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);
    }

    private bool IsInRange(char ch, char start, char end)
    {
        if (_caseSensitive)
            return ch >= start && ch <= end;

        // Case-insensitive range comparison
        char chUpper = char.ToUpperInvariant(ch);
        char chLower = char.ToLowerInvariant(ch);
        char startUpper = char.ToUpperInvariant(start);
        char startLower = char.ToLowerInvariant(start);
        char endUpper = char.ToUpperInvariant(end);
        char endLower = char.ToLowerInvariant(end);

        // Check if the character falls within the range in either case form
        return (chUpper >= startUpper && chUpper <= endUpper) ||
               (chLower >= startLower && chLower <= endLower);
    }

    private static bool IsPathSeparator(char c) => c == '/' || c == '\\';


    private static int FindClosingBracket(ReadOnlySpan<char> pattern, int startIndex)
    {
        for (int i = startIndex + 1; i < pattern.Length; i++)
        {
            if (pattern[i] == ']')
                return i;
        }

        return -1;
    }

    private static int FindCloseBrace(ReadOnlySpan<char> pattern, int openBrace)
    {
        int depth = 1;
        for (int i = openBrace + 1; i < pattern.Length; i++)
        {
            if (pattern[i] == '{')
            {
                depth++;
            }
            else if (pattern[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private static int GetBraceDepth(ReadOnlySpan<char> span)
    {
        int depth = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == '{')
                depth++;
            else if (span[i] == '}')
                depth--;
        }

        return depth;
    }
}
