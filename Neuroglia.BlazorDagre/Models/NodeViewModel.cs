namespace Neuroglia.BlazorDagre.Models
{
    public class NodeViewModel
        : BaseViewModel, INodeViewModel
    {
        public virtual double X { get; set; }
        public virtual double Y { get; set; }
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }
        public virtual double Radius { get; set; }
        public virtual double PaddingX { get; set; }
        public virtual double PaddingY { get; set; }

        public NodeViewModel(
            double width = Consts.NodeWidth, 
            double height = Consts.NodeHeight, 
            double x = 0, 
            double y = 0, 
            double radius = Consts.NodeRadius, 
            double paddingX = Consts.NodePadding, 
            double paddingY = Consts.NodePadding
        )
            : base()
        {
            this.Width = width;
            this.Height = height;
            this.X = x;
            this.Y = y;
            this.Radius = radius;
            this.PaddingX = paddingX;
            this.PaddingY = paddingY;
        }
    }
}
