using Arbiter.CommandQuery.Services;
using System.Globalization;

namespace Arbiter.Services.Tests;

public class UrlBuilderTests
{
    [Test]
    public void BuildSimpleUrlWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com");

        var url = builder.ToString();

        url.Should().Be("https://example.com");
    }

    [Test]
    public void BuildUrlWithPortWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("http")
            .Host("localhost")
            .Port(8080);

        var url = builder.ToString();

        url.Should().Be("http://localhost:8080");
    }

    [Test]
    public void BuildUrlWithIpv6HostAddsBrackets()
    {
        var url = new UrlBuilder()
            .Scheme("https")
            .Host("::1")
            .Port(8080)
            .ToString();

        url.Should().Be("https://[::1]:8080");
    }

    [Test]
    public void BuildUrlWithBracketedIpv6HostDoesNotAddBrackets()
    {
        var url = new UrlBuilder()
            .Scheme("https")
            .Host("[::1]")
            .ToString();

        url.Should().Be("https://[::1]");
    }

    [Test]
    public void BuildUrlWithUserInfoWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("ftp")
            .Host("ftp.example.com")
            .UserName("user")
            .Password("pass");

        var url = builder.ToString();

        url.Should().Be("ftp://user:pass@ftp.example.com");
    }

    [Test]
    public void BuildUrlWithPathWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api")
            .AppendSegment("v1")
            .AppendSegment("users");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/v1/users");
    }

    [Test]
    public void BuildUrlWithPathEncodesSpecialCharacters()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api v1")
            .AppendSegment("üser");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api%20v1/%C3%BCser");
    }

    [Test]
    public void BuildUrlWithQueryWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("a", "1")
            .AppendQuery("b", "2");

        var url = builder.ToString();

        url.Should().Be("https://example.com?a=1&b=2");
    }

    [Test]
    public void BuildUrlWithQueryEncodesSpecialCharacters()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("a b", "ü&c");

        var url = builder.ToString();

        url.Should().Be("https://example.com?a%20b=%C3%BC%26c");
    }

    [Test]
    public void BuildUrlWithFragmentWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .Fragment("section1");

        var url = builder.ToString();

        url.Should().Be("https://example.com#section1");
    }

    [Test]
    public void BuildOnlyPathNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .AppendSegment("api")
            .AppendSegment("v1")
            .AppendSegment("users");

        var url = builder.ToString();

        url.Should().Be("/api/v1/users");
    }

    [Test]
    public void BuildOnlyQueryNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .AppendQuery("foo", "bar")
            .AppendQuery("baz", "qux");

        var url = builder.ToString();

        url.Should().Be("?foo=bar&baz=qux");
    }

    [Test]
    public void BuildOnlyFragmentNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .Fragment("fragSection");

        var url = builder.ToString();

        url.Should().Be("#fragSection");
    }

    [Test]
    public void BuildPathAndQueryNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .AppendSegment("api")
            .AppendSegment("v1")
            .AppendQuery("id", "42");

        var url = builder.ToString();

        url.Should().Be("/api/v1?id=42");
    }

    [Test]
    public void BuildPathQueryFragmentNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .AppendSegment("api")
            .AppendSegment("v1")
            .AppendQuery("id", "42")
            .Fragment("top");

        var url = builder.ToString();

        url.Should().Be("/api/v1?id=42#top");
    }

    [Test]
    public void BuildEmptyBuilderReturnsEmptyString()
    {
        var builder = new UrlBuilder();

        var url = builder.ToString();

        url.Should().BeEmpty();
    }

    [Test]
    public void BuildUrlWithPortAsStringWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("http")
            .Host("localhost")
            .Port("1234");

        var url = builder.ToString();

        url.Should().Be("http://localhost:1234");
    }

    [Test]
    public void FromPathStartsBuilder()
    {
        var url = UrlBuilder
            .FromPath("api/user")
            .AppendQuery("id", 42)
            .ToString();

        url.Should().Be("/api/user?id=42");
    }

    [Test]
    public void FromSegmentStartsBuilder()
    {
        var url = UrlBuilder
            .FromSegment("john doe")
            .ToString();

        url.Should().Be("/john%20doe");
    }

    [Test]
    public void FromSegmentsStartsBuilder()
    {
        var url = UrlBuilder
            .FromSegments("api", "v2", "users")
            .ToString();

        url.Should().Be("/api/v2/users");
    }

    [Test]
    public void AppendSegmentGenericWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment(123)
            .AppendSegment(Guid.Empty);

        var url = builder.ToString();

        url.Should().Be("https://example.com/123/00000000-0000-0000-0000-000000000000");
    }

    [Test]
    public void AppendSegmentWithConditionFuncWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api", s => s == "api")
            .AppendSegment("skip", s => s == "nope");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api");
    }

    [Test]
    public void AppendSegmentWithConditionBoolWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api", true)
            .AppendSegment("skip", false);

        var url = builder.ToString();

        url.Should().Be("https://example.com/api");
    }

    [Test]
    public void AppendSegmentsWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegments(["api", "v2", "users"]);

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/v2/users");
    }

    [Test]
    public void AppendPathKeepsSeparators()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("api/user");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/user");
    }

    [Test]
    public void AppendPathTrimsLeadingAndTrailingSeparators()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("/api/user/")
            .AppendSegment("john doe");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/user/john%20doe");
    }

    [Test]
    public void AppendPathDoesNotEscape()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("api/john%20doe");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/john%20doe");
    }

    [Test]
    public void AppendPathNullOrEmptyDoesNothing()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath(null)
            .AppendPath(string.Empty)
            .AppendPath("/");

        var url = builder.ToString();

        url.Should().Be("https://example.com");
    }

    [Test]
    public void AppendQueryGenericWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("id", 42);

        var url = builder.ToString();

        url.Should().Be("https://example.com?id=42");
    }

    [Test]
    [NotInParallel]
    public void GenericValuesUseInvariantCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var url = new UrlBuilder()
                .AppendSegment(1.5m)
                .AppendQuery("value", 2.5m)
                .ToString();

            url.Should().Be("/1.5?value=2.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void AppendQueryWithConditionFuncWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("a", "1", v => v == "1")
            .AppendQuery("b", "2", v => v == "nope");

        var url = builder.ToString();

        url.Should().Be("https://example.com?a=1");
    }

    [Test]
    public void AppendQueryWithConditionBoolAppendsNullAsEmptyValue()
    {
        var url = new UrlBuilder()
            .AppendQuery("included", null, true)
            .AppendQuery("excluded", null, false)
            .ToString();

        url.Should().Be("?included=");
    }

    [Test]
    public void AppendQueryWithConditionBoolWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("a", "1", true)
            .AppendQuery("b", "2", false);

        var url = builder.ToString();

        url.Should().Be("https://example.com?a=1");
    }

    [Test]
    public void AppendQueriesWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQueries([
                new KeyValuePair<string, string?>("a", "1"),
                new KeyValuePair<string, string?>("b", "2")
            ]);

        var url = builder.ToString();

        url.Should().Be("https://example.com?a=1&b=2");
    }

    [Test]
    public void AppendSegmentNullOrEmptyDoesNothing()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment((string?)null)
            .AppendSegment("")
            .AppendSegment("users");

        var url = builder.ToString();

        url.Should().Be("https://example.com/users");
    }

    [Test]
    public void AppendQueryNullOrEmptyDoesNothing()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("", "value")
            .AppendQuery("key", (string?)null)
            .AppendQuery("id", "42");

        var url = builder.ToString();

        url.Should().Be("https://example.com?key=&id=42");
    }

    [Test]
    public void BuildUrlWithPathEncodesSurrogatePair()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("\U0001F600");

        var url = builder.ToString();

        url.Should().Be("https://example.com/%F0%9F%98%80");
    }

    [Test]
    public void BuildUrlWithQueryEncodesSurrogatePair()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendQuery("emoji", "\U0001F600");

        var url = builder.ToString();

        url.Should().Be("https://example.com?emoji=%F0%9F%98%80");
    }

    [Test]
    public void BuildIsNotDestructiveAndBuilderCanBeExtended()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api");

        var first = builder.ToString();
        var second = builder.AppendSegment("users").ToString();

        first.Should().Be("https://example.com/api");
        second.Should().Be("https://example.com/api/users");
    }

    [Test]
    public void ToStringMatchesToString()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendSegment("api")
            .AppendQuery("id", "42");

        builder.ToString().Should().Be(builder.ToString());
    }
}
