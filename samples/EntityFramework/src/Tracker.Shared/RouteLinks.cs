namespace Tracker;

public static partial class RouteLinks
{
    public static string Home() => "/";

    public static partial class Priorities
    {
        public static string List() => "/priorities";
        public static string Edit(int id) => $"/priorities/{id}";
    }

    public static partial class Statuses
    {
        public static string List() => "/statuses";
        public static string Edit(int id) => $"/statuses/{id}";
    }

    public static partial class Tenants
    {
        public static string List() => "/tenants";
        public static string Edit(int id) => $"/tenants/{id}";
    }

    public static partial class Users
    {
        public static string List() => "/users";
        public static string Edit(int id) => $"/users/{id}";
    }

    public static partial class Tasks
    {
        public static string List() => "/tasks";
        public static string Edit(int id) => $"/tasks/{id}";
    }
}
