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

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to describe the status of a correlation
/// </summary>
[DataContract]
public record CorrelationStatus
{

    /// <summary>
    /// Gets/sets the correlation's status phase
    /// </summary>
    [DataMember(Name = "phase", Order = 1), JsonPropertyName("phase"), JsonPropertyOrder(1), YamlMember(Alias = "phase", Order = 1)]
    public virtual string? Phase { get; set; }

    /// <summary>
    /// Gets/sets the date and time the correlation was last modified at
    /// </summary>
    [DataMember(Name = "lastModified", Order = 2), JsonPropertyName("lastModified"), JsonPropertyOrder(2), YamlMember(Alias = "lastModified", Order = 2)]
    public virtual DateTimeOffset? LastModified { get; set; }

    /// <summary>
    /// Gets/sets a list containing the contexts that have been created for the described correlation
    /// </summary>
    [DataMember(Name = "contexts", Order = 3), JsonPropertyName("contexts"), JsonPropertyOrder(3), YamlMember(Alias = "contexts", Order = 3)]
    public virtual EquatableList<CorrelationContext> Contexts { get; set; } = [];

}
