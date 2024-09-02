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

using Neuroglia;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Runner;

/// <summary>
/// Represents an runtime expression argument used to describe the current runtime
/// </summary>
[DataContract]
public record RuntimeDescriptor
{

    /// <summary>
    /// Gets the object used to describe the current runtime
    /// </summary>
    public static readonly RuntimeDescriptor Current = new()
    {
        Name = "Synapse",
        Version = typeof(RuntimeDescriptor).Assembly.GetName().Version!.ToString(3)
    };

    /// <summary>
    /// Gets/sets the runtime's name
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the runtime's version
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "version", Order = 2), JsonPropertyName("version"), JsonPropertyOrder(2), YamlMember(Alias = "version", Order = 2)]
    public virtual string Version { get; set; } = null!;

    /// <summary>
    /// Gets/sets a key/value mapping of the runtime's metadata, if any
    /// </summary>
    [DataMember(Name = "metadata", Order = 3), JsonPropertyName("metadata"), JsonPropertyOrder(3), YamlMember(Alias = "metadata", Order = 3)]
    public virtual EquatableDictionary<string, object>? Metadata { get; set; }

}
