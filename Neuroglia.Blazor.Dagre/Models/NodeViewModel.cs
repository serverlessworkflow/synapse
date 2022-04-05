namespace Neuroglia.Blazor.Dagre.Models
{
    public class NodeViewModel
        : GraphElement, INodeViewModel
    {
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? X { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Y { get; set; }

        protected double? _width { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width
        {
            get => this._width;
            set
            {
                this._width = value;
                this.UpdateBBox();
            }
        }

        protected double? _height { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.UpdateBBox();
            }
        }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? RadiusX { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? RadiusY { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual Guid? ParentId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Shape { get; set; }

        protected IBoundingBox _bbox { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual IBoundingBox? BBox => this._bbox;

        public virtual event Action? Changed;

        public NodeViewModel()
            : this("", null, null, Consts.NodeWidth, Consts.NodeHeight, Consts.NodeRadius, Consts.NodeRadius, 0, 0, null, null)
        { }

        public NodeViewModel(
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.NodeWidth, 
            double? height = Consts.NodeHeight,
            double? radiusX = Consts.NodeRadius,
            double? radiusY = Consts.NodeRadius,
            double? x = 0,
            double? y = 0,
            Type? componentType = null,
            Guid? parentId = null
        )
            : base(label, cssClass, componentType)
        {
            this.Label = label;
            this.CssClass = cssClass;
            this.Shape = shape;
            this.Width = width ?? 0;
            this.Height = height ?? 0;
            this.X = x;
            this.Y = y;
            this.RadiusX = radiusX ?? 0;
            this.RadiusY = radiusY ?? 0;
            this._bbox = new BoundingBox();
            this.ParentId = parentId;
            this.UpdateBBox();
        }


        public virtual void Move(double deltaX, double deltaY)
        {
            if (deltaX == 0 && deltaY == 0) 
                return;
            this.X += deltaX;
            this.Y += deltaY;
            this.Changed?.Invoke();
        }

        protected virtual void UpdateBBox()
        {
            switch (this.Shape)
            {
                case NodeShape.Circle:
                case NodeShape.Ellipse:
                    this._bbox = new BoundingBox(this.Width / 2, this.Height / 2, 0, 0);
                    break;
                default:
                    this._bbox = new BoundingBox(this.Width, this.Height, 0 - this.Width / 2, 0 - this.Height / 2);
                    break;

            }
            this.Changed?.Invoke();
        }
    
    }
}
