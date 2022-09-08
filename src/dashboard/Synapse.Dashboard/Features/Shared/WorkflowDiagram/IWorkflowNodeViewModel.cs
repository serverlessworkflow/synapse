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

using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Defines the fundamentals of a workflow node
    /// </summary>
    public interface IWorkflowNodeViewModel
    {

        /// <summary>
        /// Gets an <see cref="ObservableCollection{T}"/> containing the active <see cref="V1WorkflowInstance"/>s for which the activity described by the node is active
        /// </summary>
        ObservableCollection<V1WorkflowInstance> ActiveInstances { get; }

        /// <summary>
        /// Gets an <see cref="ObservableCollection{T}"/> containing the faulted <see cref="V1WorkflowInstance"/>s for which the activity described by the node is active
        /// </summary>
        ObservableCollection<V1WorkflowInstance> FaultedInstances { get; }


        /// <summary>
        /// Gets an <see cref="ObservableCollection{T}"/> containing the compensated <see cref="V1WorkflowInstance"/>s for which the activity described by the node is active
        /// </summary>
        ObservableCollection<V1WorkflowInstance> CompensatedInstances { get; }
    }

}
