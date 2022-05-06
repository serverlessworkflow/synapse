using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class EdgeViewModel
        : GraphElement, IEdgeViewModel
    {
        public virtual Guid SourceId { get; set; }
        public virtual Guid TargetId { get; set; }

        protected ICollection<IPosition> _points { get; set; }
        public virtual ICollection<IPosition> Points { 
            get => this._points; 
            set
            {
                this._points = value;
                var minX = this.Points.Min(p => p.X);
                var maxX = this.Points.Max(p => p.X);
                var minY = this.Points.Min(p => p.Y);
                var maxY = this.Points.Max(p => p.Y);
                var width = maxX - minX;
                var height = maxY - minY;
                var x = minX + width / 2;
                var y = minY + height / 2;
                this.BBox = new BoundingBox(width, height, x, y);
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("labelpos")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "labelpos", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? LabelPosition { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? LabelOffset { get; set; }

        public virtual string Shape { get; set; }
        public virtual string? StartMarkerId { get; set; }
        public virtual string? EndMarkerId { get; set; }
        public virtual IBoundingBox BBox { get; set; }

        public EdgeViewModel(Guid sourceId, Guid targetId, string? label = null, string? cssClass = null, string? shape = null, ICollection<IPosition>? points = null, Type? componentType = null)
            : base(label, cssClass, componentType)
        {
            this.SourceId = sourceId;
            this.TargetId = targetId;
            this._points = points ?? new Collection<IPosition>();
            this.Shape = shape ?? EdgeShape.BSpline;
            this.EndMarkerId = Constants.EdgeEndArrowId;
            this.BBox = new BoundingBox();
        }
    }
}
