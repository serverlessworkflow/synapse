// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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
/// Represents the definition of a correlation outcome used to correlation an existing workflow instance
/// </summary>
[DataContract]
public record CorrelateWorkflowOutcomeDefinition
{

    /// <summary>
    /// Gets/sets a '{name}.{namespace}' reference to the workflow instance to correlate
    /// </summary>
    [Required]
    [DataMember(Name = "instance", Order = 1), JsonPropertyName("instance"), JsonPropertyOrder(1), YamlMember(Alias = "instance", Order = 1)]
    public required virtual string Instance { get; set; }

    /// <summary>
    /// Gets/sets the name of the task that consumes the correlated events
    /// </summary>
    [DataMember(Name = "task", Order = 2), JsonPropertyName("task"), JsonPropertyOrder(2), YamlMember(Alias = "task", Order = 2)]
    public required virtual string Task { get; set; }

}