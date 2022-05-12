using Microsoft.AspNetCore.Components;
using Neuroglia.Data.Flux;
using Synapse.Dashboard.Features.Shared.Routing.Actions;

namespace Synapse.Dashboard.Features.Shared.Routing.Effects
{
    [Effect]
    public static class RoutingEffects
    {
        public static async Task OnNavigateTo(NavigateTo action, IEffectContext context)
        {
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();
            if (navigationManager == null)
                throw new NullReferenceException("Unable to resolved service 'NavigationManager'.");
            navigationManager.NavigateTo(action.Uri, action.ForceLoad, action.Replace);
            await Task.CompletedTask;
        }
    }
}
