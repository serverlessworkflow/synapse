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

using Neuroglia.Blazor.Dagre;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Holds the parameters used to instanciate a graph node
/// </summary>
public class NodeViewModelConfig
{
    /// <summary>
    /// Gets/sets the label of the node
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Get/sets the label template, if any. If none is provided, the label is used instead.
    /// </summary>
    public RenderFragment? LabelTemplate { get; set; }

    /// <summary>
    /// Get/sets the id of the symbol of the node.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Gets/sets the additional css classes of the node
    /// </summary>
    public string CssClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the X coordinate of the node on the graph
    /// </summary>
    public double X { get; set; } = 0;

    /// <summary>
    /// Gets/sets the Y coordinate of the node on the graph
    /// </summary>
    public double Y { get; set; } = 0;

    /// <summary>
    /// Gets/sets the width of the node
    /// </summary>
    public double? Width { get; set; } = Constants.NodeWidth * 5;

    /// <summary>
    /// Gets/sets the height of the node
    /// </summary>
    public double? Height { get; set; } = Constants.NodeHeight * 2.5;

    /// <summary>
    /// Gets/sets the X radius of the node
    /// </summary>
    public double? RadiusX { get; set; } = Constants.NodeRadius * 2;

    /// <summary>
    /// Gets/sets the Y radius of the node
    /// </summary>
    public double? RadiusY { get; set; } = Constants.NodeRadius * 2;

    /// <summary>
    /// Gets/sets parent node id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets/sets the shape the node
    /// </summary>
    public string Shape { get; set; } = SynapseNodeShape.Cartouche;

    /// <summary>
    /// Gets/sets the type the component used to render the node
    /// </summary>
    public Type ComponentType { get; set; } = typeof(WorkflowNodeTemplate);
}
