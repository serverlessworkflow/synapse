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
/// Represents an object used to configure a Synapse workflow runner
/// </summary>
[DataContract]
public record RunnerConfiguration
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
        Docker = new()
    };

    /// <summary>
    /// Gets/sets the container platform used by runners to spawn containers
    /// </summary>
    [Required]
    [DataMember(Order = 3, Name = "containerPlatform"), JsonPropertyOrder(3), JsonPropertyName("containerPlatform"), YamlMember(Order = 3, Alias = "containerPlatform")]
    public virtual string ContainerPlatform { get; set; } = Synapse.ContainerPlatform.Docker;

    /// <summary>
    /// Gets/sets the endpoint that references the base address and authentication policy for the Synapse API used by runners
    /// </summary>
    [DataMember(Order = 4, Name = "certificates"), JsonPropertyOrder(4), JsonPropertyName("certificates"), YamlMember(Order = 4, Alias = "certificates")]
    public virtual CertificateValidationStrategyDefinition? Certificates { get; set; }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not runners spawned by the configured Synapse Operators should publish lifecycle events
    /// </summary>
    [DataMember(Order = 5, Name = "publishLifecycleEvents"), JsonPropertyOrder(5), JsonPropertyName("publishLifecycleEvents"), YamlMember(Order = 5, Alias = "publishLifecycleEvents")]
    public virtual bool? PublishLifecycleEvents { get; set; } = true;

}