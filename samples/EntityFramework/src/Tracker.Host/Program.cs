namespace Tracker.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder
            .AddProject<Projects.Tracker_Service>("tracker-service")
            .WithEndpoint(
                endpointName: "https",
                callback: endpoint =>
                {
                    endpoint.UriScheme = "https";
                    endpoint.Transport = "http";
                    endpoint.Port = 7180;
                    endpoint.IsProxied = false;
                }
            );

        builder
            .AddProject<Projects.Tracker_Web>("tracker-web")
            .WithEndpoint(
                endpointName: "https",
                callback: endpoint =>
                {
                    endpoint.UriScheme = "https";
                    endpoint.Transport = "http";
                    endpoint.Port = 7051;
                    endpoint.IsProxied = false;
                }
            );

        builder.Build().Run();
    }
}
