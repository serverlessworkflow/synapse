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
            string? shape = null,
            double? width = Consts.ClusterWidth,
            double? height = Consts.ClusterHeight,
            double? x = 0,
            double? y = 0,
            double? radiusX = Consts.ClusterRadius,
            double? radiusY = Consts.ClusterRadius,
            double? paddingX = Consts.ClusterPadding,
            double? paddingY = Consts.ClusterPadding,
            Guid? parentId = null
        )
            : base(children, label, shape, width, height, x, y, radiusX, radiusY, paddingX, paddingY, typeof(Pages.StateTemplate), parentId)
        { 
        
        }
    }
}
