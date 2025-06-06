namespace Arbiter.CommandQuery.Services.Tests;

public class UrlBuilderTests
{

    [Test]
    [Arguments("http://foo/bar/baz", "date", "today", "http://foo/bar/baz?date=today")]
    [Arguments("http://foo/bar/baz", "date", "sunday afternoon", "http://foo/bar/baz?date=sunday%20afternoon")]
    [Arguments("http://foo/bar/baz?date=today", "key1", "value1", "http://foo/bar/baz?date=today&key1=value1")]
    [Arguments("http://foo/bar/baz?date=today", "key1", "value 1&", "http://foo/bar/baz?date=today&key1=value%201%26")]
    [Arguments("foo/bar/baz?date=today", "key1", "value 1&", "foo/bar/baz?date=today&key1=value%201%26")]
    public void AppendQuery(string url, string key, string value, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.AppendQuery(key, value);
        builder.Query.AllKeys.Should().Contain(key);
        builder.Query[key].Should().Contain(value);

        builder.ToString().Should().Be(expected);

    }

    [Test]
    [Arguments("http://foo/bar/baz", "date=today", "http://foo/bar/baz?date=today")]
    [Arguments("http://foo/bar/baz?date=today", "date=yesterday", "http://foo/bar/baz?date=yesterday")]
    [Arguments("http://foo/bar/baz?date=today", "?date=tomorrow", "http://foo/bar/baz?date=tomorrow")]
    [Arguments("http://foo/bar/baz?date=today", "&date=tomorrow", "http://foo/bar/baz?=&date=tomorrow")]
    [Arguments("http://foo/bar/baz?date=today", "date=", "http://foo/bar/baz?date=")]
    [Arguments("foo/bar/baz?date=today", "date=", "foo/bar/baz?date=")]
    public void SetQuery(string url, string query, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.SetQuery(query);
        builder.ToString().Should().Be(expected);

    }

    [Test]
    [Arguments("http://foo/bar/baz?date=today", null, "http://foo/bar/baz")]
    [Arguments("http://foo/bar/baz?date=today", "", "http://foo/bar/baz")]
    [Arguments("http://foo/bar/baz?date=today", "?", "http://foo/bar/baz")]
    [Arguments("http://foo/bar/baz?date=today", "&", "http://foo/bar/baz?=&=")]
    [Arguments("foo/bar/baz?date=today", "&", "foo/bar/baz?=&=")]
    public void SetQueryEmpty(string url, string? query, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.SetQuery(query);
        builder.ToString().Should().Be(expected);

    }

    [Test]
    [Arguments("http://foo.com/", "/bar/baz", "http://foo.com/bar/baz")]
    [Arguments("http://foo.com/bar", "baz", "http://foo.com/bar/baz")]
    [Arguments("http://foo.com/bar", "/baz", "http://foo.com/bar/baz")]
    [Arguments("http://foo.com/bar/", "/baz", "http://foo.com/bar/baz")]
    [Arguments("foo.com/bar/", "/baz", "foo.com/bar/baz")]
    public void AppendPath(string url, string path, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.AppendPath(path);
        builder.Path.Should().NotBeEmpty();
        builder.ToString().Should().Be(expected);
    }

    [Test]
    public void AppendParamsPath()
    {
        var builder = new UrlBuilder("http://foo.com/");
        builder.Should().NotBeNull();

        builder.AppendPaths("bar", "baz");
        builder.Path.Should().NotBeEmpty();
        builder.ToString().Should().Be("http://foo.com/bar/baz");
    }

    [Test]
    [Arguments("http://foo.com/", 123, "http://foo.com/123")]
    [Arguments("http://foo.com/bar", 5, "http://foo.com/bar/5")]
    public void AppendPathTypeInt(string url, int path, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.AppendPath(path);
        builder.Path.Should().NotBeEmpty();
        builder.ToString().Should().Be(expected);
    }

    [Test]
    [Arguments("http://foo.com/", "/bar/baz", "http://foo.com/bar/baz")]
    [Arguments("http://foo.com/bar", "baz", "http://foo.com/baz")]
    [Arguments("http://foo.com/bar", "/baz", "http://foo.com/baz")]
    [Arguments("foo.com/bar", "/baz", "foo.com/baz")]
    public void SetPath(string url, string path, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.SetPath(path);
        builder.Path.Should().NotBeEmpty();
        builder.ToString().Should().Be(expected);
    }


    [Test]
    [Arguments("http://foo.com/bar/baz", null, "http://foo.com/")]
    [Arguments("http://foo.com/bar/baz", "", "http://foo.com/")]
    [Arguments("http://foo.com/bar/baz", "/", "http://foo.com/")]
    public void SetPathEmpty(string url, string? path, string expected)
    {
        var builder = new UrlBuilder(url);
        builder.Should().NotBeNull();

        builder.SetPath(path);
        builder.Path.Should().BeEmpty();
        builder.ToString().Should().Be(expected);

    }


    [Test]
    public void Build()
    {
        var builder = new UrlBuilder();
        builder
            .SetScheme("https")
            .SetHost("foo.com")
            .SetPort(443)
            .AppendPath("/bar");

        builder.Scheme.Should().Be("https");
        builder.Host.Should().Be("foo.com");
        builder.Port.Should().Be(443);

        builder.Path.Should().NotBeEmpty();
        builder.Path.Should().Contain("bar");

        builder.ToString().Should().Be("https://foo.com/bar");

    }

    [Test]
    public void BuildRelative()
    {
        var builder = UrlBuilder
            .Create()
            .AppendPath("/bar")
            .AppendQuery("test", true);

        builder.Scheme.Should().BeNull();
        builder.Host.Should().BeNull();
        builder.Port.Should().BeNull();

        builder.Path.Should().NotBeEmpty();
        builder.Path.Should().Contain("bar");

        builder.Query.AllKeys.Length.Should().BeGreaterThan(0);
        builder.Query.AllKeys.Should().Contain("test");

        builder.ToString().Should().Be("/bar?test=True");

    }

    [Test]
    public void BuildBasePath()
    {
        var builder = UrlBuilder
            .Create("http://foo.com/test")
            .AppendPath("/bar")
            .AppendQuery("test", true);

        builder.Scheme.Should().Be("http");
        builder.Host.Should().Be("foo.com");
        builder.Port.Should().BeNull();

        builder.Path.Should().NotBeEmpty();
        builder.Path.Should().Contain("test");
        builder.Path.Should().Contain("bar");

        builder.Query.AllKeys.Length.Should().BeGreaterThan(0);
        builder.Query.AllKeys.Should().Contain("test");

        builder.ToString().Should().Be("http://foo.com/test/bar?test=True");

    }

    [Test]
    public void BuildBaseRelativePath()
    {
        var builder = UrlBuilder
            .Create("/foo")
            .AppendPath("/bar")
            .AppendQuery("test", true);

        builder.Scheme.Should().BeEmpty();
        builder.Host.Should().BeEmpty();
        builder.Port.Should().BeNull();

        builder.Path.Should().NotBeEmpty();
        builder.Path.Should().Contain("foo");
        builder.Path.Should().Contain("bar");

        builder.Query.AllKeys.Length.Should().BeGreaterThan(0);
        builder.Query.AllKeys.Should().Contain("test");

        builder.ToString().Should().Be("/foo/bar?test=True");
    }
}
