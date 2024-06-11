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
/// Represents an object used to describe the status of a workflow instance correlation
/// </summary>
[DataContract]
public record WorkflowInstanceCorrelationStatus
{

    /// <summary>
    /// Gets/sets a name/value containing the workflow instance's correlation keys
    /// </summary>
    [DataMember(Order = 1, Name = "keys"), JsonPropertyName("keys"), JsonPropertyOrder(1), YamlMember(Alias = "keys", Order = 1)]
    public virtual EquatableDictionary<string, string>? Keys { get; set; }

    /// <summary>
    /// Gets/sets a name/value containing the workflow instance's correlation contexts pending processing
    /// </summary>
    [DataMember(Order = 2, Name = "contexts"), JsonPropertyName("contexts"), JsonPropertyOrder(2), YamlMember(Alias = "contexts", Order = 2)]
    public virtual EquatableDictionary<string, CorrelationContext>? Contexts { get; set; }

}