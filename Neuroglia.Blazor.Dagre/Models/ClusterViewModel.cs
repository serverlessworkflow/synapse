using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class ClusterViewModel
        : NodeViewModel, IClusterViewModel
    {
        public virtual IDictionary<Guid, INodeViewModel> Children { get; set; }

        public ClusterViewModel(
            IDictionary<Guid, INodeViewModel>? children,
            string label = "",
            double width = Consts.ClusterWidth,
            double height = Consts.ClusterHeight,
            double x = 0,
            double y = 0,
            double radius = Consts.ClusterRadius,
            double paddingX = Consts.ClusterPadding,
            double paddingY = Consts.ClusterPadding
        )
            : base(label, width, height, x, y, radius, paddingX, paddingY)
        {
            this.Children = children ?? new Dictionary<Guid, INodeViewModel>();
        }
    }
}
