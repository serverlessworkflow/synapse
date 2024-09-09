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

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Core.Infrastructure.Containers.Configuration;

/// <summary>
/// Represents the options used to configure the <see cref="KubernetesContainerPlatform"/>
/// </summary>
public class KubernetesContainerPlatformOptions
{

    /// <summary>
    /// Gets/sets the path to the Kubeconfig file to use, if any. If not set, defaults to 'InCluster' configuration
    /// </summary>
    [DataMember(Order = 1, Name = "kubeconfig"), JsonPropertyOrder(1), JsonPropertyName("kubeconfig"), YamlMember(Order = 1, Alias = "kubeconfig")]
    public virtual string? Kubeconfig { get; set; }

    /// <summary>
    /// Gets/sets the Kubernetes image pull policy. Supported values are 'Always', 'IfNotPresent' and 'Never'. Defaults to 'Always'.
    /// </summary>
    [DataMember(Order = 2, Name = "imagePullPolicy"), JsonPropertyOrder(2), JsonPropertyName("imagePullPolicy"), YamlMember(Order = 2, Alias = "imagePullPolicy")]
    public virtual string ImagePullPolicy { get; set; } = Synapse.ImagePullPolicy.Always;

}
