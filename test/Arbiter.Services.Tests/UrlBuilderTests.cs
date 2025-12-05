using Arbiter.CommandQuery.Services;

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
            .AppendPath("api")
            .AppendPath("v1")
            .AppendPath("users");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/v1/users");
    }

    [Test]
    public void BuildUrlWithPathEncodesSpecialCharacters()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("api v1")
            .AppendPath("üser");

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
            .AppendPath("api")
            .AppendPath("v1")
            .AppendPath("users");

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
            .AppendPath("api")
            .AppendPath("v1")
            .AppendQuery("id", "42");

        var url = builder.ToString();

        url.Should().Be("/api/v1?id=42");
    }

    [Test]
    public void BuildPathQueryFragmentNoSchemeOrHost()
    {
        var builder = new UrlBuilder()
            .AppendPath("api")
            .AppendPath("v1")
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
    public void AppendPathGenericWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath(123)
            .AppendPath(Guid.Empty);

        var url = builder.ToString();

        url.Should().Be("https://example.com/123/00000000-0000-0000-0000-000000000000");
    }

    [Test]
    public void AppendPathWithConditionFuncWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("api", s => s == "api")
            .AppendPath("skip", s => s == "nope");

        var url = builder.ToString();

        url.Should().Be("https://example.com/api");
    }

    [Test]
    public void AppendPathWithConditionBoolWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath("api", true)
            .AppendPath("skip", false);

        var url = builder.ToString();

        url.Should().Be("https://example.com/api");
    }

    [Test]
    public void AppendPathsWorks()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPaths(["api", "v2", "users"]);

        var url = builder.ToString();

        url.Should().Be("https://example.com/api/v2/users");
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
    public void AppendPathNullOrEmptyDoesNothing()
    {
        var builder = new UrlBuilder()
            .Scheme("https")
            .Host("example.com")
            .AppendPath((string?)null)
            .AppendPath("")
            .AppendPath("users");

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
}
