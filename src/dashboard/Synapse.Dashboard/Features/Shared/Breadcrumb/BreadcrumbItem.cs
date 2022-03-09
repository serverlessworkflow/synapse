namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a breadcrumb element
    /// </summary>
    public class BreadcrumbItem
        : IBreadcrumbItem
    {
        /// <summary>
        /// The displayed label
        /// </summary>
        public string Label { get; init; }

        /// <summary>
        /// The displayed icon, if any
        /// </summary>
        public string? Icon { get; init; }

        /// <summary>
        /// The navigation link
        /// </summary>
        public string Link { get; init; }

        /// <summary>
        /// Initializes a new <see cref="BreadcrumbItem"/> with the provided data
        /// </summary>
        /// <param name="label"></param>
        /// <param name="link"></param>
        /// <param name="icon"></param>
        public BreadcrumbItem(string label, string link, string icon = null)
        {
            this.Label = label;
            this.Link = link;
            this.Icon = icon;
        }
    }
}
