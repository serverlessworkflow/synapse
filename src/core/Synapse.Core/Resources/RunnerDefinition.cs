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

namespace Synapse.Resources;

/// <summary>
/// Represents the definition of a Synapse workflow runner
/// </summary>
[DataContract]
public record RunnerDefinition
{

    /// <summary>
    /// Gets/sets the endpoint that references the base address and authentication policy for the Synapse API used by runners
    /// </summary>
    [Required]
    [DataMember(Order = 1, Name = "api"), JsonPropertyOrder(1), JsonPropertyName("api"), YamlMember(Order = 1, Alias = "api")]
    public virtual EndpointDefinition Api { get; set; } = null!;

    /// <summary>
    /// Gets/sets the options used to configure the runtime used by the Synapse Operator to spawn workflow instance processes
    /// </summary>
    [Required]
    [DataMember(Order = 2, Name = "runtime"), JsonPropertyOrder(2), JsonPropertyName("runtime"), YamlMember(Order = 2, Alias = "runtime")]
    public virtual RuntimeDefinition Runtime { get; set; } = new()
    {
        Container = new()
        {
            Image = "ghcr.io/serverlessworkflow/synapse/runner"
        }
    };

    /// <summary>
    /// Gets/sets the endpoint that references the base address and authentication policy for the Synapse API used by runners
    /// </summary>
    [Required]
    [DataMember(Order = 3, Name = "certificate"), JsonPropertyOrder(3), JsonPropertyName("certificate"), YamlMember(Order = 3, Alias = "certificate")]
    public virtual CertificateValidationStrategyDefinition Certificates { get; set; } = null!;
    
}
