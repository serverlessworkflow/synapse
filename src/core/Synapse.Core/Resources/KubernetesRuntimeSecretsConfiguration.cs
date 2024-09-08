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
/// Represents an object used to configure the secrets of a Kubernetes runtime
/// </summary>
[DataContract]
public record KubernetesRuntimeSecretsConfiguration
{

    /// <summary>
    /// Gets the default name for the volume on which to mount secrets
    /// </summary>
    public const string DefaultVolumeName = "secrets";
    /// <summary>
    /// Gets the default name for the volume on which to mount secrets
    /// </summary>
    public const string DefaultMountPath = "/run/secrets/synapse";

    /// <summary>
    /// Gets/sets the name on which to mounts secrets
    /// </summary>
    [DataMember(Order = 1, Name = "volumeName"), JsonPropertyOrder(1), JsonPropertyName("volumeName"), YamlMember(Order = 1, Alias = "volumeName")]
    public virtual string VolumeName { get; set; } = DefaultVolumeName;

    /// <summary>
    /// Gets/sets the path to the folder to mount the secrets volume to
    /// </summary>
    [DataMember(Order = 2, Name = "mountPath"), JsonPropertyOrder(2), JsonPropertyName("mountPath"), YamlMember(Order = 2, Alias = "mountPath")]
    public virtual string MountPath { get; set; } = DefaultMountPath;

}