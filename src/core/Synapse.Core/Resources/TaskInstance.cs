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
/// Represents the resource used to describe the instance of a task
/// </summary>
[DataContract]
public record TaskInstance
{

    /// <summary>
    /// Gets the task's id
    /// </summary>
    [DataMember(Name = "id", Order = 1), JsonPropertyName("id"), JsonPropertyOrder(1), YamlMember(Alias = "id", Order = 1)]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString("N")[..15];

    /// <summary>
    /// Gets the task's name, if any
    /// </summary>
    [DataMember(Name = "name", Order = 2), JsonPropertyName("name"), JsonPropertyOrder(2), YamlMember(Alias = "name", Order = 2)]
    public virtual string? Name { get; set; }

    /// <summary>
    /// Gets/sets a relative uri that reference to the task's definition
    /// </summary>
    [DataMember(Name = "reference", Order = 3), JsonPropertyName("reference"), JsonPropertyOrder(3), YamlMember(Alias = "reference", Order = 3)]
    public required virtual Uri Reference { get; set; }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not the task is part of an extension
    /// </summary>
    [DataMember(Name = "isExtension", Order = 4), JsonPropertyName("isExtension"), JsonPropertyOrder(4), YamlMember(Alias = "isExtension", Order = 4)]
    public virtual bool IsExtension { get; set; }

    /// <summary>
    /// Gets/sets the id of the task's parent, if any
    /// </summary>
    [DataMember(Name = "parentId", Order = 5), JsonPropertyName("parentId"), JsonPropertyOrder(5), YamlMember(Alias = "parentId", Order = 5)]
    public virtual string? ParentId { get; set; }

    /// <summary>
    /// Gets/sets the date and time the task was created at
    /// </summary>
    [DataMember(Name = "createdAt", Order = 6), JsonPropertyName("createdAt"), JsonPropertyOrder(6), YamlMember(Alias = "createdAt", Order = 6)]
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Gets/sets the date and time the task has been started at, if applicable
    /// </summary>
    [DataMember(Name = "startedAt", Order = 7), JsonPropertyName("startedAt"), JsonPropertyOrder(7), YamlMember(Alias = "startedAt", Order = 7)]
    public virtual DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Gets/sets the date and time the task has ended, if applicable
    /// </summary>
    [DataMember(Name = "endedAt", Order = 8), JsonPropertyName("endedAt"), JsonPropertyOrder(8), YamlMember(Alias = "endedAt", Order = 8)]
    public virtual DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Gets the task's status
    /// </summary>
    /// <remarks>View <see cref="TaskInstanceStatus"/></remarks>
    [DataMember(Name = "status", Order = 9), JsonPropertyName("status"), JsonPropertyOrder(9), YamlMember(Alias = "status", Order = 9)]
    public virtual string? Status { get; set; }

    /// <summary>
    /// Gets the reason, if any, why the task is in its actual status
    /// </summary>
    [DataMember(Name = "statusReason", Order = 10), JsonPropertyName("statusReason"), JsonPropertyOrder(10), YamlMember(Alias = "statusReason", Order = 10)]
    public virtual string? StatusReason { get; set; }

    /// <summary>
    /// Gets/sets a list that contains the task's runs, if any
    /// </summary>
    [DataMember(Name = "runs", Order = 11), JsonPropertyName("runs"), JsonPropertyOrder(11), YamlMember(Alias = "runs", Order = 11)]
    public virtual EquatableList<TaskRun>? Runs { get; set; }

    /// <summary>
    /// Gets/sets a list that contains the task's retry attempts, if any
    /// </summary>
    [DataMember(Name = "retries", Order = 12), JsonPropertyName("retries"), JsonPropertyOrder(12), YamlMember(Alias = "retries", Order = 12)]
    public virtual EquatableList<RetryAttempt>? Retries { get; set; }

    /// <summary>
    /// Gets/sets the error, if any, that has occurred during the task's execution
    /// </summary>
    [DataMember(Name = "error", Order = 13), JsonPropertyName("error"), JsonPropertyOrder(13), YamlMember(Alias = "error", Order = 13)]
    public virtual Error? Error { get; set; }

    /// <summary>
    /// Gets/sets a reference to the task's input data
    /// </summary>
    [Required]
    [DataMember(Name = "inputReference", Order = 14), JsonPropertyName("inputReference"), JsonPropertyOrder(14), YamlMember(Alias = "inputReference", Order = 14)]
    public virtual string? InputReference { get; set; }

    /// <summary>
    /// Gets/sets a reference to the task's context data, if any
    /// </summary>
    [Required]
    [DataMember(Name = "contextReference", Order = 15), JsonPropertyName("contextReference"), JsonPropertyOrder(15), YamlMember(Alias = "contextReference", Order = 15)]
    public virtual string? ContextReference { get; set; }

    /// <summary>
    /// Gets/sets a reference to the task's output data, if any, in case the task ran to completion
    /// </summary>
    [DataMember(Name = "outputReference", Order = 16), JsonPropertyName("outputReference"), JsonPropertyOrder(16), YamlMember(Alias = "outputReference", Order = 16)]
    public virtual string? OutputReference { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="FlowDirective"/> that must be performed next, in case the task ran to completion
    /// </summary>
    [DataMember(Name = "next", Order = 17), JsonPropertyName("next"), JsonPropertyOrder(17), YamlMember(Alias = "next", Order = 17)]
    public virtual string? Next { get; set; }

    /// <summary>
    /// Gets a value indicating whether the task is in an operative state, meaning it is pending, running, or suspended, and can potentially resume execution.
    /// </summary>
    [IgnoreDataMember, JsonIgnore, YamlIgnore]
    public virtual bool IsOperative => this.Status == TaskInstanceStatus.Pending || this.Status == TaskInstanceStatus.Running || this.Status == TaskInstanceStatus.Suspended;

}