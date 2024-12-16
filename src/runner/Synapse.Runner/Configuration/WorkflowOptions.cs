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
        this.InstanceQualifiedName = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Workflow.Instance)!;
    }

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance to run. Required if the execution mode has been set to <see cref="RunnerExecutionMode.Connected"/>, otherwise ignored
    /// </summary>
    public virtual string? InstanceQualifiedName { get; set; }

    /// <summary>
    /// Gets/sets the name of the definition's file, if any. Required if the execution mode has been set to <see cref="RunnerExecutionMode.StandAlone"/>, otherwise ignored
    /// </summary>
    public virtual string? DefinitionFilePath { get; set; }

    /// <summary>
    /// Gets/sets the name of the file, if any, that defines the input of to the workflow to run. Ignored if the execution mode has been set to <see cref="RunnerExecutionMode.Connected"/>
    /// </summary>
    public virtual string? InputFilePath { get; set; }

    /// <summary>
    /// Gets/sets the name of the file, if any, to write the workflow's output to. Ignored if the execution mode has been set to <see cref="RunnerExecutionMode.Connected"/>
    /// </summary>
    public virtual string? OutputFilePath { get; set; }

    /// <summary>
    /// Gets/sets the workflow's output format. Ignored if execution mode has been set to <see cref="RunnerExecutionMode.Connected"/>
    /// </summary>
    public virtual WorkflowOutputFormat OutputFormat { get; set; }

    /// <summary>
    /// Gets the namespace of the workflow instance to run
    /// </summary>
    /// <returns>The namespace of the workflow instance to run</returns>
    public virtual string GetInstanceNamespace()
    {
        if (string.IsNullOrWhiteSpace(this.InstanceQualifiedName)) throw new NullReferenceException("The instance qualified name is null or empty");
        return this.InstanceQualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
    }

    /// <summary>
    /// Gets the name of the workflow instance to run
    /// </summary>
    /// <returns>The name of the workflow instance to run</returns>
    public virtual string GetInstanceName()
    {
        if (string.IsNullOrWhiteSpace(this.InstanceQualifiedName)) throw new NullReferenceException("The instance qualified name is null or empty");
        return this.InstanceQualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries).First();
    }

}
