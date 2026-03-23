using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class GlobMatcherTests
{
    [Test]
    [Arguments("test.txt", "test.txt", true)]
    [Arguments("test.txt", "test2.txt", false)]
    [Arguments("test.txt", "TEST.TXT", true)]
    [Arguments("foo/bar.txt", "foo/bar.txt", true)]
    [Arguments("foo\\bar.txt", "foo\\bar.txt", true)]
    public void IsMatch_ExactMatch_CaseInsensitive(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test.txt", "test.txt", true)]
    [Arguments("test.txt", "TEST.TXT", false)]
    [Arguments("Test.txt", "test.txt", false)]
    [Arguments("foo/bar.txt", "FOO/BAR.TXT", false)]
    public void IsMatch_ExactMatch_CaseSensitive(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern, caseSensitive: true);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("*.txt", "test.txt", true)]
    [Arguments("*.txt", "file.txt", true)]
    [Arguments("*.txt", "test.csv", false)]
    [Arguments("test*", "test.txt", true)]
    [Arguments("test*", "testing", true)]
    [Arguments("test*", "mytest", false)]
    [Arguments("*test*", "mytest.txt", true)]
    [Arguments("*test*", "testing", true)]
    [Arguments("*test*", "file.txt", false)]
    public void IsMatch_SingleWildcard(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("**/*.txt", "test.txt", true)]
    [Arguments("**/*.txt", "foo/test.txt", true)]
    [Arguments("**/*.txt", "foo/bar/test.txt", true)]
    [Arguments("**/*.txt", "foo/bar/baz/test.txt", true)]
    [Arguments("**/*.txt", "test.csv", false)]
    [Arguments("src/**/*.cs", "src/file.cs", true)]
    [Arguments("src/**/*.cs", "src/folder/file.cs", true)]
    [Arguments("src/**/*.cs", "src/a/b/c/file.cs", true)]
    [Arguments("src/**/*.cs", "other/file.cs", false)]
    [Arguments("**/test/**", "a/test/b", true)]
    [Arguments("**/test/**", "test/b", true)]
    [Arguments("**/test/**", "a/test", false)]
    public void IsMatch_DoubleWildcard(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("**", "anything", true)]
    [Arguments("**", "foo/bar", true)]
    [Arguments("**", "foo/bar/baz", true)]
    [Arguments("prefix/**", "prefix/anything", true)]
    [Arguments("prefix/**", "prefix/foo/bar", true)]
    [Arguments("prefix/**", "other/foo", false)]
    public void IsMatch_DoubleWildcardAtEnd(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test?.txt", "test1.txt", true)]
    [Arguments("test?.txt", "testa.txt", true)]
    [Arguments("test?.txt", "test.txt", false)]
    [Arguments("test?.txt", "test12.txt", false)]
    [Arguments("?est.txt", "test.txt", true)]
    [Arguments("?est.txt", "best.txt", true)]
    [Arguments("?est.txt", "est.txt", false)]
    [Arguments("t?st.txt", "test.txt", true)]
    [Arguments("t?st.txt", "tast.txt", true)]
    [Arguments("t?st.txt", "txt.txt", false)]
    public void IsMatch_QuestionMark(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("[abc].txt", "a.txt", true)]
    [Arguments("[abc].txt", "b.txt", true)]
    [Arguments("[abc].txt", "c.txt", true)]
    [Arguments("[abc].txt", "d.txt", false)]
    [Arguments("test[123].txt", "test1.txt", true)]
    [Arguments("test[123].txt", "test2.txt", true)]
    [Arguments("test[123].txt", "test4.txt", false)]
    public void IsMatch_CharacterClass(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("[a-z].txt", "a.txt", true)]
    [Arguments("[a-z].txt", "m.txt", true)]
    [Arguments("[a-z].txt", "z.txt", true)]
    [Arguments("[a-z].txt", "A.txt", true)] // Case insensitive by default
    [Arguments("[a-z].txt", "1.txt", false)]
    [Arguments("[0-9].txt", "0.txt", true)]
    [Arguments("[0-9].txt", "5.txt", true)]
    [Arguments("[0-9].txt", "9.txt", true)]
    [Arguments("[0-9].txt", "a.txt", false)]
    public void IsMatch_CharacterRange(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("[!abc].txt", "d.txt", true)]
    [Arguments("[!abc].txt", "z.txt", true)]
    [Arguments("[!abc].txt", "a.txt", false)]
    [Arguments("[!abc].txt", "b.txt", false)]
    [Arguments("[!0-9].txt", "a.txt", true)]
    [Arguments("[!0-9].txt", "5.txt", false)]
    public void IsMatch_NegatedCharacterClass(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test.{txt,csv}", "test.txt", true)]
    [Arguments("test.{txt,csv}", "test.csv", true)]
    [Arguments("test.{txt,csv}", "test.pdf", false)]
    [Arguments("{foo,bar}.txt", "foo.txt", true)]
    [Arguments("{foo,bar}.txt", "bar.txt", true)]
    [Arguments("{foo,bar}.txt", "baz.txt", false)]
    [Arguments("file.{jpg,png,gif}", "file.jpg", true)]
    [Arguments("file.{jpg,png,gif}", "file.png", true)]
    [Arguments("file.{jpg,png,gif}", "file.gif", true)]
    [Arguments("file.{jpg,png,gif}", "file.bmp", false)]
    public void IsMatch_BraceExpansion(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("*.{cs,txt}", "Program.cs", true)]
    [Arguments("*.{cs,txt}", "readme.txt", true)]
    [Arguments("*.{cs,txt}", "data.json", false)]
    [Arguments("**/*.{cs,vb}", "src/Program.cs", true)]
    [Arguments("**/*.{cs,vb}", "src/Module.vb", true)]
    [Arguments("**/*.{cs,vb}", "src/file.fs", false)]
    public void IsMatch_BraceExpansionWithWildcards(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("", "", true)]
    [Arguments("", "test.txt", false)]
    [Arguments("*.txt", "", false)]
    [Arguments("*", "", true)]
    [Arguments("**", "", true)]
    [Arguments("?", "", false)]
    [Arguments("[abc]", "", false)]
    [Arguments("{a,b}", "", false)]
    public void IsMatch_EmptyStrings(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("[.txt", "a.txt", false)]
    [Arguments("[.txt", "[.txt", true)]
    [Arguments("{.txt", "a.txt", false)]
    [Arguments("{.txt", "{.txt", true)]
    public void IsMatch_MalformedPatterns_TreatedAsLiterals(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("src/**/*.cs", "src/Controllers/HomeController.cs", true)]
    [Arguments("test/**/*Tests.cs", "test/Unit/ServiceTests.cs", true)]
    [Arguments("*.{cs,fs,vb}", "Program.cs", true)]
    [Arguments("**/bin/**", "project/bin/Debug/file.dll", true)]
    [Arguments("**/obj/**", "project/obj/Release/file.dll", true)]
    [Arguments("docs/**/*.{md,txt}", "docs/guides/intro.md", true)]
    public void IsMatch_ComplexRealWorldPatterns(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("a*b*c*d*e*f", "aXbYcZdWeVf", true)]
    [Arguments("a*b*c", "abbbbbbc", true)]
    [Arguments("a*b*c", "axbxc", true)]
    [Arguments("a*b*c", "axxxx", false)]
    [Arguments("*a*b*c*", "xaxbxcx", true)]
    public void IsMatch_MultipleWildcards(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    public void IsMatch_WithSpan_Works()
    {
        var matcher = new GlobMatcher("*.txt");
        var path = "test.txt".AsSpan();

        var result = matcher.IsMatch(path);

        result.Should().BeTrue();
    }

    [Test]
    public void IsMatch_WithSpan_DoesNotMatch()
    {
        var matcher = new GlobMatcher("*.txt");
        var path = "test.csv".AsSpan();

        var result = matcher.IsMatch(path);

        result.Should().BeFalse();
    }

    [Test]
    public void Constructor_NullPattern_ThrowsArgumentNullException()
    {
        var action = () => new GlobMatcher(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [Arguments("logs/**/*.log", "logs/2024/01/app.log", true)]
    [Arguments("logs/**/*.log", "logs/app.log", true)]
    [Arguments("logs/**/*.log", "logs/server/debug/trace.log", true)]
    [Arguments("src/**/test/**/*.cs", "src/project/test/unit/Test.cs", true)]
    [Arguments("src/**/test/**/*.cs", "src/test/Test.cs", true)]
    public void IsMatch_DoubleWildcard_MatchesMultipleLevels(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test[A-Z].txt", "testA.txt", true)]
    [Arguments("test[A-Z].txt", "testZ.txt", true)]
    [Arguments("test[A-Z].txt", "testa.txt", true)] // Case insensitive
    public void IsMatch_CharacterRange_CaseInsensitive(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test[A-Z].txt", "testA.txt", true)]
    [Arguments("test[A-Z].txt", "testZ.txt", true)]
    [Arguments("test[A-Z].txt", "testa.txt", false)] // Case sensitive
    public void IsMatch_CharacterRange_CaseSensitive(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern, caseSensitive: true);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("*.txt", "file.txt", true)]
    [Arguments("*.txt", "FILE.TXT", true)]
    [Arguments("*.TXT", "file.txt", true)]
    [Arguments("Test.txt", "test.txt", true)]
    public void IsMatch_PatternAndPath_CaseInsensitiveByDefault(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("file.*", "file.txt", true)]
    [Arguments("file.*", "file.csv", true)]
    [Arguments("file.*", "file.", true)]
    [Arguments("file.*", "file", false)]
    public void IsMatch_WildcardAfterDot(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test**file.txt", "testfile.txt", true)]
    [Arguments("test**file.txt", "test123file.txt", true)]
    [Arguments("test**file.txt", "test/sub/file.txt", true)]
    public void IsMatch_DoubleWildcard_WithoutSlashes(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("a?c", "abc", true)]
    [Arguments("a?c", "aXc", true)]
    [Arguments("a?c", "ac", false)]
    [Arguments("???", "abc", true)]
    [Arguments("???", "ab", false)]
    [Arguments("???", "abcd", false)]
    public void IsMatch_QuestionMark_MatchesSingleCharacter(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("**/node_modules/**", "project/node_modules/package/file.js", true)]
    [Arguments("**/node_modules/**", "deep/nested/node_modules/lib/index.js", true)]
    [Arguments("**/.git/**", "repo/.git/objects/abc", true)]
    [Arguments("**/bin/**/*.dll", "src/project/bin/Debug/app.dll", true)]
    public void IsMatch_GitIgnore_StylePatterns(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    public void IsMatch_Performance_ManyMatches()
    {
        var matcher = new GlobMatcher("**/*.cs");

        for (int i = 0; i < 1000; i++)
        {
            var result = matcher.IsMatch($"src/folder{i}/File{i}.cs");
            result.Should().BeTrue();
        }
    }

    [Test]
    public void IsMatch_ConsistentResults()
    {
        var matcher = new GlobMatcher("*.txt");
        var path = "test.txt";

        var result1 = matcher.IsMatch(path);
        var result2 = matcher.IsMatch(path);
        var result3 = matcher.IsMatch(path);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Test]
    [Arguments("[a-zA-Z0-9].txt", "a.txt", true)]
    [Arguments("[a-zA-Z0-9].txt", "Z.txt", true)]
    [Arguments("[a-zA-Z0-9].txt", "5.txt", true)]
    [Arguments("[a-zA-Z0-9].txt", "_.txt", false)]
    public void IsMatch_CharacterClass_MultipleRanges(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("*.txt", "file.txt", true)]
    [Arguments("*.txt", "folder/file.txt", false)] // * should NOT cross directory boundaries
    [Arguments("*.txt", "a/b/file.txt", false)] // * should NOT cross directory boundaries
    [Arguments("*file.txt", "somefile.txt", true)]
    [Arguments("*file.txt", "folder/somefile.txt", false)] // * should NOT cross directory boundaries
    [Arguments("test*", "test.txt", true)]
    [Arguments("test*", "test/file.txt", false)] // * should NOT cross directory boundaries
    [Arguments("a*c", "abc", true)]
    [Arguments("a*c", "a/c", false)] // * should NOT cross directory separators
    [Arguments("a*c", "a\\c", false)] // * should NOT cross directory separators (Windows style)
    [Arguments("**/*.txt", "folder/file.txt", true)] // ** SHOULD cross directory boundaries
    [Arguments("**/*.txt", "a/b/c/file.txt", true)] // ** SHOULD cross directory boundaries
    public void IsMatch_SingleWildcard_DoesNotCrossDirectories(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("test?.txt", "test1.txt", true)]
    [Arguments("test?.txt", "testa.txt", true)]
    [Arguments("test?.txt", "test/.txt", false)] // ? should NOT match path separator
    [Arguments("a?c", "abc", true)]
    [Arguments("a?c", "a/c", false)] // ? should NOT match path separator (Unix-style)
    [Arguments("a?c", "a\\c", false)] // ? should NOT match path separator (Windows-style)
    [Arguments("file?.txt", "file1.txt", true)]
    [Arguments("file?.txt", "file/.txt", false)] // ? should NOT match path separator
    [Arguments("???", "abc", true)]
    [Arguments("???", "a/c", false)] // ? should NOT match path separator
    [Arguments("test?file", "test1file", true)]
    [Arguments("test?file", "test/file", false)] // ? should NOT match path separator
    public void IsMatch_QuestionMark_DoesNotMatchPathSeparator(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("*.{txt,csv}", "file.txt", true)]
    [Arguments("*.{txt,csv}", "file.csv", true)]
    [Arguments("*.{txt,csv}", "file.pdf", false)]
    [Arguments("*.{txt,csv}", "x", false)] // Prefix matches but none of the alternatives do
    [Arguments("*test.{a,b}", "mytest.a", true)]
    [Arguments("*test.{a,b}", "mytest.b", true)]
    [Arguments("*test.{a,b}", "mytest.c", false)]
    [Arguments("*test.{a,b}", "other.a", false)] // Doesn't match "test" part
    [Arguments("?.{x,y}", "a.x", true)]
    [Arguments("?.{x,y}", "b.y", true)]
    [Arguments("?.{x,y}", "c.z", false)]
    [Arguments("?.{x,y}", "ab.x", false)] // ? only matches single character
    public void IsMatch_BraceExpansion_WithWildcardsInPrefix(string pattern, string path, bool expected)
    {
        var matcher = new GlobMatcher(pattern);
        var result = matcher.IsMatch(path);

        result.Should().Be(expected);
    }
}
