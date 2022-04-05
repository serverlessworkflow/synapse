using Microsoft.AspNetCore.Components.Web;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre.Behaviors
{
    internal class PanBahavior
        : GraphBehavior
    {
        protected bool _enableMovement = false;
        protected double _previousX = 0;
        protected double _previousY = 0;
        protected double _movementX = 0;
        protected double _movementY = 0;

        public PanBahavior(IGraphViewModel graph)
            : base(graph)
        {
            this.Graph.MouseMove += this.OnMouseMove;
            this.Graph.MouseDown += this.OnMouseDown;
            this.Graph.MouseUp += this.OnMouseUp;
        }

        protected virtual void OnMouseMove(IGraphElement? element, MouseEventArgs e)
        {
            if (!this._enableMovement)
                return;
            this._movementX = e.ClientX - this._previousX;
            this._movementY = e.ClientY - this._previousY;
            this.UpdatedPosition();
        }

        protected virtual void OnMouseDown(IGraphElement? element, MouseEventArgs e)
        {
            if (element != null)
                return;
            this._enableMovement = true;
            this._previousX = e.ClientX;
            this._previousY = e.ClientY;
        }

        protected virtual void OnMouseUp(IGraphElement? element, MouseEventArgs e)
        {
            this._enableMovement = false;
            if (element != null)
                return;
            this.UpdatedPosition();
        }

        protected virtual void UpdatedPosition()
        {
            if (this._movementX != 0 || this._movementY != 0) { 
                this.Graph.X += this._movementX;
                this.Graph.Y += this._movementY;
                this._previousX += this._movementX;
                this._previousY += this._movementY;
                this._movementX = 0;
                this._movementY = 0;
            }
        }

        public override void Dispose()
        {
            this.Graph.MouseMove -= this.OnMouseMove;
            this.Graph.MouseDown -= this.OnMouseDown;
            this.Graph.MouseUp -= this.OnMouseUp;
        }
    }
}
