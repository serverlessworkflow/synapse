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

namespace Synapse.Dashboard.Components.ResourceManagement;

/// <summary>
/// Holds filters used to request resources
/// </summary>
public record ResourcesFilter
{

    /// <summary>
    /// Gets the <see cref="Neuroglia.Data.Infrastructure.ResourceOriented.Namespace"/>, if any, the (namespaced) resources to list belong to
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets/sets a list that contains the label selectors, if any, used to filter the resources to list
    /// </summary>
    public EquatableList<LabelSelector>? LabelSelectors { get; set; }

}
