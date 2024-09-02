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

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Runner;

/// <summary>
/// Represents an object used to describe a datetime
/// </summary>
[DataContract]
public record DateTimeDescriptor
{

    /// <summary>
    /// Gets/sets the ISO 8601 representation of the described datetime
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "iso8601", Order = 1), JsonPropertyName("iso8601"), JsonPropertyOrder(1), YamlMember(Alias = "iso8601", Order = 1)]
    public virtual string Iso8601 { get; set; } = null!;

    /// <summary>
    /// Gets/sets the duration elapsed between the described datetime and midnight of 1970-01-01 UTC
    /// </summary>
    [Required]
    [DataMember(Name = "epoch", Order = 2), JsonPropertyName("epoch"), JsonPropertyOrder(2), YamlMember(Alias = "epoch", Order = 2)]
    public virtual Epoch Epoch { get; set; } = null!;

}
