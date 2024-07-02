// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the base class for all workflow-related node models
/// </summary>
public abstract class WorkflowNodeViewModel
    : NodeViewModel, IWorkflowNodeViewModel
{

    int _operativeInstances = 0;
    int _faultedInstances = 0;

    /// <inheritdoc/>
    public WorkflowNodeViewModel(string? label = "", string? cssClass = null, string? shape = null, double? width = Constants.NodeWidth * 1.5, double? height = Constants.NodeHeight * 1.5,
        double? radiusX = Constants.NodeRadius, double? radiusY = Constants.NodeRadius, double? x = 0, double? y = 0, Type? componentType = null, Guid? parentId = null)
        : base(label, cssClass, shape, width, height, radiusX, radiusY, x, y, componentType, parentId)
    {
        ComponentType = typeof(WorkflowNodeTemplate);
    }

    /// <inheritdoc/>
    public int OperativeInstancesCount
    {
        get => _operativeInstances;
        set
        {
            _operativeInstances = value;
            OnChange();
        }
    }

    /// <inheritdoc/>
    public int FaultedInstancesCount
    {
        get => _faultedInstances;
        set
        {
            _faultedInstances = value;
            OnChange();
        }
    }

    /// <inheritdoc/>
    public void ResetInstancesCount()
    {
        _operativeInstances = 0;
        _faultedInstances = 0;
        OnChange();
    }

}
