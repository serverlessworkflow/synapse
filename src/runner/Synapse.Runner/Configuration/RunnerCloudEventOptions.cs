﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the cloud events produced by a Synapse Runner
/// </summary>
[DataContract]
public class RunnerCloudEventOptions
{

    /// <summary>
    /// Initializes a new <see cref="RunnerCloudEventOptions"/>
    /// </summary>
    public RunnerCloudEventOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.LifecycleEvents);
        if (!string.IsNullOrWhiteSpace(env) && bool.TryParse(env, out var publishLifecycleEvents)) this.PublishLifecycleEvents = publishLifecycleEvents;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.CloudEvents.Sink);
        if (!string.IsNullOrWhiteSpace(env) && Uri.TryCreate(env, UriKind.RelativeOrAbsolute, out var sink)) this.Sink = sink;
    }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not the Synapse Runner should produce lifecycle events. Defaults to `true`.
    /// </summary>
    public virtual bool PublishLifecycleEvents { get; set; } = true;

    /// <summary>
    /// Gets/sets the 
    /// </summary>
    public virtual Uri? Sink { get; set; }

}
