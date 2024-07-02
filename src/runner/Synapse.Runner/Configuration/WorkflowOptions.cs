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

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the workflow the Synapse Runner must run and how
/// </summary>
public class WorkflowOptions
{

    /// <summary>
    /// Initializes a new <see cref="WorkflowOptions"/>
    /// </summary>
    public WorkflowOptions()
    {
        this.Instance = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Workflow.Instance)!;
    }

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance to run
    /// </summary>
    public virtual string Instance { get; set; }

    /// <summary>
    /// Gets the namespace of the workflow instance to run
    /// </summary>
    /// <returns>The namespace of the workflow instance to run</returns>
    public virtual string GetInstanceNamespace() => this.Instance.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();

    /// <summary>
    /// Gets the name of the workflow instance to run
    /// </summary>
    /// <returns>The name of the workflow instance to run</returns>
    public virtual string GetInstanceName() => this.Instance.Split('.', StringSplitOptions.RemoveEmptyEntries).First();

}