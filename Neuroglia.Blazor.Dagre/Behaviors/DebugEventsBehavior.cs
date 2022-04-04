using Microsoft.AspNetCore.Components.Web;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre.Behaviors
{
    public class DebugEventsBehavior
        : GraphBehavior
    {
        public DebugEventsBehavior(IGraphViewModel graph)
            : base(graph)
        {
            //this.Graph.MouseMove += this.OnMouseMove;
            this.Graph.MouseDown += this.OnMouseDown;
            this.Graph.MouseUp += this.OnMouseUp;
        }

        protected virtual void OnMouseMove(IGraphElement? element, MouseEventArgs e)
        {
            Console.WriteLine("Mouse move - " + Newtonsoft.Json.JsonConvert.SerializeObject(element));
        }

        protected virtual void OnMouseDown(IGraphElement? element, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - " + Newtonsoft.Json.JsonConvert.SerializeObject(element));
        }

        protected virtual void OnMouseUp(IGraphElement? element, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - " + Newtonsoft.Json.JsonConvert.SerializeObject(element));
        }

        public override void Dispose()
        {
            //this.Graph.MouseMove -= this.OnMouseMove;
            this.Graph.MouseDown -= this.OnMouseDown;
            this.Graph.MouseUp -= this.OnMouseUp;
        }
    }
}
