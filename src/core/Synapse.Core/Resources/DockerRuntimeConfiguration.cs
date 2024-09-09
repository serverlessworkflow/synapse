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

using Docker.DotNet.Models;
using Neuroglia.Serialization.Yaml;

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure a Docker runtime 
/// </summary>
[DataContract]
public record DockerRuntimeConfiguration
{

    /// <summary>
    /// Gets the default network to connect Runner containers to
    /// </summary>
    public const string DefaultNetwork = "synapse";
    /// <summary>
    /// Gets the default runner container template
    /// </summary>
    public static readonly Config DefaultContainerTemplate = new()
    {
        Image = SynapseDefaults.Containers.Images.Runner
    };

    /// <summary>
    /// Initializes a new <see cref="DockerRuntimeConfiguration"/>
    /// </summary>
    public DockerRuntimeConfiguration()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Api.Endpoint);
        if (!string.IsNullOrWhiteSpace(env) && Uri.TryCreate(env, UriKind.RelativeOrAbsolute, out var uri)) this.Api.Endpoint = uri;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Api.Version);
        if (!string.IsNullOrWhiteSpace(env)) this.Api.Version = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Image.Registry);
        if (!string.IsNullOrWhiteSpace(env)) this.ImageRegistry = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Image.PullPolicy);
        if (!string.IsNullOrWhiteSpace(env)) this.ImagePullPolicy = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Secrets.Directory);
        if (!string.IsNullOrWhiteSpace(env)) this.Secrets.Directory = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Secrets.MountPath);
        if (!string.IsNullOrWhiteSpace(env)) this.Secrets.MountPath = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Network);
        if (!string.IsNullOrWhiteSpace(env)) this.Network = env;
    }

    /// <summary>
    /// Gets/sets the Docker API to use
    /// </summary>
    [DataMember(Order = 1, Name = "api"), JsonPropertyOrder(1), JsonPropertyName("api"), YamlMember(Order = 1, Alias = "api")]
    public virtual DockerApiConfiguration Api { get; set; } = new();

    /// <summary>
    /// Gets/sets the name of the image registry to use when pulling the runtime's container image
    /// </summary>
    [DataMember(Order = 2, Name = "imageRegistry"), JsonPropertyOrder(2), JsonPropertyName("imageRegistry"), YamlMember(Order = 2, Alias = "imageRegistry")]
    public virtual string? ImageRegistry { get; set; }

    /// <summary>
    /// Gets/sets the Docker image pull policy. Supported values are 'Always', 'IfNotPresent' and 'Never'. Defaults to 'Always'.
    /// </summary>
    [DataMember(Order = 3, Name = "imagePullPolicy"), JsonPropertyOrder(3), JsonPropertyName("imagePullPolicy"), YamlMember(Order = 3, Alias = "imagePullPolicy")]
    public virtual string ImagePullPolicy { get; set; } = Synapse.ImagePullPolicy.Always;

    /// <summary>
    /// Gets/sets the template to use to create runner containers
    /// </summary>
    [DataMember(Order = 4, Name = "containerTemplate"), JsonPropertyOrder(4), JsonPropertyName("containerTemplate"), YamlMember(Order = 4, Alias = "containerTemplate")]
    public virtual Config ContainerTemplate { get; set; } = LoadContainerTemplate();

    /// <summary>
    /// Gets/sets the path to the directory that contains the secrets to mount in runner containers on a per workflow configuration basis
    /// </summary>
    [DataMember(Order = 5, Name = "secrets"), JsonPropertyOrder(5), JsonPropertyName("secrets"), YamlMember(Order = 5, Alias = "secrets")]
    public virtual DockerRuntimeSecretsConfiguration Secrets { get; set; } = new();

    /// <summary>
    /// Gets/sets the name of the network, if any, to connect Runner containers to
    /// </summary>
    [DataMember(Order = 6, Name = "network"), JsonPropertyOrder(6), JsonPropertyName("network"), YamlMember(Order = 6, Alias = "network")]
    public virtual string Network { get; set; } = DefaultNetwork;

    /// <summary>
    /// Loads the runner container template
    /// </summary>
    /// <returns>The runner container template</returns>
    public static Config LoadContainerTemplate()
    {
        var templateFilePath = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Docker.Container);
        if (string.IsNullOrWhiteSpace(templateFilePath) || !File.Exists(templateFilePath)) return DefaultContainerTemplate;
        var yaml = File.ReadAllText(templateFilePath);
        return YamlSerializer.Default.Deserialize<Config>(yaml)!;
    }

}
