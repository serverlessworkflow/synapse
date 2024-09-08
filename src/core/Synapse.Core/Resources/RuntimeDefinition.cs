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
/// Represents an object used to configure the runtime used to spawn workflow instance processes
/// </summary>
[DataContract]
public record RuntimeDefinition
{

    /// <summary>
    /// Gets/sets the options used to configure the native runtime
    /// </summary>
    [DataMember(Order = 1, Name = "native"), JsonPropertyOrder(1), JsonPropertyName("native"), YamlMember(Order = 1, Alias = "native")]
    public virtual NativeRuntimeConfiguration? Native { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the Docker runtime
    /// </summary>
    [DataMember(Order = 2, Name = "docker"), JsonPropertyOrder(2), JsonPropertyName("docker"), YamlMember(Order = 2, Alias = "docker")]
    public virtual DockerRuntimeConfiguration? Docker { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the Kubernetes runtime
    /// </summary>
    [DataMember(Order = 3, Name = "kubernetes"), JsonPropertyOrder(3), JsonPropertyName("kubernetes"), YamlMember(Order = 3, Alias = "kubernetes")]
    public virtual KubernetesRuntimeConfiguration? Kubernetes { get; set; }

    /// <summary>
    /// Gets the runtime mode
    /// </summary>
    [IgnoreDataMember, JsonIgnore, YamlIgnore]
    public virtual string Mode => this.Native != null ? OperatorRuntimeMode.Native : this.Docker != null ? OperatorRuntimeMode.Docker : this.Kubernetes != null ? OperatorRuntimeMode.Kubernetes : throw new Exception("The runtime mode must be set");

}
