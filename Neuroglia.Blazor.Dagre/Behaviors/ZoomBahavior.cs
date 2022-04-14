using Microsoft.AspNetCore.Components.Web;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre.Behaviors
{
    internal class ZoomBahavior
        : GraphBehavior
    {
        public ZoomBahavior(IGraphViewModel graph)
            : base(graph)
        {
            this.Graph.Wheel += this.OnWheel;
        }

        protected virtual void OnWheel(IGraphElement? element, WheelEventArgs e)
        {
            if (element != null)
                return;
            this.Graph.Scale += (decimal)(e.DeltaY / Math.Abs(e.DeltaY)) * -0.1M; ;
            this.Graph.Scale = Math.Clamp(this.Graph.Scale, Consts.MinScale, Consts.MaxScale);
        }

        public override void Dispose()
        {
            this.Graph.Wheel -= this.OnWheel;
        }
    }
}
