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
/// Represents the object used to configure the desired state of an <see cref="Correlator"/>
/// </summary>
[DataContract]
public record CorrelatorSpec
{

    /// <summary>
    /// Gets/sets a key/value mapping of the labels to select correlations by.<para></para>
    /// If not set, the broker will attempt to pick up all unclaimed workflows and workflow instances
    /// </summary>
    [DataMember(Order = 1, Name = "selector"), JsonPropertyOrder(1), JsonPropertyName("selector"), YamlMember(Order = 1, Alias = "selector")]
    public virtual IDictionary<string, string>? Selector { get; set; }

}
