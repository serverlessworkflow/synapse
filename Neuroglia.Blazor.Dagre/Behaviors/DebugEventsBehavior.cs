using Microsoft.AspNetCore.Components;
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
            //this.Graph.MouseMove += this.OnMouseMoveAsync;
            this.Graph.MouseEnter += this.OnMouseEnterAsync;
            this.Graph.MouseLeave += this.OnMouseLeaveAsync;
            this.Graph.MouseDown += this.OnMouseDownAsync;
            this.Graph.MouseUp += this.OnMouseUpAsync;
            this.Graph.Wheel += this.OnWheelAsync;
        }

        protected virtual async Task OnMouseEnterAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this.Print("Mouse enter", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseLeaveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this.Print("Mouse leave", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseMoveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this.Print("Mouse move", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseDownAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this.Print("Mouse down", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseUpAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this.Print("Mouse up", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual async Task OnWheelAsync(ElementReference sender, WheelEventArgs e, IGraphElement? element)
        {
            this.Print("Wheel", sender, e, element);
            await Task.CompletedTask;
        }

        protected virtual void Print(string type, ElementReference sender, EventArgs e, IGraphElement? element)
        {
            Console.WriteLine(type);
            Console.WriteLine(type + " - sender - " + Newtonsoft.Json.JsonConvert.SerializeObject(sender));
            Console.WriteLine(type + " - element - " + Newtonsoft.Json.JsonConvert.SerializeObject(element));
            Console.WriteLine(type + " - event - " + Newtonsoft.Json.JsonConvert.SerializeObject(e));
        }

        public override void Dispose()
        {
            //this.Graph.MouseMove -= this.OnMouseMoveAsync;
            this.Graph.MouseEnter -= this.OnMouseEnterAsync;
            this.Graph.MouseLeave -= this.OnMouseLeaveAsync;
            this.Graph.MouseDown -= this.OnMouseDownAsync;
            this.Graph.MouseUp -= this.OnMouseUpAsync;
            this.Graph.Wheel -= this.OnWheelAsync;
        }
    }
}
