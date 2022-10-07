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
using Synapse.Dashboard.Pages.Workflows.View.Actions;
using Synapse.Dashboard.Pages.Workflows.View.State;
using Synapse.Integration.Models;
using static Synapse.EnvironmentVariables;


namespace Synapse.Dashboard.Pages.Workflows.View.Effects
{

    [Effect]
    public static class WorkflowViewEffects
    {
        /// <summary>
        /// Handles the state initialization
        /// </summary>
        /// <param name="action">The <see cref="InitializeState"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        public static async Task On(InitializeState action, IEffectContext context)
        {
            WorkflowViewState initialState = new()
            {
                Instances = new(),
                Activities = new()
            };
            context.Dispatcher.Dispatch(new InitializeStateSuccessful(initialState));
        }

        /// <summary>
        /// Gets the specified workflow from the API
        /// </summary>
        /// <param name="action">The <see cref="GetWorkflowById"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task On(GetWorkflowById action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var workflow = await api.GetWorkflowByIdAsync(action.WorkflowId);
            context.Dispatcher.Dispatch(new SetWorkflow(workflow));
            var workflowInstances = await api.GetWorkflowInstancesAsync($"$filter={nameof(V1WorkflowInstance.WorkflowId)} eq '{action.WorkflowId}'");
            context.Dispatcher.Dispatch(new SetWorkflowInstances(workflowInstances));
        }
    }
}
