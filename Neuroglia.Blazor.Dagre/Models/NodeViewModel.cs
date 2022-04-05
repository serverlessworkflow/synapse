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

        protected double? _paddingX { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? PaddingX
        {
            get => this._paddingX;
            set
            {
                this._paddingX = value;
                this.UpdateBBox();
            }
        }

        protected double? _paddingY { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? PaddingY
        {
            get => this._paddingY;
            set
            {
                this._paddingY = value;
                this.UpdateBBox();
            }
        }

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

        public NodeViewModel()
            : this("", null, null, Consts.NodeWidth, Consts.NodeHeight, 0 , 0, Consts.NodeRadius, Consts.NodeRadius, Consts.NodePadding, Consts.NodePadding, null, null)
        { }

        public NodeViewModel(
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.NodeWidth, 
            double? height = Consts.NodeHeight,
            double? x = 0, 
            double? y = 0, 
            double? radiusX = Consts.NodeRadius,
            double? radiusY = Consts.NodeRadius,
            double? paddingX = Consts.NodePadding, 
            double? paddingY = Consts.NodePadding,
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
            this.PaddingX = paddingX ?? 0;
            this.PaddingY = paddingY ?? 0;
            this._bbox = new BoundingBox();
            this.ParentId = parentId;
            this.UpdateBBox();
        }

        protected virtual void UpdateBBox()
        {
            switch (this.Shape)
            {
                case NodeShape.Circle:
                case NodeShape.Ellipse:
                    this._bbox = new BoundingBox((this.Width + this.PaddingX + this.PaddingY) / 2, (this.Height + this.PaddingX + this.PaddingY) / 2, 0, 0);
                    break;
                default:
                    var rectWidth = this.Width + this.PaddingX * 2;
                    var rectHeight = this.Height + this.PaddingY * 2;
                    this._bbox = new BoundingBox(rectWidth, rectHeight, 0 - rectWidth / 2, 0 - rectHeight / 2);
                    break;

            }
        }
    }
}
