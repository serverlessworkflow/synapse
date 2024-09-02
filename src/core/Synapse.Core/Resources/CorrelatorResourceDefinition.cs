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

using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Serialization.Yaml;

namespace Synapse.Resources;

/// <summary>
/// Represents the definition of an <see cref="Correlator"/> <see cref="Resource"/>
/// </summary>
[DataContract]
public record CorrelatorResourceDefinition
    : ResourceDefinition
{

    /// <summary>
    /// Gets the definition of <see cref="ResourceDefinition"/>s
    /// </summary>
    public static new ResourceDefinition Instance { get; set; }
    /// <summary>
    /// Gets/sets the group resource definitions belong to
    /// </summary>
    public static new string ResourceGroup { get; set; }
    /// <summary>
    /// Gets/sets the resource version of resource definitions
    /// </summary>
    public static new string ResourceVersion { get; set; }
    /// <summary>
    /// Gets/sets the plural name of resource definitions
    /// </summary>
    public static new string ResourcePlural { get; set; }
    /// <summary>
    /// Gets/sets the kind of resource definitions
    /// </summary>
    public static new string ResourceKind { get; set; }

    static CorrelatorResourceDefinition()
    {
        using var stream = typeof(Workflow).Assembly.GetManifestResourceStream($"{typeof(Correlator).Namespace}.{nameof(Correlator)}.yaml")!;
        using var streamReader = new StreamReader(stream);
        Instance = YamlSerializer.Default.Deserialize<ResourceDefinition>(streamReader.ReadToEnd())!;
        ResourceGroup = Instance.Spec.Group;
        ResourceVersion = Instance.Spec.Versions.Last().Name;
        ResourcePlural = Instance.Spec.Names.Plural;
        ResourceKind = Instance.Spec.Names.Kind;
    }

    /// <summary>
    /// Initializes a new <see cref="CorrelatorResourceDefinition"/>
    /// </summary>
    public CorrelatorResourceDefinition() : base(Instance) { }

}
