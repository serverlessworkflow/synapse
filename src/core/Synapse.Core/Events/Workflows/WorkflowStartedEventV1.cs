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

using Synapse.Resources;

namespace Synapse.Events.Workflows;

/// <summary>
/// Represents the data carried by the cloud event that notifies that a workflow instance has started
/// </summary>
[CloudEvent(SynapseDefaults.CloudEvents.Workflow.Started.v1)]
public record WorkflowStartedEventV1
{

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance that has started
    /// </summary>
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets/sets the an object that describes the definition of the workflow instance that has started
    /// </summary>
    [DataMember(Name = "definition", Order = 2), JsonPropertyName("definition"), JsonPropertyOrder(2), YamlMember(Alias = "definition", Order = 2)]
    public required WorkflowDefinitionReference Definition { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the workflow instance has started
    /// </summary>
    [DataMember(Name = "startedAt", Order = 3), JsonPropertyName("startedAt"), JsonPropertyOrder(3), YamlMember(Alias = "startedAt", Order = 3)]
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;

}
