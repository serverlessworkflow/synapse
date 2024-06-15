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
/// Represents the object used to configure a service account
/// </summary>
[DataContract]
public record ServiceAccountSpec
{

    /// <summary>
    /// Gets/sets the service account's key
    /// </summary>
    [DataMember(Name = "key", Order = 1), JsonPropertyName("key"), JsonPropertyOrder(1), YamlMember(Alias = "key", Order = 1)]
    public virtual string Key { get; set; } = null!;

    /// <summary>
    /// Gets/sets the claims associated to the service account
    /// </summary>
    [DataMember(Name = "claims", Order = 2), JsonPropertyName("claims"), JsonPropertyOrder(2), YamlMember(Alias = "claims", Order = 2)]
    public virtual IDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();

}