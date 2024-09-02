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
/// Represents the default implementation of the <see cref="ITaskLifeCycleEvent"/>
/// </summary>
/// <param name="Type">The <see cref="ITaskLifeCycleEvent"/>'s type</param>
/// <param name="Data">The <see cref="ITaskLifeCycleEvent"/>'s data, if any</param>
[DataContract]
public record TaskLifeCycleEvent(string Type, object? Data = null)
    : ITaskLifeCycleEvent
{

    /// <inheritdoc/>
    [Required, MinLength(1)]
    [DataMember(Name = "type", Order = 1), JsonPropertyName("type"), JsonPropertyOrder(1), YamlMember(Alias = "type", Order = 1)]
    public virtual string Type { get; set; } = Type;

    /// <inheritdoc/>
    [Required, MinLength(2)]
    [DataMember(Name = "data", Order = 2), JsonPropertyName("data"), JsonPropertyOrder(2), YamlMember(Alias = "data", Order = 2)]
    public virtual object? Data { get; set; } = Data;

}
