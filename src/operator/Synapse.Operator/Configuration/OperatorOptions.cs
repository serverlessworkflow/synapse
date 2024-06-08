// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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
        Namespace = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Namespace)!;
        Name = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Name)!;
        var uri = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Api);
        if (!string.IsNullOrWhiteSpace(uri)) this.Runner.Api.Uri = new(uri);
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
    public virtual RunnerDefinition Runner { get; set; } = new();

}
