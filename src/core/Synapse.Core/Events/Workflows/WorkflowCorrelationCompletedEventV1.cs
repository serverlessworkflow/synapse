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
/// Represents the data carried by the cloud event that notifies that a workflow instance has completed correlating events
/// </summary>
[CloudEvent(SynapseDefaults.CloudEvents.Workflow.CorrelationCompleted.v1)]
public record WorkflowCorrelationCompletedEventV1
{

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance that has completed correlating events
    /// </summary>
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets/sets the id of the correlation context
    /// </summary>
    [DataMember(Name = "correlationContext", Order = 2), JsonPropertyName("correlationContext"), JsonPropertyOrder(2), YamlMember(Alias = "correlationContext", Order = 2)]
    public required string CorrelationContext { get; set; }

    /// <summary>
    /// Gets a key/value mapping of the context's correlation keys
    /// </summary>
    [DataMember(Name = "correlationKeys", Order = 3), JsonPropertyName("correlationKeys"), JsonPropertyOrder(3), YamlMember(Alias = "correlationKeys", Order = 3)]
    public required EquatableDictionary<string, string>? CorrelationKeys { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the workflow instance has completed correlating events
    /// </summary>
    [DataMember(Name = "completedAt", Order = 4), JsonPropertyName("completedAt"), JsonPropertyOrder(4), YamlMember(Alias = "completedAt", Order = 4)]
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.Now;

}