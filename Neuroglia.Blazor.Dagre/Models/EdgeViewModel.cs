namespace Neuroglia.Blazor.Dagre.Models
{
    public class EdgeViewModel
        : GraphElement, IEdgeViewModel
    {
        public virtual Guid SourceId { get; set; }
        public virtual Guid TargetId { get; set; }

        public EdgeViewModel(Guid sourceId, Guid targetId)
            : base()
        {
            this.SourceId = sourceId;
            this.TargetId = targetId;
        }
    }
}
