using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard
{
    public class DragAndDropNodeBehavior
        : GraphBehavior
    {
        protected double _previousX = 0;
        protected double _previousY = 0;
        protected double _movementX = 0;
        protected double _movementY = 0;
        protected readonly IJSRuntime _jSRuntime = null!;
        protected IJSInProcessObjectReference? _nodeGhostModule = null;
        protected IJSObjectReference? _nodeGhost = null;
        protected INodeViewModel? _source = null;
        protected INodeViewModel? _destination = null;


        public DragAndDropNodeBehavior(IGraphViewModel graph, IJSRuntime jSRuntime)
            : base(graph)
        {
            this._jSRuntime = jSRuntime;
            this.Graph.MouseEnter += this.OnMouseEnterAsync;
            this.Graph.MouseLeave += this.OnMouseLeaveAsync;
            this.Graph.MouseMove += this.OnMouseMoveAsync;
            this.Graph.MouseDown += this.OnMouseDownAsync;
            this.Graph.MouseUp += this.OnMouseUpAsync;
        }

        protected virtual async Task OnMouseEnterAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (element == null || element is not INodeViewModel || element is StartNodeViewModel || element is EndNodeViewModel  || this._source == null || this._source == element)
                return;
            this._destination = element as INodeViewModel;
            this._destination.CssClass = (this._destination.CssClass ?? "") + " drop-destination";
        }

        protected virtual async Task OnMouseLeaveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (element == null || element is not INodeViewModel || element is StartNodeViewModel || element is EndNodeViewModel || this._destination != element)
                return;
            this._destination.CssClass = (this._destination.CssClass ?? "").Replace(" drop-destination", "");
            this._destination = null;
        }

        protected virtual async Task OnMouseMoveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this._nodeGhost == null)
                return;
            this._movementX = e.ClientX - this._previousX;
            this._movementY = e.ClientY - this._previousY;
            await this.UpdatedPosition();
        }

        protected virtual async Task OnMouseDownAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (element == null || element is not INodeViewModel)
                return;
            if (element is StartNodeViewModel || element is EndNodeViewModel)
                return;
            if (this._nodeGhostModule == null)
            {
                this._nodeGhostModule = await _jSRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "./js/node-ghost.js");
            }
            if (this._nodeGhostModule == null)
            {
                throw new NullReferenceException("Unable to load JS module './js/node-ghost.js'");
            }
            this._nodeGhost = await this._nodeGhostModule.InvokeAsync<IJSObjectReference>("createNodeGhost", sender);
            if (this._nodeGhost == null)
            {
                throw new NullReferenceException("Unable to create new ghost node");
            }
            this._source = element as INodeViewModel;
            this._previousX = e.ClientX;
            this._previousY = e.ClientY;
        }

        protected virtual async Task OnMouseUpAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this._nodeGhost == null)
                return;
            await this.UpdatedPosition();
            await this._nodeGhost.InvokeVoidAsync("dispose");
            this._nodeGhost = null;
            this._source = null;
            if (this._destination != null)
            {
                this._destination.CssClass = (this._destination.CssClass ?? "").Replace(" drop-destination", "");
                this._destination = null;
            }
        }

        protected virtual async Task UpdatedPosition()
        {
            if (this._nodeGhost == null)
                return;
            if (this._movementX == 0 && this._movementY == 0)
                return;
            await this._nodeGhost.InvokeVoidAsync("move", this._movementX / (double)this.Graph.Scale, this._movementY / (double)this.Graph.Scale);
            this._previousX += this._movementX;
            this._previousY += this._movementY;
            this._movementX = 0;
            this._movementY = 0;
        }

        public override void Dispose()
        {
            this.Graph.MouseEnter -= this.OnMouseEnterAsync;
            this.Graph.MouseLeave -= this.OnMouseLeaveAsync;
            this.Graph.MouseMove -= this.OnMouseMoveAsync;
            this.Graph.MouseDown -= this.OnMouseDownAsync;
            this.Graph.MouseUp -= this.OnMouseUpAsync;
        }
    }
}
