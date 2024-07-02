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
/// Represents a document
/// </summary>
[DataContract]
public record Document
    : IIdentifiable<string>
{

    /// <inheritdoc/>
    [DataMember(Name = "id", Order = 1), JsonPropertyName("id"), JsonPropertyOrder(1), YamlMember(Alias = "id", Order = 1)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..15];

    /// <summary>
    /// Gets/sets the document's name
    /// </summary>
    [Required]
    [DataMember(Name = "name", Order = 2), JsonPropertyName("name"), JsonPropertyOrder(2), YamlMember(Alias = "name", Order = 2)]
    public required virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets the document's content
    /// </summary>
    [Required]
    [DataMember(Name = "content", Order = 3), JsonPropertyName("content"), JsonPropertyOrder(3), YamlMember(Alias = "content", Order = 3)]
    public required virtual object Content { get; set; } = null!;

    [IgnoreDataMember, JsonIgnore, YamlIgnore]
    object IIdentifiable.Id => this.Name;

    bool IEquatable<IIdentifiable<string>>.Equals(IIdentifiable<string>? other) => other != null && this.Name.Equals(other?.Id);

}