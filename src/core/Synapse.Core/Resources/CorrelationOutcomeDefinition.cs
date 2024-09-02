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
/// Represents the correlation's outcome
/// </summary>
[DataContract]
public record CorrelationOutcomeDefinition
{

    /// <summary>
    /// Gets the type of the correlation outcome
    /// </summary>
    public virtual string Type => this.Start != null ? CorrelationOutcomeType.Start : this.Correlate != null ? CorrelationOutcomeType.Correlate : throw new NotSupportedException($"The specified correlation outcome type is not supported");

    /// <summary>
    /// Gets/sets an object used to configure the outcome, if any, used to start a new workflow instance,. Is mutually exclusive to <see cref="Correlate"/>
    /// </summary>

    [DataMember(Name = "start", Order = 1), JsonPropertyName("start"), JsonPropertyOrder(1), YamlMember(Alias = "start", Order = 1)]
    public virtual StartWorkflowOutcomeDefinition? Start { get; set; }

    /// <summary>
    /// Gets/sets an object used to configure the outcome, if any, used to correlate a correlation context to an existing workflow instance. Is mutually exclusive to <see cref="Start"/>
    /// </summary>
    [DataMember(Name = "correlate", Order = 2), JsonPropertyName("correlate"), JsonPropertyOrder(2), YamlMember(Alias = "correlate", Order = 2)]
    public virtual CorrelateWorkflowOutcomeDefinition? Correlate { get; set; }

}
