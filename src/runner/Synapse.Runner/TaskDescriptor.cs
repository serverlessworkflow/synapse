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
/// Represents an runtime expression argument used to describe the task being executed
/// </summary>
[DataContract]
public record TaskDescriptor
{

    /// <summary>
    /// Gets/sets the task's name
    /// </summary>
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public virtual string? Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the task's reference
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "reference", Order = 2), JsonPropertyName("reference"), JsonPropertyOrder(2), YamlMember(Alias = "reference", Order = 2)]
    public virtual string Reference { get; set; } = null!;

    /// <summary>
    /// Gets/sets the task's definition
    /// </summary>
    [Required]
    [DataMember(Name = "definition", Order = 3), JsonPropertyName("definition"), JsonPropertyOrder(3), YamlMember(Alias = "definition", Order = 3)]
    public virtual TaskDefinition Definition { get; set; } = null!;

    /// <summary>
    /// Gets/sets the task's raw, untransformed input
    /// </summary>
    [DataMember(Name = "input", Order = 4), JsonPropertyName("input"), JsonPropertyOrder(4), YamlMember(Alias = "input", Order = 4)]
    public virtual object? Input { get; set; }

    /// <summary>
    /// Gets/sets the task's raw, untransformed output
    /// </summary>
    [DataMember(Name = "output", Order = 5), JsonPropertyName("output"), JsonPropertyOrder(5), YamlMember(Alias = "output", Order = 5)]
    public virtual object? Output { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the task has started
    /// </summary>
    [DataMember(Name = "startedAt", Order = 6), JsonPropertyName("startedAt"), JsonPropertyOrder(6), YamlMember(Alias = "startedAt", Order = 6)]
    public virtual DateTimeDescriptor? StartedAt { get; set; } = null!;

}
