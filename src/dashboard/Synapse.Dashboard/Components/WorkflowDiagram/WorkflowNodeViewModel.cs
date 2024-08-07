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

using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the base class for all workflow-related node models
/// </summary>
public class WorkflowNodeViewModel
        : NodeViewModel, IWorkflowNodeViewModel
{

    int _operativeInstances = 0;
    int _faultedInstances = 0;

    /// <inheritdoc/>
    public string? Symbol { get; init; }

    /// <inheritdoc/>
    public string? Type { get; init; }

    /// <inheritdoc/>
    public string? Content { get; init; }

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

    /// <summary>
    /// Initialiazes a new <see cref="WorkflowNodeViewModel"/>
    /// </summary>
    /// <param name="taskReference">The node task reference</param>
    /// <param name="config">The <see cref="NodeViewModelConfig"/> for the node</param>
    public WorkflowNodeViewModel(string taskReference, NodeViewModelConfig? config = null)
        : base(config?.Label, config?.CssClass, config?.Shape, config?.Width ?? 0, config?.Height ?? 0, config?.RadiusX ?? 0, config?.RadiusY ?? 0, config?.X ?? 0, config?.Y ?? 0, config?.ComponentType, config?.ParentId)
    {
        this.Id = taskReference;
    }

    /// <inheritdoc/>
    public void ResetInstancesCount()
    {
        _operativeInstances = 0;
        _faultedInstances = 0;
        OnChange();
    }

}
