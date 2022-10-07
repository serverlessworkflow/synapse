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
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Workflows.View.State
{

    /// <summary>
    /// The <see cref="State{TState}"/> of the workflow details view
    /// </summary>
    [Feature]
    public record WorkflowViewState
    {
        /// <summary>
        /// The displayed <see cref="V1Workflow"/>
        /// </summary>
        public V1Workflow? Workflow { get; set; }

        /// <summary>
        /// The workflow instances
        /// </summary>
        public Dictionary<string, V1WorkflowInstance>? Instances { get; set; }

        /// <summary>
        /// The instances activities
        /// </summary>
        public Dictionary<string, V1WorkflowActivity>? Activities { get; set; }

        /// <summary>
        ///  The displayed <see cref="V1WorkflowInstance"/>, if any focused
        /// </summary>
        public V1WorkflowInstance? ActiveInstance { get; set; }
    }
}
