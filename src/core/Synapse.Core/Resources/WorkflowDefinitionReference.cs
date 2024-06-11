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
/// Represents a reference to a <see cref="WorkflowDefinition"/>
/// </summary>
[DataContract]
public record WorkflowDefinitionReference
{

    /// <summary>
    /// Gets/sets the name of the referenced workflow definition
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "name", Order = 1), JsonPropertyName("name"), JsonPropertyOrder(1), YamlMember(Alias = "name", Order = 1)]
    public required virtual string Name { get; set; }

    /// <summary>
    /// Gets/sets the namespace of the referenced workflow definition
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "namespace", Order = 2), JsonPropertyName("namespace"), JsonPropertyOrder(2), YamlMember(Alias = "namespace", Order = 2)]
    public required virtual string Namespace { get; set; }

    /// <summary>
    /// Gets/sets the semantic version of the referenced workflow definition
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "version", Order = 3), JsonPropertyName("version"), JsonPropertyOrder(3), YamlMember(Alias = "version", Order = 3)]
    public required virtual string Version { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{this.Namespace}.{this.Name}:{this.Version}";

    /// <summary>
    /// Parses the specified input into a new <see cref="WorkflowDefinitionReference"/>
    /// </summary>
    /// <param name="input">The input to parse</param>
    /// <returns>The parsed <see cref="WorkflowDefinitionReference"/></returns>
    public static WorkflowDefinitionReference Parse(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var components = input.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
        var qualifiedName = components[0];
        var version = components[1];
        components = qualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var @namespace = components[0];
        var name = components[1];
        return new()
        {
            Name = name,
            Namespace = @namespace,
            Version = version
        };
    }

    /// <summary>
    /// Attempts to parse the specified input into a new <see cref="WorkflowDefinitionReference"/>
    /// </summary>
    /// <param name="input">The input to parse</param>
    /// <param name="reference">The The parsed <see cref="WorkflowDefinitionReference"/>, if any</param>
    /// <returns>A boolean indicating whether or not the specified input could be parsed into a new <see cref="WorkflowDefinitionReference"/></returns>
    public static bool TryParse(string input, out WorkflowDefinitionReference? reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        reference = null;
        try
        {
            reference = Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts the specified reference into string
    /// </summary>
    /// <param name="reference">The reference to convert</param>
    public static implicit operator string(WorkflowDefinitionReference reference) => reference.ToString();

    /// <summary>
    /// Implicitly parses the specified string into a new reference 
    /// </summary>
    /// <param name="reference">The string to parse</param>
    public static implicit operator WorkflowDefinitionReference(string reference) => Parse(reference);

}