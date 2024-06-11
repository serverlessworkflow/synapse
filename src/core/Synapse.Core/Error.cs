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

namespace Synapse;

/// <summary>
/// Represents an object used to describe an error or problem, as defined by <see href="https://www.rfc-editor.org/rfc/rfc7807">RFC 7807</see>
/// </summary>
[DataContract]
public record Error
    : IExtensible
{

    /// <summary>
    /// Gets/sets an uri that reference the type of the described problem.
    /// </summary>
    [DataMember(Order = 1, Name = "type"), JsonPropertyName("type"), JsonPropertyOrder(1), YamlMember(Alias = "type", Order = 1)]
    public required virtual Uri Type { get; set; }

    /// <summary>
    /// Gets/sets a short, human-readable summary of the problem type.It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
    /// </summary>
    [DataMember(Order = 2, Name = "title"), JsonPropertyName("title"), JsonPropertyOrder(2), YamlMember(Alias = "title", Order = 2)]
    public required virtual string Title { get; set; }

    /// <summary>
    /// Gets/sets the status code produced by the described problem
    /// </summary>
    [DataMember(Order = 3, Name = "status"), JsonPropertyName("status"), JsonPropertyOrder(3), YamlMember(Alias = "status", Order = 3)]
    public required virtual ushort Status { get; set; }

    /// <summary>
    /// Gets/sets a human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [DataMember(Order = 4, Name = "detail"), JsonPropertyName("detail"), JsonPropertyOrder(4), YamlMember(Alias = "detail", Order = 4)]
    public virtual string? Detail { get; set; }

    /// <summary>
    /// Gets/sets a <see cref="Uri"/> reference that identifies the specific occurrence of the problem. It may or may not yield further information if dereferenced.
    /// </summary>
    [DataMember(Order = 5, Name = "instance"), JsonPropertyName("instance"), JsonPropertyOrder(5), YamlMember(Alias = "instance", Order = 5)]
    public virtual Uri? Instance { get; set; }

    /// <summary>
    /// Gets/sets a mapping containing problem details extension data, if any
    /// </summary>
    [DataMember(Name = "extensionData", Order = 6), JsonExtensionData]
    public virtual IDictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    /// Creates a new communication <see cref="Error"/>
    /// </summary>
    /// <param name="instance">The <see cref="Error"/> source</param>
    /// <param name="status">The <see cref="Error"/>'s status</param>
    /// <param name="detail">The <see cref="Error"/> detail, if any</param>
    /// <returns>A new communication <see cref="Error"/></returns>
    public static Error Communication(Uri instance, ushort status = ErrorStatus.Communication, string? detail = null) => new()
    {
        Status = status,
        Type = ErrorType.Communication,
        Title = ErrorTitle.Communication,
        Detail = detail,
        Instance = instance
    };

    /// <summary>
    /// Creates a new communication <see cref="Error"/>
    /// </summary>
    /// <param name="instance">The <see cref="Error"/> source</param>
    /// <param name="detail">The <see cref="Error"/> detail, if any</param>
    /// <returns>A new communication <see cref="Error"/></returns>
    public static Error Configuration(Uri instance, string? detail = null) => new()
    {
        Status = ErrorStatus.Configuration,
        Type = ErrorType.Configuration,
        Title = ErrorTitle.Configuration,
        Detail = detail,
        Instance = instance
    };

    /// <summary>
    /// Creates a new runtime <see cref="Error"/>
    /// </summary>
    /// <param name="instance">The <see cref="Error"/> source</param>
    /// <param name="detail">The <see cref="Error"/> detail, if any</param>
    /// <returns>A new communication <see cref="Error"/></returns>
    public static Error Runtime(Uri instance, string? detail = null) => new()
    {
        Status = ErrorStatus.Runtime,
        Type = ErrorType.Runtime,
        Title = ErrorTitle.Runtime,
        Detail = detail,
        Instance = instance
    };

}