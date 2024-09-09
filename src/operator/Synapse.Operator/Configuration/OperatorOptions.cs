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

namespace Synapse.Operator.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Operator application
/// </summary>
public class OperatorOptions
{

    /// <summary>
    /// Initializes a new <see cref="OperatorOptions"/>
    /// </summary>
    public OperatorOptions()
    {
        this.Namespace = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Namespace)!;
        this.Name = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Name)!;
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Mode);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Runner.Runtime = env switch
            {
                OperatorRuntimeMode.Docker => new()
                {
                    Docker = new()
                },
                OperatorRuntimeMode.Kubernetes => new()
                {
                    Kubernetes = new()
                },
                OperatorRuntimeMode.Native => new()
                {
                    Native = new()
                },
                _ => throw new NotSupportedException($"The specified operator runtime mode '{env}' is not supported"),
            };
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Api);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Runner ??= new();
            if (this.Runner.Api == null) this.Runner.Api ??= new()
            {
                Uri = new(env)
            };
            else this.Runner.Api.Uri = new(env);
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.LifecycleEvents);
        if (!string.IsNullOrWhiteSpace(env) && bool.TryParse(env, out var publishLifeCycleEvents))
        {
            this.Runner ??= new();
            this.Runner.PublishLifecycleEvents = publishLifeCycleEvents;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.ContainerPlatform);
        if (!string.IsNullOrWhiteSpace(env)) this.Runner.ContainerPlatform = env;
    }

    /// <summary>
    /// Gets/sets the operator's namespace
    /// </summary>
    public virtual string Namespace { get; set; }

    /// <summary>
    /// Gets/sets the operator's name
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the runners spawned by a Synapse Operator
    /// </summary>
    public virtual RunnerConfiguration Runner { get; set; } = new();

}
