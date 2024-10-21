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
/// Represents the options used to configure how a Synapse Runner should handle certificates
/// </summary>
[DataContract]
public class RunnerCertificateOptions
{

    /// <summary>
    /// Initializes a new <see cref="RunnerCertificateOptions"/>
    /// </summary>
    public RunnerCertificateOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.SkipCertificateValidation);
        if (!string.IsNullOrWhiteSpace(env) && bool.TryParse(env, out var skipValidation)) this.Validate = !skipValidation;
    }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not the runner should validate certificates
    /// </summary>
    public virtual bool Validate { get; set; } = true;

}