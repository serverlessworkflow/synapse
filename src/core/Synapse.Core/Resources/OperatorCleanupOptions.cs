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
/// Represents the options used to configure the cleanup behavior of a Synapse Operator
/// </summary>
[DataContract]
public record OperatorCleanupOptions
{

    /// <summary>
    /// Gets or sets the time to live for completed workflow instances. Defaults to 7 days. If null, the operator will not delete completed workflow instances.
    /// </summary>
    [DataMember(Order = 1, Name = "ttl"), JsonPropertyOrder(1), JsonPropertyName("ttl"), YamlMember(Order = 1, Alias = "ttl")]
    public TimeSpan? Ttl { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the interval at which the operator sweeps for completed workflow instances to delete. Defaults to 5 minutes.
    /// </summary>
    [DataMember(Order = 2, Name = "interval"), JsonPropertyOrder(2), JsonPropertyName("interval"), YamlMember(Order = 2, Alias = "interval")]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

}
