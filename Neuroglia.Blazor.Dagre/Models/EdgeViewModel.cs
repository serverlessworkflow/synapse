using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class EdgeViewModel
        : GraphElement, IEdgeViewModel
    {
        public virtual Guid SourceId { get; set; }
        public virtual Guid TargetId { get; set; }
        public virtual ICollection<IPosition> Points { get; set; }

        public EdgeViewModel(Guid sourceId, Guid targetId, ICollection<IPosition>? points = null)
            : base()
        {
            this.SourceId = sourceId;
            this.TargetId = targetId;
            this.Points = points ?? new Collection<IPosition>();
        }
    }
}
