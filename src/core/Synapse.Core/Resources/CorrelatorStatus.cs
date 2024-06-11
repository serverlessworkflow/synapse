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
/// Represents an object used to describe the status of an correlator
/// </summary>
[DataContract]
public record CorrelatorStatus
{

    /// <summary>
    /// Gets/sets the correlator's current status phase
    /// </summary>
    [DataMember(Name = "phase", Order = 1), JsonPropertyName("phase"), JsonPropertyOrder(1), YamlMember(Alias = "phase", Order = 1)]
    public virtual string? Phase { get; set; }

}