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

namespace Synapse.Events.Workflows;

/// <summary>
/// Represents the data carried by the cloud event that notifies that a workflow instance has faulted
/// </summary>
[CloudEvent(SynapseDefaults.CloudEvents.Workflow.Faulted.v1)]
public record WorkflowFaultedEventV1
{

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance that has faulted
    /// </summary>
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets/sets the error that has cause the workflow to fault
    /// </summary>
    [DataMember(Name = "error", Order = 2), JsonPropertyName("error"), JsonPropertyOrder(2), YamlMember(Alias = "error", Order = 2)]
    public required Error Error { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the workflow instance has faulted
    /// </summary>
    [DataMember(Name = "faultedAt", Order = 3), JsonPropertyName("faultedAt"), JsonPropertyOrder(3), YamlMember(Alias = "faultedAt", Order = 3)]
    public DateTimeOffset FaultedAt { get; set; } = DateTimeOffset.Now;

}
