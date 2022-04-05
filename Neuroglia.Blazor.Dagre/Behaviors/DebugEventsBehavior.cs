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
            this.Graph.Wheel += this.OnWheel;
        }

        protected virtual void OnMouseMove(IGraphElement? element, MouseEventArgs e)
        {
            this.Print("Mouse move", element, e);
        }

        protected virtual void OnMouseDown(IGraphElement? element, MouseEventArgs e)
        {
            this.Print("Mouse down", element, e);
        }

        protected virtual void OnMouseUp(IGraphElement? element, MouseEventArgs e)
        {
            this.Print("Mouse up", element, e);
        }

        protected virtual void OnWheel(IGraphElement? element, WheelEventArgs e)
        {
            this.Print("Wheel", element, e);
        }

        protected virtual void Print(string type, IGraphElement? element, EventArgs e)
        {
            Console.WriteLine(type);
            Console.WriteLine(type + " - element - " + Newtonsoft.Json.JsonConvert.SerializeObject(element));
            Console.WriteLine(type + " - event - " + Newtonsoft.Json.JsonConvert.SerializeObject(e));
        }

        public override void Dispose()
        {
            //this.Graph.MouseMove -= this.OnMouseMove;
            this.Graph.MouseDown -= this.OnMouseDown;
            this.Graph.MouseUp -= this.OnMouseUp;
            this.Graph.Wheel -= this.OnWheel;
        }
    }
}
