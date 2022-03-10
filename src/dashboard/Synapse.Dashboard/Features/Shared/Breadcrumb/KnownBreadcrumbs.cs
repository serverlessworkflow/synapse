namespace Synapse.Dashboard
{
    public static class KnownBreadcrumbs
    {
        public static IEnumerable<IBreadcrumbItem> Home = new List<IBreadcrumbItem>() { new BreadcrumbItem("Home", "/", "oi-clipboard") };
        public static IEnumerable<IBreadcrumbItem> Workflows = new List<IBreadcrumbItem>(KnownBreadcrumbs.Home) { new BreadcrumbItem("Workflows", "/workflows", "oi-cog") };
        public static IEnumerable<IBreadcrumbItem> CreateWorkflow = new List<IBreadcrumbItem>(KnownBreadcrumbs.Workflows) { new BreadcrumbItem("Create Workflow", "/workflows/new") };
    }
}
