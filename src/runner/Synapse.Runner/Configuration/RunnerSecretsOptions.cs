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

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the secrets of a Synapse Runner
/// </summary>
[DataContract]
public class RunnerSecretsOptions
{

    /// <summary>
    /// Gets the default directory where to locate the runner's secrets
    /// </summary>
    public static readonly string DefaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "secrets");

    /// <summary>
    /// Gets/sets the directory where the runner's secrets are located
    /// </summary>
    public virtual string? Directory { get; set; } = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Secrets.Directory);

}