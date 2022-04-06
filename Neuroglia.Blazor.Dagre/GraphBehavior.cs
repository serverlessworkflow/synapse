using Neuroglia.Blazor.Dagre.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroglia.Blazor.Dagre
{
    public abstract class GraphBehavior
        : IDisposable
    {
        public GraphBehavior(IGraphViewModel graph)
        {
            this.Graph = graph;
        }

        protected IGraphViewModel Graph { get; }

        public abstract void Dispose();
    }
}
