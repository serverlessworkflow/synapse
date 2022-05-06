using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre.Behaviors
{
    internal class MoveNodeBehavior
        : GraphBehavior
    {
        protected double _previousX = 0;
        protected double _previousY = 0;
        protected double _movementX = 0;
        protected double _movementY = 0;
        protected INodeViewModel? _target = null;

        public MoveNodeBehavior(IGraphViewModel graph)
            : base(graph)
        {
            this.Graph.MouseMove += this.OnMouseMoveAsync;
            this.Graph.MouseDown += this.OnMouseDownAsync;
            this.Graph.MouseUp += this.OnMouseUpAsync;
        }

        protected virtual async Task OnMouseMoveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this._target == null)
                return;
            this._movementX = e.ClientX - this._previousX;
            this._movementY = e.ClientY - this._previousY;
            this.UpdatedPosition();
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseDownAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (element == null || element is not INodeViewModel)
                return;
            this._target = (INodeViewModel)element;
            this._previousX = e.ClientX;
            this._previousY = e.ClientY;
            await Task.CompletedTask;
        }

        protected virtual async Task OnMouseUpAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            this._target = null;
            this.UpdatedPosition();
            await Task.CompletedTask;
        }

        protected virtual void UpdatedPosition()
        {
            if (this._target == null)
                return;
            if (this._movementX == 0 && this._movementY == 0)
                return;
            this._target.Move(this._movementX / (double)this.Graph.Scale, this._movementY / (double)this.Graph.Scale);
            this._previousX += this._movementX;
            this._previousY += this._movementY;
            this._movementX = 0;
            this._movementY = 0;
        }

        public override void Dispose()
        {
            this.Graph.MouseMove -= this.OnMouseMoveAsync;
            this.Graph.MouseDown -= this.OnMouseDownAsync;
            this.Graph.MouseUp -= this.OnMouseUpAsync;
        }
    }
}
