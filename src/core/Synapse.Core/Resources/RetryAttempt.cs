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
/// Represents an object used to describe a retry attempt
/// </summary>
[DataContract]
public record RetryAttempt
{

    /// <summary>
    /// Gets/sets the retry attempt number
    /// </summary>
    [Required]
    [DataMember(Name = "number", Order = 1), JsonPropertyName("number"), JsonPropertyOrder(1), YamlMember(Alias = "number", Order = 1)]
    public required virtual uint Number { get; set; }

    /// <summary>
    /// Gets/sets the date and time at which the retry attempt was performed
    /// </summary>
    [DataMember(Name = "time", Order = 2), JsonPropertyName("time"), JsonPropertyOrder(2), YamlMember(Alias = "time", Order = 2)]
    public virtual DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Gets/sets the <see cref="Error"/> that is the cause of the try attempt
    /// </summary>
    [Required]
    [DataMember(Name = "cause", Order = 3), JsonPropertyName("cause"), JsonPropertyOrder(3), YamlMember(Alias = "cause", Order = 3)]
    public required virtual Error Cause { get; set; }

}