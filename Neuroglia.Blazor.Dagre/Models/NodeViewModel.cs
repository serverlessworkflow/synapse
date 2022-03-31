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

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? RadiusX { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? RadiusY { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? PaddingX { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? PaddingY { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual Guid? ParentId { get; set; }

        public NodeViewModel()
            : this("", Consts.NodeWidth, Consts.NodeHeight, 0 , 0, Consts.NodeRadius, Consts.NodeRadius, Consts.NodePadding, Consts.NodePadding)
        { }

        public NodeViewModel(
            string label = "",
            double width = Consts.NodeWidth, 
            double height = Consts.NodeHeight, 
            double x = 0, 
            double y = 0, 
            double radiusX = Consts.NodeRadius,
            double radiusY = Consts.NodeRadius,
            double paddingX = Consts.NodePadding, 
            double paddingY = Consts.NodePadding
        )
            : base()
        {
            this.Label = label;
            this.Width = width;
            this.Height = height;
            this.X = x;
            this.Y = y;
            this.RadiusX = radiusX;
            this.RadiusY = radiusY;
            this.PaddingX = paddingX;
            this.PaddingY = paddingY;
        }
    }
}
