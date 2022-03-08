using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    /// <summary>
    /// The service used to manage the breadcrumb
    /// </summary>
    public class BreadcrumbService
        : IBreadcrumbService
    {
        /// <summary>
        /// The list of displayed <see cref="IBreadcrumbItem"/>
        /// </summary>
        public List<IBreadcrumbItem> Items { get; init; }

        /// <summary>
        /// The <see cref="NavigationManager"/> service
        /// </summary>
        protected NavigationManager NavigationManager { get; init; }

        /// <summary>
        /// Initializes a new <see cref="BreadcrumbService"/>
        /// </summary>
        /// <param name="navigationManager"></param>
        public BreadcrumbService(NavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
            this.Items = new List<IBreadcrumbItem>()
            {
                new BreadcrumbItem("Home", "/", "oi-clipboard")
        };
        }

        /// <summary>
        /// Adds the specified <see cref="IBreadcrumbItem"/> to the list
        /// </summary>
        /// <param name="breadcrumbItem"></param>
        /// <returns></returns>
        public async Task AddItem(IBreadcrumbItem breadcrumbItem)
        {
            if (!this.Items.Contains(breadcrumbItem))
            {
                this.Items.Add(breadcrumbItem);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new <see cref="IBreadcrumbItem"/> with the specified label and icon for the active route and adds it to the list
        /// </summary>
        /// <param name="label"></param>
        /// <param name="icon"></param>
        /// <returns>The created  <see cref="IBreadcrumbItem"/></returns>
        public async Task<IBreadcrumbItem> AddCurrentUri(string label, string? icon = null)
        {
            IBreadcrumbItem item = new BreadcrumbItem(label, this.NavigationManager.Uri, icon);
            this.Items.Add(item);
            return await Task.FromResult(item);
        }

        /// <summary>
        /// Adds the specified <see cref="IBreadcrumbItem"/> to the list
        /// </summary>
        /// <param name="breadcrumbItem"></param>
        /// <returns></returns>
        public async Task RemoveItem(IBreadcrumbItem breadcrumbItem)
        {
            if (this.Items.Contains(breadcrumbItem))
            {
                this.Items.Remove(breadcrumbItem);
            }
            await Task.CompletedTask;
        }
    }
}
