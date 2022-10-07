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
using Synapse.Dashboard.Services;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents the base class for all workflow-related <see cref="NodeModel"/>s
    /// </summary>
    public abstract class WorkflowNodeViewModel
        : NodeViewModel, IWorkflowNodeViewModel
    {

        private int _activeInstances = 0;
        /// <inheritdoc/>
        public int ActiveInstancesCount {
            get => this._activeInstances;
            set
            {
                this._activeInstances = value;
                this.OnChange();
            }
        }

        private int _faultedInstances = 0;
        /// <inheritdoc/>
        public int FaultedInstancesCount
        {
            get => this._faultedInstances;
            set
            {
                this._faultedInstances = value;
                this.OnChange();
            }
        }
        private int _compensatedInstances = 0;
        /// <inheritdoc/>
        public int CompensatedInstancesCount
        {
            get => this._compensatedInstances;
            set
            {
                this._compensatedInstances = value;
                this.OnChange();
            }
        }

        public WorkflowNodeViewModel(
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Neuroglia.Blazor.Dagre.Constants.NodeWidth * 1.5,
            double? height = Neuroglia.Blazor.Dagre.Constants.NodeHeight * 1.5,
            double? radiusX = Neuroglia.Blazor.Dagre.Constants.NodeRadius,
            double? radiusY = Neuroglia.Blazor.Dagre.Constants.NodeRadius,
            double? x = 0,
            double? y = 0,
            Type? componentType = null,
            Guid? parentId = null
        )
            : base(label, cssClass, shape, width, height, radiusX, radiusY, x, y, componentType, parentId)
        {
            this.ComponentType = typeof(WorkflowNodeTemplate);
        }

        /// <inheritdoc/>
        public void ResetInstancesCount()
        {
            this._activeInstances = 0;
            this._faultedInstances = 0;
            this._compensatedInstances = 0;
            this.OnChange();
        }
    }

}
