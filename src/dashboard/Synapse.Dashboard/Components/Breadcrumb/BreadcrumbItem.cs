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

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents a breadcrumb element
/// </summary>
/// <remarks>
/// Initializes a new <see cref="BreadcrumbItem"/> with the provided data
/// </remarks>
public class BreadcrumbItem
{

    /// <summary>
    /// Initializes a new <see cref="BreadcrumbItem"/> with the provided data
    /// </summary>
    /// <param name="label">The breadcrumb's label</param>
    /// <param name="link">The link associated to the breadcrumb</param>
    /// <param name="icon">The breadcrumb's icon, if any</param>
    public BreadcrumbItem(string label, string link, string? icon = null)
    {
        Label = label;
        Link = link;
        Icon = icon;
    }

    /// <summary>
    /// Initializes a new <see cref="BreadcrumbItem"/> with the provided template
    /// </summary>
    /// <param name="template">The breadcrumb's template</param>
    public BreadcrumbItem(RenderFragment template)
    {
        Template = template;
    }

    /// <summary>
    /// Gets the breadcrumb's label, if any
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the link associated to the breadcrumb, if any
    /// </summary>
    public string? Link { get; }

    /// <summary>
    /// Gets the breadcrumb's icon, if any
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// Get the breadcrumb's <see cref="RenderFragment"/>, if any
    /// </summary>
    public RenderFragment? Template { get; }

}