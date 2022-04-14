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

namespace Synapse.Dashboard
{

    [Feature]
    public class V1WorkflowCollectionState
        : List<V1Workflow>
    {

        public V1WorkflowCollectionState()
        {

        }

        public V1WorkflowCollectionState(IEnumerable<V1Workflow> workflows)
            : base(workflows)
        {

        }

    }

    [Reducer]
    public static class V1WorkflowCollectionReducers
    {

        public static V1WorkflowCollectionState OnGetWorkflowsFromApiSucceeded(V1WorkflowCollectionState state, SetV1WorkflowCollection action)
        {
            return new(action.Workflows);
        }

        public static V1WorkflowCollectionState OnAddV1Workflow(V1WorkflowCollectionState state, AddV1Workflow action)
        {
            state.Add(action.Workflow);
            return state;
        }

        public static V1WorkflowCollectionState OnRemoveV1Workflow(V1WorkflowCollectionState state, RemoveV1Workflow action)
        {
            var workflow = state.FirstOrDefault(w => w.Id == action.WorkflowId);
            if(workflow != null)
                state.Remove(workflow);
            return state;
        }

    }

    [Effect]
    public static class V1WorkflowCollectionEffects
    {

        public static async Task OnListV1Workflows(ListV1Workflows action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var workflows = await api.GetWorkflowsAsync();
            context.Dispatcher.Dispatch(new SetV1WorkflowCollection(workflows));
        }

    }

    public class SetV1WorkflowCollection
    {

        public SetV1WorkflowCollection(IEnumerable<V1Workflow> workflows)
        {
            this.Workflows = workflows;
        }

        public IEnumerable<V1Workflow> Workflows { get; }

    }

    public class AddV1Workflow
    {

        public AddV1Workflow(V1Workflow workflow)
        {
            this.Workflow = workflow;
        }

        public V1Workflow Workflow { get; }

    }

    public class RemoveV1Workflow
    {

        public RemoveV1Workflow(string workflowId)
        {
            this.WorkflowId = workflowId;
        }

        public string WorkflowId { get; }

    }

    public class ListV1Workflows
    {



    }

}
