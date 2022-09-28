using Neuroglia.Data.Flux;
using Synapse.Apis.Management;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Defines the Flux effects applying to <see cref="V1FunctionDefinitionCollectionState"/>-related actions
    /// </summary>
    [Effect]
    public static class V1FunctionDefinitionCollectionEffects
    {

        /// <summary>
        /// Queries <see cref="FunctionDefinitionCollection"/>s
        /// </summary>
        /// <param name="action">The Flux action the effect applies to</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(QueryV1FunctionDefinitionCollections action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var collections = await api.GetFunctionDefinitionCollectionsAsync(action.Query);
            context.Dispatcher.Dispatch(new SetV1FunctionDefinitionCollections(collections));
        }

    }

}
