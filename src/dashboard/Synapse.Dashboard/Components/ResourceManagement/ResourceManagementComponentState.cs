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
/// Represents the state of a resource management component
/// </summary>
/// <typeparam name="TResource">The type of managed <see cref="IResource"/>s</typeparam>
public record ResourceManagementComponentState<TResource>
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets/sets the definition of the managed resource type
    /// </summary>
    public ResourceDefinition? Definition { get; set; }

    /// <summary>
    /// Gets/sets a <see cref="List{T}"/> that contains all <see cref="IResource"/>s
    /// </summary>
    public EquatableList<TResource>? Resources { get; set; } = [];

    /// <summary>
    /// Gets/sets a <see cref="List{T}"/> that contains all selected <see cref="IResource"/>s
    /// </summary>
    public EquatableList<string> SelectedResourceNames { get; set; } = [];

    /// <summary>
    /// Gets/sets a list that contains the label selectors, if any, used to filter the resources to list
    /// </summary>
    public EquatableList<LabelSelector>? LabelSelectors { get; set; } = [];

    /// <summary>
    /// Gets/sets the search term, if any, used to filter the resources to list
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets/sets the maximum number of results to return when listing resources. Defaults to 25.
    /// </summary>
    public ulong MaxResults { get; set; } = 25;

    /// <summary>
    /// Gets/sets the token used to continue listing resources when there are more results than the specified <see cref="MaxResults"/>
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets/sets a boolean value that indicates whether data is currently being gathered
    /// </summary>
    public bool Loading { get; set; } = true;

    /// <summary>
    /// Gets/sets the name of the selected resource
    /// </summary>
    public string ActiveResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets a <see cref="ResourcesFilter"/> used to filter the resources to list
    /// </summary>
    public ResourcesFilter Filter { get; set; } = new ResourcesFilter();

}
