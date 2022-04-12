using Neuroglia.Blazor.Dagre;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="NodeViewModel"/> containing a label
    /// </summary>
    public class LabeledNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="LabeledNodeViewModel"/>
        /// </summary>
        public LabeledNodeViewModel(
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.NodeWidth * 2,
            double? height = Consts.NodeHeight,
            double? radiusX = Consts.NodeRadius,
            double? radiusY = Consts.NodeRadius,
            double? x = 0,
            double? y = 0,
            Type? componentType = null,
            Guid? parentId = null
        )
            : base(label, cssClass, shape, width, height, radiusX, radiusY, x, y, componentType, parentId)
        {}

    }

}
