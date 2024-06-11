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
/// Represents the options used to configure the Synapse CLI application
/// </summary>
[DataContract]
public record ApplicationOptions
{

    /// <summary>
    /// Gets/sets the name of the API currently used by the Synapse CLI
    /// </summary>
    [DataMember(Name = "api", Order = 1), JsonPropertyOrder(1), JsonPropertyName("api"), YamlMember(Alias = "api", Order = 1)]
    public ApplicationApiOptions Api { get; set; } = new();

}
