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
/// Represents the definition of a correlation outcome used to start a new instance of a workflow
/// </summary>
[DataContract]
public record StartWorkflowOutcomeDefinition
{

    /// <summary>
    /// Gets/sets the workflow to start upon correlation
    /// </summary>
    [Required]
    [DataMember(Name = "workflow", Order = 1), JsonPropertyName("workflow"), JsonPropertyOrder(1), YamlMember(Alias = "workflow", Order = 1)]
    public required virtual WorkflowDefinitionReference Workflow { get; set; }

    /// <summary>
    /// Gets/sets a key/value mapping of the input of the workflow to start upon correlation
    /// </summary>
    [Required]
    [DataMember(Name = "input", Order = 2), JsonPropertyName("input"), JsonPropertyOrder(2), YamlMember(Alias = "input", Order = 2)]
    public virtual IDictionary<string, object>? Input { get; set; }

}
