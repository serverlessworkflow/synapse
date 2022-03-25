using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    public class Dagre
        : IGraphLibJsonConverter
    {
        protected readonly IJSRuntime jsRuntime;

        public Dagre(IJSRuntime jSRuntime) { 
            this.jsRuntime = jSRuntime; 
        }

        public virtual async Task<IGraphLib> Deserialize(string json) => await this.jsRuntime.InvokeAsync<IGraphLib>("dagre.graphlib.json.read", json);

        public virtual async Task<IGraphLib> Graph(IGraphLibOptions? options = null)
        {
            var jsGraph = await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.graph", options);
            return new GraphLib(jsGraph);
        }

        public virtual async Task<IGraphLib> Layout(IGraphLib graph) => await this.jsRuntime.InvokeAsync<IGraphLib>("neuroglia.blazor.dagre.layout", graph);

        public virtual async Task<string> Serialize(IGraphLib graph) => await this.jsRuntime.InvokeAsync<string>("dagre.graphlib.json.write", graph);
    }
}
