/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Synapse.Apis.Management;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections
{

    /// <summary>
    /// Defines the Flux effects applying to <see cref="ResourceListState"/>-related actions
    /// </summary>
    [Effect]
    public static class ResourceListEffects
    {

        /// <summary>
        /// Queries <see cref="V1FunctionDefinitionCollection"/>s
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

        /// <summary>
        /// Queries <see cref="V1EventDefinitionCollection"/>s
        /// </summary>
        /// <param name="action">The Flux action the effect applies to</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(QueryV1EventDefinitionCollections action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var collections = await api.GetEventDefinitionCollectionsAsync(action.Query);
            context.Dispatcher.Dispatch(new SetV1EventDefinitionCollections(collections));
        }

        /// <summary>
        /// Queries <see cref="V1AuthenticationDefinitionCollection"/>s
        /// </summary>
        /// <param name="action">The Flux action the effect applies to</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(QueryV1AuthenticationDefinitionCollections action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var collections = await api.GetAuthenticationDefinitionCollectionsAsync(action.Query);
            context.Dispatcher.Dispatch(new SetV1AuthenticationDefinitionCollections(collections));
        }

    }

}
