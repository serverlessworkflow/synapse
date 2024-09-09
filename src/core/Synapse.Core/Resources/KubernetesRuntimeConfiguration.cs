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

using k8s.Models;
using Neuroglia.Serialization.Yaml;

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure a Kubernetes runtime
/// </summary>
[DataContract]
public record KubernetesRuntimeConfiguration
{

    /// <summary>
    /// Gets the default worker <see cref="V1Pod"/>
    /// </summary>
    public static readonly V1Pod DefaultPodTemplate = new()
    {
        Metadata = new(),
        Spec = new()
        {
            RestartPolicy = "Never",
            Containers =
            [
                new("runner")
                {
                    Image = SynapseDefaults.Containers.Images.Runner,
                    ImagePullPolicy = ImagePullPolicy.Always,
                    Ports =
                    [
                        new()
                        {
                            Name = "http",
                            ContainerPort = 8080,
                            Protocol = "TCP"
                        }
                    ]
                }
            ]
        }
    };

    /// <summary>
    /// Initializes a new <see cref="KubernetesRuntimeConfiguration"/>
    /// </summary>
    public KubernetesRuntimeConfiguration()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Kubernetes.Kubeconfig);
        if (!string.IsNullOrWhiteSpace(env)) this.Kubeconfig = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Kubernetes.Secrets.VolumeName);
        if (!string.IsNullOrWhiteSpace(env)) this.Secrets.VolumeName = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Kubernetes.Secrets.MountPath);
        if (!string.IsNullOrWhiteSpace(env)) this.Secrets.MountPath = env;
    }

    /// <summary>
    /// Gets/sets the path to the Kubeconfig file to use, if any. If not set, defaults to 'InCluster' configuration
    /// </summary>
    [DataMember(Order = 1, Name = "kubeconfig"), JsonPropertyOrder(1), JsonPropertyName("kubeconfig"), YamlMember(Order = 1, Alias = "kubeconfig")]
    public virtual string? Kubeconfig { get; set; }

    /// <summary>
    /// Gets/sets the template to use to create runner containers
    /// </summary>
    [DataMember(Order = 2, Name = "podTemplate"), JsonPropertyOrder(2), JsonPropertyName("podTemplate"), YamlMember(Order = 2, Alias = "podTemplate")]
    public virtual V1Pod PodTemplate { get; set; } = LoadPodTemplate();

    /// <summary>
    /// Gets/sets the configuration of the secrets used by the Kubernetes runtime
    /// </summary>
    [DataMember(Order = 3, Name = "secrets"), JsonPropertyOrder(3), JsonPropertyName("secrets"), YamlMember(Order = 3, Alias = "secrets")]
    public virtual KubernetesRuntimeSecretsConfiguration Secrets { get; set; } = new();

    /// <summary>
    /// Loads the runner container template
    /// </summary>
    /// <returns>The runner container template</returns>
    public static V1Pod LoadPodTemplate()
    {
        var templateFilePath = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Kubernetes.Pod);
        if (string.IsNullOrWhiteSpace(templateFilePath) || !File.Exists(templateFilePath)) return DefaultPodTemplate;
        var yaml = File.ReadAllText(templateFilePath);
        return YamlSerializer.Default.Deserialize<V1Pod>(yaml)!;
    }

}
