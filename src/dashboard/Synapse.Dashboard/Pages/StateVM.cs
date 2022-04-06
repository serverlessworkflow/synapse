using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard
{
    public class StateVM
        : ClusterViewModel
    {
        public StateVM(
            Dictionary<Guid, INodeViewModel>? children = null,
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.ClusterWidth,
            double? height = Consts.ClusterHeight,
            double? radiusX = Consts.ClusterRadius,
            double? radiusY = Consts.ClusterRadius,
            double? x = 0,
            double? y = 0,
            Guid? parentId = null
        )
            : base(children, label, cssClass, shape, width, height, radiusX, radiusY, x, y, typeof(Pages.StateTemplate), parentId)
        { 
        
        }
    }
}
