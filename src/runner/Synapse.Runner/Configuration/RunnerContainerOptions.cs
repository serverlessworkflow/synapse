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

using Synapse.Core.Infrastructure.Containers.Configuration;
using System.Runtime.Serialization;

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the containers spawned by a Synapse Runner
/// </summary>
[DataContract]
public class RunnerContainerOptions
{

    /// <summary>
    /// Initializes a new <see cref="RunnerContainerOptions"/>
    /// </summary>
    public RunnerContainerOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.ContainerPlatform)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(env)) throw new NullReferenceException("The runner's container platform must be configured");
        switch (env)
        {
            case ContainerPlatform.Docker:
                this.Docker = new();
                break;
            case ContainerPlatform.Kubernetes:
                this.Kubernetes = new();
                break;
            default: throw new NotSupportedException($"The specified container platform '{env}' is not supported");
        }
    }

    /// <summary>
    /// Gets/sets the options used to configure the Docker container platform, if any
    /// </summary>
    public virtual DockerContainerPlatformOptions? Docker { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the Kubernetes container platform, if any
    /// </summary>
    public virtual KubernetesContainerPlatformOptions? Kubernetes { get; set; }

    /// <summary>
    /// Gets the container platform used by the configured runner
    /// </summary>
    public virtual string Platform => this.Docker != null ? ContainerPlatform.Docker : this.Kubernetes != null ? ContainerPlatform.Kubernetes : throw new NullReferenceException("The runner's container platform must be configured");

}