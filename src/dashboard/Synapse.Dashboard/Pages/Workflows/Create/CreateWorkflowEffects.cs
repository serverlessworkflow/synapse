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
using Synapse.Dashboard.Features.Shared.Routing.Actions;
using Synapse.Dashboard.Pages.Workflows.Create.Actions;
using Synapse.Integration.Commands.Workflows;

namespace Synapse.Dashboard.Pages.Workflows.Create.Effects
{
    [Effect]
    public static class CreateWorkflowEffects
    {
        public static async Task OnCreateWorkflow(CreateWorkflow action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            if (api == null)
                throw new NullReferenceException("Unable to resolved service 'ISynapseManagementApi'.");
            var command = new V1CreateWorkflowCommand()
            {
                Definition = action.Definition
            };
            var workflow = await api.CreateWorkflowAsync(command);
            if (workflow != null)
                context.Dispatcher.Dispatch(new NavigateTo("workflows"));
        }
    }
}
