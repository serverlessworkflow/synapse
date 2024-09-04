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
/// Represents an object used to configure the secrets of a Docker runtime
/// </summary>
[DataContract]
public record DockerRuntimeSecretsConfiguration
{

    /// <summary>
    /// Gets the default secrets directory
    /// </summary>
    public static string DefaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".synapse", "secrets");
    /// <summary>
    /// Gets the default name for the volume on which to mount secrets
    /// </summary>
    public const string DefaultMountPath = "/run/secrets/synapse";

    /// <summary>
    /// Gets/sets the path to the directory that contains the secrets to mount
    /// </summary>
    [DataMember(Order = 1, Name = "directory"), JsonPropertyOrder(1), JsonPropertyName("directory"), YamlMember(Order = 1, Alias = "directory")]
    public virtual string Directory { get; set; } = DefaultDirectory;

    /// <summary>
    /// Gets/sets the path to the folder to mount the secrets volume to
    /// </summary>
    [DataMember(Order = 2, Name = "mountPath"), JsonPropertyOrder(2), JsonPropertyName("mountPath"), YamlMember(Order = 2, Alias = "mountPath")]
    public virtual string MountPath { get; set; } = DefaultMountPath;

}
