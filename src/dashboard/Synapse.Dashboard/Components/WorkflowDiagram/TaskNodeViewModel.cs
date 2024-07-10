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
public class TaskNodeViewModel
    : ClusterViewModel, ICounterNodeViewModel
{

    int _operativeInstances;
    int _faultedInstances;

    /// <summary>
    /// Initializes a new <see cref="TaskNodeViewModel"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="TaskDefinition"/> the <see cref="TaskNodeViewModel"/> represents</param>
    /// <param name="definition">The <see cref="TaskDefinition"/> the <see cref="TaskNodeViewModel"/> represents</param>
    /// <param name="isFirst">Indicates whether or not the task to create the <see cref="TaskNodeViewModel"/> for is the first task of the workflow it belongs to</param>
    public TaskNodeViewModel(string name, TaskDefinition definition, bool isFirst = false)
        : base(null, name)
    {
        this.Name = name;
        this.Definition = definition;
        this.IsFirst = isFirst;
        this.ComponentType = typeof(TaskNodeTemplate);
    }

    /// <summary>
    /// Gets the name of the <see cref="TaskDefinition"/> the <see cref="TaskNodeViewModel"/> represents
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="TaskDefinition"/> the <see cref="TaskNodeViewModel"/> represents
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public TaskDefinition Definition { get; }

    /// <summary>
    /// Gets if the state is the first of the workflow
    /// </summary>
    public bool IsFirst { get; }

    /// <inheritdoc/>
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
    public void ResetInstancesCount()
    {
        this._operativeInstances = 0;
        this._faultedInstances = 0;
        this.OnChange();
    }

}
