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

namespace Synapse.Cli.Configuration;

/// <summary>
/// Represents the object used to configure the Synapse API used by the CLI
/// </summary>
[DataContract]
public record ApplicationApiOptions
{

    /// <summary>
    /// Gets/sets a name/value mapping of all configured APIs
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "configurations", Order = 1), JsonPropertyOrder(1), JsonPropertyName("configurations"), YamlMember(Alias = "configurations", Order = 1)]
    public virtual IDictionary<string, ApiConfiguration> Configurations { get; set; } = new Dictionary<string, ApiConfiguration>();

    /// <summary>
    /// Gets/sets the name of the API currently used by the Synapse CLI
    /// </summary>
    [DataMember(Name = "current", Order = 2), JsonPropertyOrder(2), JsonPropertyName("current"), YamlMember(Alias = "current", Order = 2)]
    public string? Current { get; set; }

}
