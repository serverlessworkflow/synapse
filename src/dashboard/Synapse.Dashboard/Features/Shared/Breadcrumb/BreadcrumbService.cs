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
            this.Items = new List<IBreadcrumbItem>(KnownBreadcrumbs.Home);
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
    
        /// <summary>
        /// Clears the <see cref="IBreadcrumbItem"/>'s list
        /// </summary>
        /// <returns></returns>
        public async Task Clear()
        {
            this.Items?.Clear();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Replaces the current <see cref="IBreadcrumbItem"/>'s list with the provided one
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task Use(IEnumerable<IBreadcrumbItem> list)
        {
            await this.Clear();
            foreach (var item in list)
            {
                await this.AddItem(item);
            }
        }
    }
}
