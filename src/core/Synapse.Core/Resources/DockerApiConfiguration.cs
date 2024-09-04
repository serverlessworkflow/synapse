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

using System.Runtime.InteropServices;

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure the Docker API to use
/// </summary>
[DataContract]
public record DockerApiConfiguration
{

    /// <summary>
    /// Gets/sets the endpoint of the Docker API to use
    /// </summary>
    [DataMember(Order = 1, Name = "endpoint"), JsonPropertyOrder(1), JsonPropertyName("endpoint"), YamlMember(Order = 1, Alias = "endpoint")]
    public virtual Uri Endpoint { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");

    /// <summary>
    /// Gets/sets the version of the Docker API to use
    /// </summary>
    [DataMember(Order = 2, Name = "version"), JsonPropertyOrder(2), JsonPropertyName("version"), YamlMember(Order = 2, Alias = "version")]
    public virtual string? Version { get; set; }

}
