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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure a correlation
/// </summary>
[DataContract]
public record CorrelationSpec
{

    /// <summary>
    /// Gets/sets the correlation's source (user or workflow)
    /// </summary>
    [DataMember(Name = "source", Order = 1), JsonPropertyName("source"), JsonPropertyOrder(1), YamlMember(Alias = "source", Order = 1)]
    public virtual ResourceReference Source { get; set; } = null!;

    /// <summary>
    /// Gets/sets the correlation's lifetime
    /// </summary>
    [DataMember(Name = "lifetime", Order = 2), JsonPropertyName("lifetime"), JsonPropertyOrder(2), YamlMember(Alias = "lifetime", Order = 2)]
    public virtual string Lifetime { get; set; } = null!;

    /// <summary>
    /// Gets/sets an object used to define the events to correlate
    /// </summary>
    [DataMember(Name = "events", Order = 3), JsonPropertyName("events"), JsonPropertyOrder(3), YamlMember(Alias = "events", Order = 3)]
    public virtual EventConsumptionStrategyDefinition Events { get; set; } = null!;

}
