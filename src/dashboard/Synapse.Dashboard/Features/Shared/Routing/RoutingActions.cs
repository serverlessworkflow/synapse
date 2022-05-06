using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard.Features.Shared.Routing.Actions
{
    public class NavigateTo
    {
        public string Uri { get; }
        public bool ForceLoad { get; }
        public bool Replace { get; }

        public NavigateTo(string uri, bool forceLoad = false, bool replace = false)
        {
            this.Uri = uri;
            this.ForceLoad = forceLoad;
            this.Replace = replace;
        }

        public NavigateTo(string uri, NavigationOptions options) 
            : this(uri, options.ForceLoad, options.ReplaceHistoryEntry)
        { 
        }
    }
}
