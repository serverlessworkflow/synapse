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

using Synapse.Resources;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Exposes the default breadcrumbs for all pages of the application
/// </summary>
public static class Breadcrumbs
{
    /// <summary>
    /// Holds the breadcrumb items for <see cref="Workflow"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Workflows = [new("Workflows", "/workflows")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="WorkflowInstance"/> related routes
    /// </summary>
    public static BreadcrumbItem[] WorkflowInstances = [new("Workflows Instances", "/workflow-instances")];
    /// <summary>
    /// Holds the breadcrumb items for the user profile related routes
    /// </summary>
    public static BreadcrumbItem[] UserProfile = [new("User Profile", "/users/profile")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="Operator"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Operators = [new("Operators", "/operators")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="Namespace"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Namespaces = [new("Namespaces", "/operators")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="CustomFunction"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Functions = [new("Functions", "/functions")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="Correlator"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Correlators = [new("Correlators", "/correlators")];
    /// <summary>
    /// Holds the breadcrumb items for <see cref="Correlation"/> related routes
    /// </summary>
    public static BreadcrumbItem[] Correlations = [new("Correlations", "/correlations")];
    /// <summary>
    /// Holds the breadcrumb items for about related routes
    /// </summary>
    public static BreadcrumbItem[] About = [new("About", "/about")];

}
