using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    /// <summary>
    /// Wraps Dagre js library as an injectable service
    /// https://github.com/dagrejs/dagre
    /// </summary>
    public class DagreService
        : IGraphLibJsonConverter
    {
        /// <summary>
        /// The JS Runtime instance
        /// </summary>
        protected readonly IJSRuntime jsRuntime;

        /// <summary>
        /// Creates a new instance of DagreService
        /// </summary>
        /// <param name="jSRuntime"></param>
        public DagreService(IJSRuntime jSRuntime) { 
            this.jsRuntime = jSRuntime; 
        }

        /// <inheritdoc/>
        public virtual async Task<IGraphLib> Deserialize(string json) => await this.jsRuntime.InvokeAsync<IGraphLib>("dagre.graphlib.json.read", json);

        /// <summary>
        /// Returns a new <see cref="IGraphLib"/> instance
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Graph(IGraphLibOptions? options = null)
        {
            var jsGraph = await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.graph", options);
            return new GraphLib(jsGraph);
        }

        /// <summary>
        /// Computes the graph layout
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Layout(IGraphLib graph) => await this.jsRuntime.InvokeAsync<IGraphLib>("neuroglia.blazor.dagre.layout", graph);
        
        /// <inheritdoc/>
        public virtual async Task<string> Serialize(IGraphLib graph) => await this.jsRuntime.InvokeAsync<string>("dagre.graphlib.json.write", graph);
    }
}
