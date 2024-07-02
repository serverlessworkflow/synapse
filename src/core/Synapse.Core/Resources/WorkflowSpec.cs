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
/// Represents the configuration of a workflow
/// </summary>
[DataContract]
public record WorkflowSpec
{

    /// <summary>
    /// Gets/sets the versions of the configured workflow
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "versions", Order = 1), JsonPropertyName("versions"), JsonPropertyOrder(1), YamlMember(Alias = "versions", Order = 1)]
    public virtual EquatableList<WorkflowDefinition> Versions { get; set; } = null!;

}