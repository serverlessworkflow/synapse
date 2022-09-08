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

using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="StateDefinition"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class StateNodeViewModel
        : ClusterViewModel, IWorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="StateNodeViewModel"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="StateNodeViewModel"/> represents</param>
        public StateNodeViewModel(StateDefinition state, bool isFirst = false)
            : base(null, state.Name!, null, null, Neuroglia.Blazor.Dagre.Constants.ClusterWidth * 1.5, Neuroglia.Blazor.Dagre.Constants.ClusterWidth * 1.5)
        {
            this.State = state;
            this.IsFirst = isFirst;
            this.ComponentType = typeof(StateNodeTemplate);
            if (this.State.UsedForCompensation)
            {
                this.CssClass = (this.CssClass ?? "") + " used-for-compensation";
            }
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> the <see cref="ActionNodeModel"/> represents
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Always)]
        [Newtonsoft.Json.JsonProperty]
        public StateDefinition State { get; }

        /// <summary>
        /// Gets if the state is the first of the worlflow
        /// </summary>
        public bool IsFirst { get; }

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> ActiveInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> FaultedInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> CompensatedInstances { get; } = new();

    }

}
