﻿namespace Synapse.Dashboard
{
    public static class KnownBreadcrumbs
    {
        public static IEnumerable<IBreadcrumbItem> Home = new List<IBreadcrumbItem>() { new BreadcrumbItem("Home", "/", "oi-clipboard") };
        public static IEnumerable<IBreadcrumbItem> Workflows = new List<IBreadcrumbItem>() { new BreadcrumbItem("Workflows", "/workflows", "oi-cog") };
        public static IEnumerable<IBreadcrumbItem> CreateWorkflow = new List<IBreadcrumbItem>() { new BreadcrumbItem("Create Workflow", "/workflows/new") };
        public static IEnumerable<IBreadcrumbItem> Correlations = new List<IBreadcrumbItem>() { new BreadcrumbItem("Correlations", "/correlations", "oi-link-intact") };
    }
}