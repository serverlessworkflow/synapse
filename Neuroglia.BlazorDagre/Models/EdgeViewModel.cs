namespace Neuroglia.BlazorDagre.Models
{
    public class EdgeViewModel
        : BaseViewModel, IEdgeViewModel
    {
        public virtual Guid SourceId { get; set; }
        public virtual Guid TargetId { get; set; }

        public EdgeViewModel(Guid sourceId, Guid targetId)
        {
            this.SourceId = sourceId;
            this.TargetId = targetId;
        }
    }
}
