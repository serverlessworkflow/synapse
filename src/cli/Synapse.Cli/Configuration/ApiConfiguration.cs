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
/// Represents the named options used to configure a Synapse API to connect to using the Synapse CLI
/// </summary>
[DataContract]
public class ApiConfiguration
{

    /// <summary>
    /// Gets/sets the uri that references the API server to connect to
    /// </summary>
    [DataMember(Name = "server", Order = 1), JsonPropertyOrder(1), JsonPropertyName("server"), YamlMember(Alias = "server", Order = 1)]
    public required virtual Uri Server { get; set; }

    /// <summary>
    /// Gets/sets the token used to authenticate on the API server
    /// </summary>
    [DataMember(Name = "token", Order = 2), JsonPropertyOrder(2), JsonPropertyName("token"), YamlMember(Alias = "token", Order = 2)]
    public required virtual string Token { get; set; }

}