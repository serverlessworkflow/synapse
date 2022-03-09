namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a breadcrumb element
    /// </summary>
    public interface IBreadcrumbItem
    {
        /// <summary>
        /// The displayed label
        /// </summary>
        string Label { get; init; }

        /// <summary>
        /// The displayed icon, if any
        /// </summary>
        string? Icon { get; init; }

        /// <summary>
        /// The navigation link
        /// </summary>
        string Link { get; init; }
    }
}
