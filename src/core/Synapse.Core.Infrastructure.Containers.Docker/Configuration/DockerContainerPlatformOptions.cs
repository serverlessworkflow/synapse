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

using Synapse.Resources;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Core.Infrastructure.Containers.Configuration;

/// <summary>
/// Represents the options used to configure the <see cref="DockerContainerPlatform"/>
/// </summary>
public class DockerContainerPlatformOptions
{

    /// <summary>
    /// Gets the default network to connect containers to
    /// </summary>
    public const string DefaultNetwork = "synapse";

    /// <summary>
    /// Gets/sets the Docker API to use
    /// </summary>
    [DataMember(Order = 1, Name = "api"), JsonPropertyOrder(1), JsonPropertyName("api"), YamlMember(Order = 1, Alias = "api")]
    public virtual DockerApiConfiguration Api { get; set; } = new();

    /// <summary>
    /// Gets/sets the network to connect containers to, if any
    /// </summary>
    [DataMember(Order = 2, Name = "network"), JsonPropertyOrder(2), JsonPropertyName("network"), YamlMember(Order = 2, Alias = "network")]
    public virtual string? Network { get; set; } = DefaultNetwork;

}
