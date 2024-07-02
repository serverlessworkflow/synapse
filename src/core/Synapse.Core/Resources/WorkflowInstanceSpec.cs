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
/// Represents the configuration of a workflow instance resource
/// </summary>
[DataContract]
public record WorkflowInstanceSpec
{

    /// <summary>
    /// Gets/sets the definition of the workflow to run
    /// </summary>
    [Required]
    [DataMember(Name = "definition", Order = 1), JsonPropertyName("definition"), JsonPropertyOrder(1), YamlMember(Alias = "definition", Order = 1)]
    public virtual WorkflowDefinitionReference Definition { get; set; } = null!;

    /// <summary>
    /// Gets/sets a name/value mapping of the workflow's input data
    /// </summary>
    [Required]
    [DataMember(Name = "input", Order = 2), JsonPropertyName("input"), JsonPropertyOrder(2), YamlMember(Alias = "input", Order = 2)]
    public virtual EquatableDictionary<string, object>? Input { get; set; }

}
