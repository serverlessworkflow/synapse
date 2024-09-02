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
using ServerlessWorkflow.Sdk.Models;
using System.Text.Json.Serialization;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents a <see cref="TaskDefinition"/> <see cref="NodeViewModel"/>
/// </summary>
public abstract class WorkflowClusterViewModel
    : ClusterViewModel, IWorkflowNodeViewModel
{
    int _operativeInstances = 0;
    int _faultedInstances = 0;

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Symbol { get; init; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Type { get; init; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Content { get; init; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int OperativeInstancesCount
    {
        get => this._operativeInstances;
        set
        {
            this._operativeInstances = value;
            this.OnChange();
        }
    }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int FaultedInstancesCount
    {
        get => this._faultedInstances;
        set
        {
            this._faultedInstances = value;
            this.OnChange();
        }
    }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public bool IsCluster => true;

    /// <summary>
    /// Initializes a new <see cref="WorkflowClusterViewModel"/>
    /// </summary>
    /// <param name="taskReference">The node task reference</param>
    /// <param name="config">The <see cref="NodeViewModelConfig"/> for the node</param>
    public WorkflowClusterViewModel(string taskReference, NodeViewModelConfig? config = null)
        : base(null, config?.Label, config?.CssClass, config?.Shape, config?.Width ?? 0, config?.Height ?? 0, config?.RadiusX ?? 0, config?.RadiusY ?? 0, config?.X ?? 0, config?.Y ?? 0, config?.ComponentType, config?.ParentId)
    {
        this.Id = taskReference;
    }

    /// <inheritdoc/>
    public void ResetInstancesCount()
    {
        this._operativeInstances = 0;
        this._faultedInstances = 0;
        this.OnChange();
    }
}
