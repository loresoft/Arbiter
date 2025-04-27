internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddProject<Projects.Tracker_Web>("tracker");

        builder.Build().Run();
    }
}
