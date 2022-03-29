using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;
using System.Dynamic;

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
        public virtual async Task<IGraphLib> Deserialize(string json) => await this.jsRuntime.InvokeAsync<IGraphLib>("neuroglia.blazor.dagre.read", json);

        /// <summary>
        /// Returns a new <see cref="IGraphLib"/> instance
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Graph(IDagreGraphOptions? options = null)
        {
            var graphLibOptions = new GraphLibOptions(options);
            if (graphLibOptions.Multigraph == null) graphLibOptions.Multigraph = true;
            if (graphLibOptions.Compound == null) graphLibOptions.Compound = true;
            var jsIntance = await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.graph", graphLibOptions);
            var graphLib = new GraphLib(jsIntance);
            await graphLib.SetGraph(new DagreGraphConfig(options));
            return graphLib;
        }

        /// <summary>
        /// Computes the graph layout
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Layout(IGraphLib graph) => await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.layout", await graph.Instance()) as IGraphLib;
        
        /// <inheritdoc/>
        public virtual async Task<string> Serialize(IGraphLib graph) => await this.jsRuntime.InvokeAsync<string>("neuroglia.blazor.dagre.write", await graph.Instance());
    }
}
