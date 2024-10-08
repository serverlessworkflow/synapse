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

namespace Synapse.Correlator.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Correlator application
/// </summary>
public class CorrelatorOptions
{

    /// <summary>
    /// Initializes a new <see cref="CorrelatorOptions"/>
    /// </summary>
    public CorrelatorOptions()
    {
        this.Namespace = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Correlator.Namespace)!;
        this.Name = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Correlator.Name)!;
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Correlator.Events.ConsumerGroup);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Events ??= new();
            this.Events.ConsumerGroup = env;
        }
    }

    /// <summary>
    /// Gets/sets the correlator's namespace
    /// </summary>
    public virtual string Namespace { get; set; }

    /// <summary>
    /// Gets/sets the correlator's name
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the way a Synapse Correlator consumes cloud events
    /// </summary>
    public virtual CloudEventConsumptionOptions? Events { get; set; }

}
