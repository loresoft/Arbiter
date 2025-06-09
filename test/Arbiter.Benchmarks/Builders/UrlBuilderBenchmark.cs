using System.ComponentModel;

using Arbiter.CommandQuery.Services;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Builders;

[MemoryDiagnoser]
[Description("UrlBuilder")]
public class UrlBuilderBenchmark
{
    private const string Scheme = "https";
    private const string Host = "example.com";
    private const string Path1 = "api";
    private const string Path2 = "v1";
    private const string Path3 = "users";
    private const string QueryKey = "id";
    private const string QueryValue = "42";
    private const string Fragment = "top";

    [Benchmark]
    public string UrlBuilderLegacyToString()
    {
        var builder = new UrlBuilderLegacy()
            .SetScheme(Scheme)
            .SetHost(Host)
            .AppendPath(Path1)
            .AppendPath(Path2)
            .AppendPath(Path3)
            .AppendQuery(QueryKey, QueryValue)
            .SetFragment(Fragment);

        return builder.ToString();
    }

    [Benchmark]
    public string UrlBuilderToString()
    {
        var builder = new UrlBuilder()
            .Scheme(Scheme)
            .Host(Host)
            .AppendPath(Path1)
            .AppendPath(Path2)
            .AppendPath(Path3)
            .AppendQuery(QueryKey, QueryValue)
            .Fragment(Fragment);

        return builder.ToString();
    }

    [Benchmark]
    public string UriBuilderToString()
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = Scheme,
            Host = Host,
            Path = $"{Path1}/{Path2}/{Path3}",
            Fragment = Fragment
        };

        uriBuilder.Query = $"{QueryKey}={Uri.EscapeDataString(QueryValue)}";
        return uriBuilder.Uri.ToString().TrimEnd('/');
    }

    [Benchmark]
    public string InterpolatedString()
    {
        // Use Uri.EscapeDataString for encoding path segments, query, and fragment
        var encodedPath1 = Uri.EscapeDataString(Path1);
        var encodedPath2 = Uri.EscapeDataString(Path2);
        var encodedPath3 = Uri.EscapeDataString(Path3);
        var encodedQueryKey = Uri.EscapeDataString(QueryKey);
        var encodedQueryValue = Uri.EscapeDataString(QueryValue);
        var encodedFragment = Uri.EscapeDataString(Fragment);

        return $"{Scheme}://{Host}/{encodedPath1}/{encodedPath2}/{encodedPath3}?{encodedQueryKey}={encodedQueryValue}#{encodedFragment}";
    }
}
