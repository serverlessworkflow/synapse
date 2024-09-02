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

using System.ComponentModel;

namespace Synapse.Dashboard.Services;

/// <summary>
/// The service used to manage the breadcrumb
/// </summary>
public interface IBreadcrumbManager
{

    /// <summary>
    /// The event fired whenever the breadcrumb collection has changed
    /// </summary>
    event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets a list containing all <see cref="BreadcrumbItem"/>s
    /// </summary>
    IReadOnlyCollection<Components.BreadcrumbItem> Items { get; }

    /// <summary>
    /// Adds the specified <see cref="BreadcrumbItem"/> to the list
    /// </summary>
    /// <param name="breadcrumbItem">The breadcrumb item to add</param>
    /// <returns>The added <see cref="BreadcrumbItem"/></returns>
    Components.BreadcrumbItem Add(Components.BreadcrumbItem breadcrumbItem);

    /// <summary>
    /// Creates a new <see cref="BreadcrumbItem"/> with the specified label and icon for the active route and adds it to the list
    /// </summary>
    /// <param name="label">The label of the breadcrumb item to add</param>
    /// <param name="link">The link associated to the breadcrumb item to add</param>
    /// <param name="icon">The icon, if any, of the breadcrumb item to add</param>
    /// <returns>The added <see cref="BreadcrumbItem"/></returns>
    Components.BreadcrumbItem Add(string label, string link, string? icon = null);

    /// <summary>
    /// Removes the specified <see cref="BreadcrumbItem"/> from the list
    /// </summary>
    /// <param name="breadcrumbItem">The <see cref="BreadcrumbItem"/> to remove</param>
    void Remove(Components.BreadcrumbItem breadcrumbItem);

    /// <summary>
    /// Clears all <see cref="BreadcrumbItem"/>s
    /// </summary>
    void Clear();

    /// <summary>
    /// Replaces the current <see cref="BreadcrumbItem"/>s list with the provided one
    /// </summary>
    /// <param name="breadcrumbs">The <see cref="BreadcrumbItem"/>s to use</param>
    void Use(params Components.BreadcrumbItem[] breadcrumbs);

    /// <summary>
    /// Navigate to the provided item and set the breadcrumb state accordingly
    /// </summary>
    /// <param name="breadcrumbItem">The <see cref="BreadcrumbItem"/> to navigate to</param>
    void NavigateTo(Components.BreadcrumbItem breadcrumbItem);

}
