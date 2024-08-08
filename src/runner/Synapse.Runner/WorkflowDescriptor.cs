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

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Runner;

/// <summary>
/// Represents an runtime expression argument used to describe the workflow being executed
/// </summary>
[DataContract]
public record WorkflowDescriptor
{

    /// <summary>
    /// Gets/sets the workflow's id
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "id", Order = 1), JsonPropertyName("id"), JsonPropertyOrder(1), YamlMember(Alias = "id", Order = 1)]
    public virtual string Id { get; set; } = null!;

    /// <summary>
    /// Gets/sets the workflow's definition
    /// </summary>
    [Required]
    [DataMember(Name = "definition", Order = 2), JsonPropertyName("definition"), JsonPropertyOrder(2), YamlMember(Alias = "definition", Order = 2)]
    public virtual WorkflowDefinition Definition { get; set; } = null!;

    /// <summary>
    /// Gets/sets the workflow's raw, untransformed input
    /// </summary>
    [DataMember(Name = "input", Order = 3), JsonPropertyName("input"), JsonPropertyOrder(3), YamlMember(Alias = "input", Order = 3)]
    public virtual object? Input { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the workflow has started
    /// </summary>
    [Required]
    [DataMember(Name = "startedAt", Order = 4), JsonPropertyName("startedAt"), JsonPropertyOrder(4), YamlMember(Alias = "startedAt", Order = 4)]
    public virtual DateTimeDescriptor StartedAt { get; set; } = null!;

}
