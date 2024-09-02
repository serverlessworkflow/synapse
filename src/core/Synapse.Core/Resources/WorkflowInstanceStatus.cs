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

namespace Synapse.Resources;

/// <summary>
/// Represents the status of a workflow instance resource
/// </summary>
[DataContract]
public record WorkflowInstanceStatus
{

    /// <summary>
    /// Gets/sets the current phase of the workflow
    /// </summary>
    [DataMember(Order = 1, Name = "phase"), JsonPropertyName("phase"), JsonPropertyOrder(1), YamlMember(Alias = "phase", Order = 1)]
    public virtual string? Phase { get; set; }

    /// <summary>
    /// Gets/sets the date and time the task has been started at, if applicable
    /// </summary>
    [DataMember(Name = "startedAt", Order = 2), JsonPropertyName("startedAt"), JsonPropertyOrder(2), YamlMember(Alias = "startedAt", Order = 2)]
    public virtual DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Gets/sets the date and time the task has ended, if applicable
    /// </summary>
    [DataMember(Name = "endedAt", Order = 3), JsonPropertyName("endedAt"), JsonPropertyOrder(3), YamlMember(Alias = "endedAt", Order = 3)]
    public virtual DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Gets/sets a list containing the tasks that are being performed -or already have been performed- by the workflow
    /// </summary>
    [DataMember(Order = 4, Name = "tasks"), JsonPropertyName("tasks"), JsonPropertyOrder(4), YamlMember(Alias = "tasks", Order = 4)]
    public virtual EquatableList<TaskInstance>? Tasks { get; set; }

    /// <summary>
    /// Gets/sets a list that contains the workflow's runs, if any
    /// </summary>
    [DataMember(Order = 5, Name = "runs"), JsonPropertyName("runs"), JsonPropertyOrder(5), YamlMember(Alias = "runs", Order = 5)]
    public virtual EquatableList<WorkflowRun>? Runs { get; set; }

    /// <summary>
    /// Gets/sets a name/context map that contains the workflow's pending correlations
    /// </summary>
    [DataMember(Order = 6, Name = "correlation"), JsonPropertyName("correlation"), JsonPropertyOrder(6), YamlMember(Alias = "correlation", Order = 6)]
    public virtual WorkflowInstanceCorrelationStatus? Correlation { get; set; }

    /// <summary>
    /// Gets/sets the error, if any, that has occurred during the workflow's execution
    /// </summary>
    [DataMember(Name = "error", Order = 7), JsonPropertyName("error"), JsonPropertyOrder(7), YamlMember(Alias = "error", Order = 7)]
    public virtual Error? Error { get; set; }

    /// <summary>
    /// Gets/sets a reference to the workflow's context data, if any
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 8, Name = "contextReference"), JsonPropertyName("contextReference"), JsonPropertyOrder(8), YamlMember(Alias = "contextReference", Order = 8)]
    public virtual string ContextReference { get; set; } = null!;

    /// <summary>
    /// Gets/sets a reference to the workflow's context data, if any
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 9, Name = "outputReference"), JsonPropertyName("outputReference"), JsonPropertyOrder(9), YamlMember(Alias = "outputReference", Order = 9)]
    public virtual string? OutputReference { get; set; }

}
