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

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the service account used by a Synapse Runner application
/// </summary>
public class ServiceAccountOptions
{

    /// <summary>
    /// Initializes a new <see cref="ServiceAccountOptions"/>
    /// </summary>
    public ServiceAccountOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Name);
        if (!string.IsNullOrWhiteSpace(env)) this.Name = env;
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Key);
        if (!string.IsNullOrWhiteSpace(env)) this.Key = env;
    }

    /// <summary>
    /// Gets/sets the name of the runner's service account
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the key of the runner's service account
    /// </summary>
    public virtual string Key { get; set; } = null!;

}